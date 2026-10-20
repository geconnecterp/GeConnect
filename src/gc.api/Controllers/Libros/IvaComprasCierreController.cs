using gc.api.core.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Libros;

[Authorize, ApiController, Route("api/iva-compras-cierre")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class IvaComprasCierreController(IvaComprasCierreServicio servicio, ILogger<IvaComprasCierreController> logger) : ControllerBase
{
    private string Perfil => Request.Headers["X-Geco-Perfil"].ToString();
    private string Menu => Request.Headers["X-Geco-Menu"].ToString();
    private static CancellationTokenSource Presupuesto(CancellationToken ct)
    {
        var source = CancellationTokenSource.CreateLinkedTokenSource(ct);
        source.CancelAfter(TimeSpan.FromMinutes(10));
        return source;
    }
    [HttpGet("acceso")]
    public IActionResult Acceso()
    {
        try { return servicio.TieneAcceso(User, Perfil, Menu) ? Ok() : Forbid(); }
        catch (Exception ex) { return Error(ex, false); }
    }

    [HttpGet("estado")]
    public async Task<IActionResult> Estado(CancellationToken ct)
    {
        using var tiempo = Presupuesto(ct);
        try { return Ok(await servicio.Consultar(User, Perfil, Menu, tiempo.Token)); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex, false); }
    }

    [HttpPost("cerrar")]
    public async Task<IActionResult> Cerrar(IvaComprasCierrePedido pedido, CancellationToken ct)
    {
        using var tiempo = Presupuesto(ct);
        try { return Ok(await servicio.Cerrar(pedido, User, Perfil, Menu, tiempo.Token)); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex, true); }
    }

    private static readonly SemaphoreSlim Descargas = new(2, 2);
    [HttpPost("rg3685")]
    public async Task<IActionResult> Rg3685(IvaComprasCierreDescargaPedido pedido, CancellationToken ct)
    {
        FileStream? archivo = null; var adquirido = false;
        using var tiempo = Presupuesto(ct);
        try
        {
            if (!servicio.TieneAcceso(User, Perfil, Menu)) return Forbid();
            pedido.Validar();
            adquirido = await Descargas.WaitAsync(0, tiempo.Token);
            if (!adquirido) return StatusCode(429, new { mensaje = "Hay otras descargas de Cierre IVA Compras en preparación. Intente nuevamente en unos momentos." });
            archivo = new FileStream(Path.Combine(Path.GetTempPath(), "geco-iva-compras-" + Guid.NewGuid().ToString("N") + ".zip"),
                FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
            var conteos = await servicio.GenerarRg3685(pedido, User, Perfil, Menu, archivo, tiempo.Token);
            archivo.Position = 0;
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["X-Geco-Registros-C"] = conteos.Cabecera.ToString(System.Globalization.CultureInfo.InvariantCulture);
            Response.Headers["X-Geco-Registros-D"] = conteos.Detalle.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var resultado = File(archivo, "application/zip", IvaComprasCierreArchivos.NombreZip(pedido.Periodo));
            archivo = null; // El resultado MVC libera el temporal al terminar la respuesta.
            return resultado;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex, false); }
        finally { if (archivo != null) await archivo.DisposeAsync(); if (adquirido) Descargas.Release(); }
    }

    private IActionResult Error(Exception ex, bool cierre)
    {
        if (ex is UnauthorizedAccessException) return StatusCode(403, new { mensaje = ex.Message });
        if (ex is ArgumentException) return BadRequest(new { mensaje = ex.Message });
        if (ex is IvaComprasCierreConflictoException) return Conflict(new { mensaje = ex.Message });
        if (ex is IvaComprasCierreArchivoException) return UnprocessableEntity(new { mensaje = ex.Message });
        logger.LogError("IVA Compras cierre fallido. Operación {Operacion}; tipo {Tipo}; referencia {Ref}",
            cierre ? "cerrar" : "consultar/exportar", ex.GetType().Name, HttpContext.TraceIdentifier);
        return StatusCode(503, new { mensaje = cierre
            ? "No se pudo confirmar el resultado del cierre. Actualice el historial antes de volver a intentarlo. El cierre no se reintentó automáticamente."
            : "No se pudo completar la consulta o descarga de IVA Compras. No se entregaron datos parciales. Intente nuevamente." });
    }
}

