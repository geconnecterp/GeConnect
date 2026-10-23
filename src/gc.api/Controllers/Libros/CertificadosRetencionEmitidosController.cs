using gc.api.core.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Libros;

[Authorize, ApiController, Route("api/certificados-retencion-emitidos")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CertificadosRetencionEmitidosController(CertificadosRetencionServicio servicio, ILogger<CertificadosRetencionEmitidosController> logger) : ControllerBase
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
    public async Task<IActionResult> Consultar(CertificadosRetencionFiltro filtro, CancellationToken ct)
    {
        try { return Ok(await servicio.Consultar(filtro, User, Perfil, Menu, ct)); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex); }
    }
    private static readonly SemaphoreSlim Descargas = new(2, 2);
    [HttpPost("ddjj")]
    public async Task<IActionResult> Ddjj(CertificadosRetencionDdjjPedido pedido, CancellationToken ct)
    {
        FileStream? archivo = null; var adquirido = false;
        try
        {
            if (!servicio.TieneAcceso(User, Perfil, Menu)) return Forbid();
            pedido.Validar();
            adquirido = await Descargas.WaitAsync(0, ct);
            if (!adquirido) return StatusCode(429, new { mensaje = "Hay otras DDJJ en preparación. Intente nuevamente en unos momentos." });
            archivo = new FileStream(Path.Combine(Path.GetTempPath(), "geco-ret-ddjj-" + Guid.NewGuid().ToString("N") + ".tmp"),
                FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
            await servicio.GenerarDdjj(pedido, User, Perfil, Menu, archivo, ct);
            archivo.Position = 0;
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            var resultado = File(archivo, pedido.ContentType, pedido.Nombre);
            archivo = null; // MVC libera el temporal después de enviar el archivo completo.
            return resultado;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex, true); }
        finally { if (archivo != null) await archivo.DisposeAsync(); if (adquirido) Descargas.Release(); }
    }
    private IActionResult Error(Exception ex, bool descarga = false)
    {
        if (ex is UnauthorizedAccessException) return StatusCode(403, new { mensaje = ex.Message });
        if (ex is ArgumentException) return BadRequest(new { mensaje = ex.Message });
        if (ex is CertificadosRetencionDdjjException) return UnprocessableEntity(new { mensaje = ex.Message });
        logger.LogError("Retenciones: operación fallida. Descarga {Descarga}; tipo {Tipo}; referencia {Ref}", descarga, ex.GetType().Name, HttpContext.TraceIdentifier);
        return StatusCode(ex is OperationCanceledException ? 504 : 503, new { mensaje = ex is OperationCanceledException
            ? descarga ? "La DDJJ mensual excedió el tiempo de espera. No se entregó un archivo parcial. Intente nuevamente más tarde." : "La consulta excedió el tiempo de espera. Acote los filtros e intente nuevamente."
            : descarga ? "No se pudo generar la DDJJ mensual. No se entregó un archivo parcial. Solicite revisar la conexión y el contrato del SP." : "No se pudieron consultar los certificados. No se muestran datos parciales. Solicite revisar la conexión y el contrato del SP." });
    }
}
