using System.Globalization;
using gc.caja.Models.Facturacion;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace gc.caja.Areas.Facturacion.Controllers;

public partial class ProductoFactController
{
    [HttpGet]
    public async Task<IActionResult> ObtenerTiposFacturaEmitida()
    {
        if (!VerificarAutenticacion(out _) || string.IsNullOrWhiteSpace(TokenCookie))
            return Unauthorized();
        try
        {
            var resultado = await _notaCreditoServicio.GetTipoComprobante("%", "VE", TokenCookie);
            if (resultado == null || !resultado.Ok)
                return Json(new { ok = false, mensaje = resultado?.Mensaje ?? "No se pudieron obtener los tipos de comprobante." });
            var datos = (resultado.ListaEntidad ?? [])
                .Where(x => !string.IsNullOrWhiteSpace(x.tco_id))
                .Select(x => new { tco_id = x.tco_id.Trim(), tco_desc = x.tco_desc?.Trim() ?? string.Empty })
                .ToList();
            return Json(new { ok = datos.Count > 0, datos, mensaje = datos.Count > 0 ? "OK" : "No hay tipos de comprobante disponibles." });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Factura emitida: error consultando tipos de comprobante.");
            return Json(new { ok = false, mensaje = "No se pudieron obtener los tipos de comprobante." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CargarFacturaEmitida([FromBody] FacturaEmitidaRequest? request)
    {
        if (!VerificarAutenticacion(out _) || string.IsNullOrWhiteSpace(TokenCookie))
            return Unauthorized();
        if (request == null)
            return Json(new { ok = false, mensaje = "Ingrese los datos de la factura emitida." });
        if (!request.TryNormalizar(out var tipo, out var comprobante, out var mensaje))
            return Json(new { ok = false, mensaje });

        var caja = CajaActual;
        var cliente = ClienteActual;
        if (caja?.Caja == null || string.IsNullOrWhiteSpace(caja.CajaId) || cliente == null)
            return Json(new { ok = false, mensaje = "Debe seleccionar un cliente y tener una caja activa." });
        if (string.IsNullOrWhiteSpace(LP_Id))
            return Json(new { ok = false, mensaje = "No hay una lista de precios activa para la operacion." });
        if ((ProductosSeleccionados ?? []).Any(x => !string.IsNullOrWhiteSpace(x.pre_id)))
            return Json(new { ok = false, mensaje = "No se puede agregar una factura emitida a una cotizacion." });

        var proceso = Convert.ToString(caja.Caja.caja_nro_proceso, CultureInfo.InvariantCulture)?.Trim();
        if (string.IsNullOrWhiteSpace(proceso) || !short.TryParse(
                Convert.ToString(caja.Caja.caja_nro_cierre, CultureInfo.InvariantCulture), out var cierre))
            return Json(new { ok = false, mensaje = "No se encontraron el proceso y cierre de la caja." });

        try
        {
            var tipos = await _notaCreditoServicio.GetTipoComprobante("%", "VE", TokenCookie);
            if (tipos?.Ok != true || tipos.ListaEntidad?.Any(x => x.tco_id.Trim() == tipo) != true)
                return Json(new { ok = false, mensaje = "El tipo de comprobante no esta habilitado para ventas." });

            var validacionRequest = new NCValidaRequestDto
            {
                tco_id = tipo,
                cm_compte = comprobante,
                caja_nro_proceso = proceso,
                caja_nro_cierre = cierre
            };
            _logger?.LogInformation("Factura emitida: SPGECO_CAJA_NC_Valida Request={Request}",
                JsonConvert.SerializeObject(validacionRequest));
            var validacion = await _notaCreditoServicio.ValidarNC(validacionRequest, TokenCookie);
            _logger?.LogInformation("Factura emitida: validacion Ok={Ok}, Mensaje={Mensaje}, Repeticiones={Repeticiones}",
                validacion?.Ok, validacion?.Mensaje,
                JsonConvert.SerializeObject(validacion?.ListaEntidad?.Select(x => new { x.tco_id, x.cm_compte, x.cm_repetido })));
            if (validacion?.Ok != true)
                return Json(new { ok = false, mensaje = validacion?.Mensaje ?? "No fue posible validar la factura." });

            // Solo identificamos la venta. Los indicadores nc_* no restringen su copia.
            var origen = FacturaEmitidaRequest.SeleccionarUltimaRepeticion(validacion.ListaEntidad ?? [], tipo, comprobante);
            if (origen == null)
                return Json(new { ok = false, mensaje = "No se encontro la factura emitida." });
            string clave;
            try
            {
                clave = FacturaEmitidaRequest.ConstruirClave(tipo, comprobante, origen.cm_repetido);
            }
            catch (ArgumentException ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }

            var esConsumidorFinal = string.Equals(cliente.Origen, "F", StringComparison.OrdinalIgnoreCase);
            var canal = string.IsNullOrWhiteSpace(cliente.ctc_id) ? (esConsumidorFinal ? "MI" : "MA") : cliente.ctc_id;
            var resultado = await ObtenerProductoDatosCommon("V", clave, LP_Id, canal, cliente.cta_id ?? string.Empty, 1, true);
            _logger?.LogInformation("Factura emitida: SPGECO_CAJA_B_Producto_Datos Clave={Clave}, Response={Response}",
                clave, JsonConvert.SerializeObject(resultado));
            if (resultado?.Ok != true)
                return Json(new { ok = false, mensaje = resultado?.Mensaje ?? "No fue posible cargar el detalle de la factura." });

            var filas = resultado.ListaEntidad ?? [];
            var validos = filas.Where(x => x.respuesta == 0 && !string.IsNullOrWhiteSpace(x.p_id) && x.cantidad_tot > 0).ToList();
            var errores = filas.Except(validos)
                .Select(x => $"{x.p_id} {x.p_desc}: {(x.respuesta == 0 ? "Datos de producto incompletos" : x.respuesta_msj)}")
                .ToList();
            if (validos.Count == 0)
                return Json(new { ok = false, errores, mensaje = "La factura no contiene productos disponibles para cargar." });

            var seleccionados = ProductosSeleccionados ?? [];
            var backupOk = true;
            try
            {
                if (seleccionados.Count == 0)
                    backupOk = await _backupServicio.ReiniciarBackup(caja.CajaId, UserName ?? string.Empty);
                if (backupOk)
                    backupOk = await _backupServicio.GuardarProductosEnBloque(validos, caja.CajaId, UserName ?? string.Empty);
            }
            catch (Exception ex)
            {
                backupOk = false;
                _logger?.LogWarning(ex, "Factura emitida: no fue posible respaldar el detalle.");
            }
            seleccionados.AddRange(validos);
            ProductosSeleccionados = seleccionados;
            if (!backupOk)
                errores.Add("No se pudo guardar el respaldo local de los productos cargados.");

            return Json(new
            {
                ok = true,
                productos = validos,
                errores,
                acumula = caja.acumula,
                comprobante = new { tco_id = tipo, cm_compte = comprobante, cm_repetido = origen.cm_repetido },
                mensaje = $"Factura {comprobante}, repeticion {origen.cm_repetido}: {validos.Count} productos cargados."
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Factura emitida: error cargando {Tipo} {Comprobante}.", tipo, comprobante);
            return Json(new { ok = false, mensaje = "No fue posible cargar la factura emitida. Intente nuevamente." });
        }
    }
}
