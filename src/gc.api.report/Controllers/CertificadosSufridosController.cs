using gc.api.core.Servicios.Libros;
using gc.api.core.Servicios.Reportes;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.api.report.Controllers;

[Authorize, ApiController, Route("api/certificados-sufridos")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CertificadosSufridosController(CertificadosSufridosServicio servicio, IOptions<EmpresaGeco> empresa,
    IWebHostEnvironment env, ILogger<CertificadosSufridosController> logger) : ControllerBase
{
    private static readonly SemaphoreSlim Reportes = new(2, 2);
    [HttpPost("pdf")]
    public async Task<IActionResult> Pdf(CertificadosSufridosFiltro filtro, CancellationToken ct)
    {
        var adquirido = false;
        try
        {
            var perfil = Request.Headers["X-Geco-Perfil"].ToString(); var menu = Request.Headers["X-Geco-Menu"].ToString();
            if (!servicio.TieneAcceso(User, perfil, menu)) return Forbid();
            filtro.Validar(); adquirido = await Reportes.WaitAsync(0, ct);
            if (!adquirido) return StatusCode(429, new { mensaje = "Hay otros reportes en preparación. Intente nuevamente en unos momentos." });
            var consulta = await servicio.Consultar(filtro, User, perfil, menu, ct);
            var bytes = CertificadosSufridosReporte.Pdf(consulta, filtro, empresa.Value, Path.Combine(env.ContentRootPath, "img", empresa.Value.Logo), ct);
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(bytes, "application/pdf", filtro.NombrePdf);
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (NotSupportedException ex) { return UnprocessableEntity(new { mensaje = ex.Message }); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogError("Certificados sufridos: reporte fallido. Tipo {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
            return StatusCode(ex is OperationCanceledException ? 504 : 503, new { mensaje = "No se pudo generar el reporte completo. Solicite revisar el SP y la configuración del reporte. No se entregó un PDF parcial." });
        }
        finally { if (adquirido) Reportes.Release(); }
    }
}
