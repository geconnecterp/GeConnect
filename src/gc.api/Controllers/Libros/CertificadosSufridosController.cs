using gc.api.core.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Libros;

[Authorize, ApiController, Route("api/certificados-sufridos")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CertificadosSufridosController(CertificadosSufridosServicio servicio, ILogger<CertificadosSufridosController> logger) : ControllerBase
{
    private string Perfil => Request.Headers["X-Geco-Perfil"].ToString();
    private string Menu => Request.Headers["X-Geco-Menu"].ToString();
    [HttpGet("opciones")]
    public async Task<IActionResult> Opciones(CancellationToken ct)
    {
        try { return Ok(await servicio.Opciones(User, Perfil, Menu, ct)); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex); }
    }
    [HttpPost("consultar")]
    public async Task<IActionResult> Consultar(CertificadosSufridosFiltro filtro, CancellationToken ct)
    {
        try { return Ok(await servicio.Consultar(filtro, User, Perfil, Menu, ct)); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex); }
    }
    private IActionResult Error(Exception ex)
    {
        if (ex is UnauthorizedAccessException) return StatusCode(403, new { mensaje = ex.Message });
        if (ex is ArgumentException) return BadRequest(new { mensaje = ex.Message });
        if (ex is NotSupportedException) return UnprocessableEntity(new { mensaje = ex.Message });
        logger.LogError("Certificados sufridos: consulta fallida. Tipo {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
        return StatusCode(ex is OperationCanceledException ? 504 : 503, new { mensaje = ex is OperationCanceledException
            ? "La consulta excedió el tiempo de espera. Acote los filtros e intente nuevamente."
            : "No se pudo completar la consulta. Solicite revisar el contrato y la ejecución del SP de certificados sufridos del modo elegido. No se muestran datos parciales.", referencia = HttpContext.TraceIdentifier });
    }
}
