using gc.api.core.Servicios.LineaCaja;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Caja;

[Authorize, ApiController, Route("api/caja/custodia")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public partial class ApiCustodiaController(CustodiaServicio servicio, ILogger<ApiCustodiaController> logger, Microsoft.Extensions.Caching.Memory.IMemoryCache cache) : ControllerBase
{
    private (string Sucursal, string Usuario) Contexto()
    {
        var adm = User.FindFirst("AdmId")?.Value.Split('#')[0] ?? "";
        var usuario = User.FindFirst("user")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        if (string.IsNullOrWhiteSpace(adm) || string.IsNullOrWhiteSpace(usuario)) throw new UnauthorizedAccessException("La sesión no identifica usuario y sucursal.");
        return (adm, usuario);
    }

    [HttpGet("consulta")]
    public IActionResult Consulta()
    {
        try { var c = Contexto(); return Ok(new ApiResponse<CustodiaConsultaDto>(servicio.Consulta(c.Sucursal))); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception ex) { logger.LogError(ex, "Consulta de custodia fallida. Ref={Ref}", HttpContext.TraceIdentifier); return StatusCode(503, new { mensaje = "No se pudieron consultar las rendiciones pendientes de custodia." }); }
    }
    [HttpGet("acuse")]
    public IActionResult Acuse(string entrega)
    {
        try
        {
            var c = Contexto();
            return File(gc.api.core.Servicios.Reportes.CustodiaAcusePdf.Generar(servicio.Acuse(c.Sucursal, entrega)), "application/pdf");
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (Exception ex) { logger.LogError(ex, "[CUSTODIA] Acuse fallido. Entrega={Entrega}, Ref={Ref}", entrega, HttpContext.TraceIdentifier); return StatusCode(503, new { mensaje = "No se pudo generar el acuse. No registre nuevamente la entrega." }); }
    }

}
