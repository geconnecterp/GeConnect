using gc.caja.Controllers;
using gc.caja.Models;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Enumeraciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.caja.Areas.Facturacion.Controllers;

[Area("Facturacion"), Authorize, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SalidaComprobantesController(IOptions<AppSettings> options, IHttpContextAccessor context,
    ILogger<SalidaComprobantesController> logger, IReportesConfigService configuracion, IReportesService reportes)
    : ControladorBaseCaja(options, context, logger)
{
    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(8192)]
    public IActionResult Registrar([FromBody] SalidaDiagnosticoDto? dato)
    {
        if (dato == null || !ModelState.IsValid || dato.Solicitud == Guid.Empty) return BadRequest();
        logger.LogInformation("[IMPRESION][ESTADO] Usuario={Usuario} Caja={Caja} Sucursal={Sucursal}\n{Json}",
            UserName, CajaActual.CajaId, AdministracionId, JsonConvert.SerializeObject(dato, Formatting.Indented));
        return NoContent();
    }

    // Sólo recupera el PDF de un recibo ya registrado. Nunca confirma una cobranza.
    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(2048)]
    public async Task<IActionResult> Recibo([FromBody] DocumentoSalidaDto? dato)
    {
        if (dato == null || !ModelState.IsValid || dato.Tipo is not ("RECIBO_CD" or "RECIBO_CC") ||
            string.IsNullOrWhiteSpace(dato.Rb_compte) || dato.Rb_compte.Any(char.IsControl))
            return BadRequest(new { mensaje = "La identificación del recibo es inválida." });
        if (!TieneTokenValido()) return Unauthorized(new { mensaje = "La sesión expiró." });
        var reporte = configuracion.ObtenerPorKey(dato.Tipo) ?? configuracion.ObtenerPorKey("RECIBO");
        if (reporte == null || !int.TryParse(reporte.Id, out var id) || !Enum.IsDefined(typeof(InfoReporte), id))
            return StatusCode(503, new { mensaje = "La cobranza está registrada. No hay un reporte PDF de recibo configurado para este módulo." });
        try
        {
            var respuesta = await reportes.ObtenerPdfDesdeAPI(new ReporteSolicitudDto {
                Reporte = (InfoReporte)id, Formato = "P", Titulo = reporte.Nombre,
                Parametros = new Dictionary<string, string> { ["rb_compte"] = dato.Rb_compte, ["adm_id"] = AdministracionId }
            }, TokenCookie);
            return Ok(respuesta);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[IMPRESION][RECIBO] No se pudo recuperar {Recibo} de {Modulo}.", dato.Rb_compte, dato.Tipo);
            return StatusCode(503, new { mensaje = "La cobranza conserva su registro. No se pudo recuperar el PDF del recibo." });
        }
    }
}
