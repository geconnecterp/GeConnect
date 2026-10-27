using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.caja.core.Servicios.Implementacion.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using gc.infraestructura.Dtos.Gen;
using Newtonsoft.Json;

namespace gc.caja.Areas.Facturacion.Controllers;

[Area("Facturacion"), Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class CloverController : PagoConCreditosController
{
    private readonly CloverApiServicio clover;
    private readonly ICajaServicio cajaServicio;
    public CloverController(IOptions<AppSettings> options, ICheckoutServicio pago, IHttpContextAccessor context,
        ILogger<CloverController> logger, CloverApiServicio clover, ICajaServicio cajaServicio) : base(options, pago, context, logger) { this.clover = clover; this.cajaServicio = cajaServicio; }

    [HttpGet]
    public async Task<IActionResult> Opciones(string? insId, string coTipo)
    {
        try {
            if (!VerificarAutenticacion(out _)) return Unauthorized();
            var c = ContextoClover(coTipo);
            var opciones = await clover.Opciones(new() { contexto = c, pago = new() { ins_id = insId ?? "", co_tipo = c.co_tipo } }, TokenCookie);
            return Json(new { ok = true, opciones, mensaje = opciones.Count == 0 ? "No hay cuotas Clover disponibles. Consulte la configuración del medio de pago con el administrador." : "Opciones vigentes." });
        } catch (Exception ex) { _logger?.LogWarning(ex, "Clover: no se pudieron cargar opciones"); return Json(new { ok = false, mensaje = ex.Message }); }
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Procesar([FromBody] CloverPreparacionDto solicitud)
    {
        var enviado = false;
        try {
            if (!VerificarAutenticacion(out _)) return Unauthorized();
            if (solicitud == null) return BadRequest();
            var c = ContextoClover(solicitud.co_tipo);
            if (c.co_tipo is "CR" or "CF" && (string.IsNullOrWhiteSpace(LP_Id) || FacturaProductos.Count == 0 || FacturaSubtotales.Count == 0))
                return Json(new RespuestaDto { resultado = -1, resultado_msj = "Debe cargar productos, calcular totales y tener una lista de precios activa antes de cobrar con Clover." });
            var valores = solicitud.valores ?? []; var totalNc = 0m;
            var catalogo = await CatalogoActual(c);
            // Conservar los instrumentos concretos que el SP devolvió para pagos anteriores.
            foreach (var par in LeerCatalogo(c)) catalogo.TryAdd(par.Key, par.Value);
            if (c.co_tipo is not ("CV" or "IV")) {
                var nc = await ValidarYConstruirUnionesNcAsync(solicitud.uniones, c.co_tipo, c.co_tipo == "CF");
                if (!nc.Ok) return Json(new RespuestaDto { resultado = -1, resultado_msj = nc.Mensaje });
                totalNc = nc.TotalImputado;
                var total = TotalBaseClover(c.co_tipo) + valores.Where(v => catalogo.GetValueOrDefault(v.ins_id) == "T1").Sum(v => v.rb_aux);
                if (total <= 0 || solicitud.monto_base <= 0 || solicitud.monto_base > total - valores.Sum(v => v.rb_importe) - totalNc)
                    return Json(new RespuestaDto { resultado = -1, resultado_msj = "El monto Clover supera el saldo real pendiente de la operación." });
                var regla = ReglasCreditoPago.Validar(valores, total, totalNc, c.co_tipo, ReglasCreditoPago.ObtenerCondiciones(ClienteActual!), catalogo, DateTime.Today);
                if (!regla.Ok) throw new InvalidOperationException(regla.Mensaje);
                GuardarCatalogo(c, catalogo);
            }
            if (c.co_tipo == "CD" && solicitud.recargo != 0) return Json(new RespuestaDto { resultado = -1, resultado_msj = "Cobranza Diferida no admite recargos financieros." });
            var pv = await ValidarEstadoPuntoVenta(cajaServicio, c.caja_id, CajaActual.Caja.ctrl_id ?? "", c.caja_nro_proceso, c.caja_nro_cierre.ToString(), "F");
            if (!pv.PuedeContinuar) return Json(new RespuestaDto { resultado = -1, resultado_msj = pv.Mensaje });
            enviado = true;
            var resultado = await clover.Procesar(new() { contexto = c, pago = solicitud }, TokenCookie);
            if (resultado.resultado == 0) {
                var nuevos = JsonConvert.DeserializeObject<List<Json_Valor>>(resultado.resultado_id) ?? [];
                foreach (var valor in nuevos) if (!string.IsNullOrWhiteSpace(valor.ins_id)) catalogo[valor.ins_id] = "T1";
                GuardarCatalogo(c, catalogo);
            }
            _logger?.LogInformation("Clover {Operacion}/{Intento}: resultado={Resultado}; mensaje={Mensaje}", c.operacion_id, solicitud.intento_id, resultado.resultado, resultado.resultado_msj);
            return Json(resultado);
        } catch (Exception ex) {
            _logger?.LogError(ex, "Clover: error de procesamiento. EnviadoAPI={Enviado}", enviado);
            return Json(new { resultado = -1, resultado_msj = ex.Message, sin_respuesta = enviado });
        }
    }
    private Dictionary<string, string> LeerCatalogo(CloverContextoDto c) =>
        new(JsonConvert.DeserializeObject<Dictionary<string, string>>(HttpContext.Session.GetString(ClaveCatalogo(c)) ?? "{}") ?? [], StringComparer.OrdinalIgnoreCase);
    private string ClaveCatalogo(CloverContextoDto c) => $"PAGO_CATALOGO:{CajaActual.CajaId}:{c.co_tipo}:{ClienteActual!.Origen}:{ClienteActual!.cta_id}:{ClienteActual!.cta_documento}";
    private void GuardarCatalogo(CloverContextoDto c, Dictionary<string, string> catalogo) =>
        HttpContext.Session.SetString(ClaveCatalogo(c), JsonConvert.SerializeObject(catalogo));
    private async Task<Dictionary<string, string>> CatalogoActual(CloverContextoDto c)
    {
        var catalogo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var medios = await _pagoFactServicio.ObtenerValoresMP(new() { co_tipo = c.co_tipo, cta_id = c.cta_id, adm_id = c.adm_id }, TokenCookie);
        if (!medios.Ok) throw new InvalidOperationException(medios.Mensaje);
        foreach (var medio in medios.ListaEntidad ?? []) {
            var instrumentos = await _pagoFactServicio.ObtenerValoresIns(new() { tcf_id = medio.tcf_id ?? "", co_tipo = c.co_tipo, cta_id = c.cta_id, adm_id = c.adm_id }, TokenCookie);
            if (!instrumentos.Ok) throw new InvalidOperationException(instrumentos.Mensaje);
            foreach (var i in instrumentos.ListaEntidad ?? []) if (!string.IsNullOrWhiteSpace(i.ins_id)) catalogo[i.ins_id.Trim()] = (medio.tcf_id ?? "").Trim().ToUpperInvariant();
        }
        return catalogo;
    }

}
