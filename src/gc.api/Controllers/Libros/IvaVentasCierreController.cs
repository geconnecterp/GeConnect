using gc.api.core.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Libros;

[Authorize, ApiController, Route("api/iva-ventas-cierre")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class IvaVentasCierreController(IvaVentasCierreServicio servicio, ILogger<IvaVentasCierreController> logger) : ControllerBase
{
    private string Perfil => Request.Headers["X-Geco-Perfil"].ToString();
    private string Menu => Request.Headers["X-Geco-Menu"].ToString();
    [HttpGet("acceso")]
    public IActionResult Acceso() => servicio.TieneAcceso(User, Perfil, Menu) ? Ok() : Forbid();
    [HttpGet("estado")]
    public async Task<IActionResult> Estado(CancellationToken ct)
    {
        try { return Ok(await servicio.Consultar(User, Perfil, Menu, ct)); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex, false); }
    }
    [HttpPost("cerrar")]
    public async Task<IActionResult> Cerrar(IvaVentasCierrePedido pedido, CancellationToken ct)
    {
        try { return Ok(await servicio.Cerrar(pedido, User, Perfil, Menu, ct)); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex, true); }
    }
    private static readonly SemaphoreSlim Descargas = new(2, 2);
    [HttpPost("rg3685")]
    public async Task<IActionResult> Rg3685(IvaVentasCierrePedido pedido, CancellationToken ct)
    {
        FileStream? archivo = null; var adquirido = false;
        try
        {
            if (!servicio.TieneAcceso(User, Perfil, Menu)) return Forbid();
            pedido.Validar();
            adquirido = await Descargas.WaitAsync(0, ct);
            if (!adquirido) return StatusCode(429, new { mensaje = "Hay otras descargas RG 3685 en preparación. Intente nuevamente en unos momentos." });
            // Preparar todo antes de enviar; archivo temporal borrado al finalizar o ante error.
            archivo = new FileStream(Path.Combine(Path.GetTempPath(), "geco-iva-" + Guid.NewGuid().ToString("N") + ".zip"),
                FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
            await servicio.GenerarRg3685(pedido, User, Perfil, Menu, archivo, ct);
            archivo.Position = 0;
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            var resultado = File(archivo, "application/zip", IvaVentasCierreArchivos.NombreZip(pedido.Periodo));
            archivo = null; // FileStreamResult adquiere y libera la propiedad del archivo.
            return resultado;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex, false); }
        finally { if (archivo != null) await archivo.DisposeAsync(); if (adquirido) Descargas.Release(); }
    }
    private IActionResult Error(Exception ex, bool cierre)
    {
        if (ex is UnauthorizedAccessException) return StatusCode(403, new { mensaje = ex.Message });
        if (ex is ArgumentException) return BadRequest(new { mensaje = ex.Message });
        if (ex is IvaVentasCierreConflictoException) return Conflict(new { mensaje = ex.Message });
        logger.LogError("IVA cierre fallido. Operación {Operacion}; tipo {Tipo}; referencia {Ref}", cierre ? "cerrar" : "consultar/exportar", ex.GetType().Name, HttpContext.TraceIdentifier);
        return StatusCode(503, new { mensaje = cierre
            ? "No se pudo confirmar el resultado del cierre. Actualice el historial antes de volver a intentarlo. El cierre no se reintentó automáticamente."
            : "No se pudo completar la consulta o descarga de IVA Ventas. No se entregaron datos parciales. Intente nuevamente." });
    }
}
