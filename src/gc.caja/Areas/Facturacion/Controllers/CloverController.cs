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
            var catalogo = await PrepararCobroIntegrado(c, solicitud, cajaServicio, "Clover");
            enviado = true;
            var resultado = await clover.Procesar(new() { contexto = c, pago = solicitud }, TokenCookie);
            if (resultado.resultado == 0) RecordarCobroIntegrado(c.co_tipo, "T1", catalogo, resultado.resultado_id);
            _logger?.LogInformation("Clover {Operacion}/{Intento}: resultado={Resultado}; mensaje={Mensaje}", c.operacion_id, solicitud.intento_id, resultado.resultado, resultado.resultado_msj);
            return Json(resultado);
        } catch (Exception ex) {
            _logger?.LogError(ex, "Clover: error de procesamiento. EnviadoAPI={Enviado}", enviado);
            return Json(new { resultado = -1, resultado_msj = ex.Message, sin_respuesta = enviado });
        }
    }
}
