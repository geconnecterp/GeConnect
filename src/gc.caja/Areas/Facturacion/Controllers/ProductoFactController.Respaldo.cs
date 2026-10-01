using gc.caja.Models;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace gc.caja.Areas.Facturacion.Controllers;
public partial class ProductoFactController
{
    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(262144)]
    public async Task<IActionResult> RecuperarBackup([FromBody] RecuperarDetalleRequest? request)
    {
        var id = Guid.TryParse(Request.Headers["X-Respaldo-Id"].ToString(), out var recibido)
            ? recibido.ToString("D") : HttpContext.TraceIdentifier;
        Response.Headers["X-Respaldo-Id"] = id;
        var reloj = Stopwatch.StartNew();
        var etapa = "validacion";
        string? codigoActual = null;
        IActionResult Rechazar(int estado, string mensaje)
        {
            _logger?.LogWarning("[UltimoDetalle] Id={Id} Etapa={Etapa} HTTP={Estado} Motivo={Motivo}", id, etapa, estado, mensaje);
            var contenido = new { ok = false, mensaje };
            return estado switch { 400 => BadRequest(contenido), 401 => Unauthorized(contenido),
                409 => Conflict(contenido), _ => StatusCode(estado, contenido) };
        }
        _logger?.LogInformation("[UltimoDetalle] Id={Id} Inicio Ruta={PathBase}{Path} Filas={Filas}",
            id, Request.PathBase, Request.Path, request?.Productos?.Count ?? 0);
        try
        {
            if (!VerificarAutenticacion(out _)) return Rechazar(401, "La sesión ha expirado.");
            if (request?.Productos == null || !ModelState.IsValid)
                return Rechazar(400, "El respaldo contiene datos inválidos.");
            var cliente = ClienteActual;
            var lista = LP_Id;
            if (cliente == null || string.IsNullOrWhiteSpace(lista))
                return Rechazar(400, "Seleccione un cliente y su lista de precios antes de recuperar el detalle.");
            if (ProductosSeleccionados.Count > 0 || FacturaProductos.Count > 0)
                return Rechazar(409, "La grilla debe estar vacía para recuperar el detalle.");
            var canal = string.IsNullOrWhiteSpace(cliente.ctc_id) ? (cliente.Origen == "F" ? "MI" : "MA") : cliente.ctc_id;
            var productos = new List<ProductoDatosResponseDto>();
            var omitidos = 0;
            etapa = "consultar-productos";
            _logger?.LogInformation("[UltimoDetalle] Id={Id} Cliente={Cliente} Lista={Lista} Canal={Canal}", id, cliente.cta_id, lista, canal);
            foreach (var item in request.Productos)
            {
                codigoActual = item.Codigo;
                _logger?.LogDebug("[UltimoDetalle] Id={Id} Consultando Producto={Producto}", id, codigoActual);
                var respuesta = await ObtenerProductoDatosCommon("P", item.Codigo, lista, canal, cliente.cta_id ?? "", item.Cantidad, false);
                if (respuesta?.EsWarn == true && respuesta.ListaEntidad is { Count: 0 })
                {
                    _logger?.LogWarning("[UltimoDetalle] Id={Id} Producto={Producto} Omitido=no-encontrado", id, codigoActual);
                    omitidos++; continue;
                }
                // Un fallo de transporte no debe convertir un respaldo completo en otro parcial.
                if (respuesta == null || !respuesta.Ok || respuesta.ListaEntidad == null)
                {
                    _logger?.LogWarning("[UltimoDetalle] Id={Id} Producto={Producto} Consulta fallida Ok={Ok}", id, codigoActual, respuesta?.Ok);
                    return Rechazar(502, "No se pudo verificar el detalle completo. El respaldo se conserva; vuelva a intentar.");
                }
                var validos = respuesta.ListaEntidad.Where(p => p.respuesta == 0 && !string.IsNullOrWhiteSpace(p.p_id) && p.cantidad_tot > 0).ToList();
                if (validos.Count == 0)
                {
                    _logger?.LogWarning("[UltimoDetalle] Id={Id} Producto={Producto} Omitido=validacion-producto Respuestas={Respuestas}",
                        id, codigoActual, string.Join(",", respuesta.ListaEntidad.Select(p => p.respuesta)));
                    omitidos++; continue;
                }
                foreach (var producto in validos)
                {
                    // El SP aplica p_unidad_pres incluso con bulto=false. El respaldo ya contiene unidades.
                    producto.cantidad_tot = item.Cantidad;
                    producto.item = productos.Count + 1;
                    producto.cpf_nro = null;
                    producto.pre_id = null;
                    productos.Add(producto);
                }
            }
            etapa = "guardar-sesion";
            ProductosSeleccionados = productos;
            _logger?.LogInformation("[UltimoDetalle] Id={Id} Recuperado Filas={Filas} Omitidos={Omitidos}", id, productos.Count, omitidos);
            return Json(new { ok = true, producto = productos, omitidos, acumula = CajaActual.acumula });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[UltimoDetalle] Id={Id} Etapa={Etapa} Producto={Producto} Error al recuperar", id, etapa, codigoActual);
            return StatusCode(502, new { ok = false, mensaje = "No se pudo recuperar el detalle. El respaldo original se conserva." });
        }
        finally
        {
            _logger?.LogInformation("[UltimoDetalle] Id={Id} Fin Etapa={Etapa} DuracionMs={DuracionMs}", id, etapa, reloj.ElapsedMilliseconds);
        }
    }
}
