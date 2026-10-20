using gc.api.core.Servicios.Libros;
using gc.api.core.Servicios.Reportes;
using gc.infraestructura.Dtos.Libros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.report.Controllers;

[Authorize, ApiController, Route("api/iva-compras-resumen")]
public class IvaComprasResumenController(IvaComprasResumenServicio servicio, ILogger<IvaComprasResumenController> logger) : ControllerBase
{
    private static readonly SemaphoreSlim Exportaciones = new(2, 2);
    [HttpPost("excel")]
    public async Task<IActionResult> Excel(IvaComprasResumenFiltro filtro, CancellationToken ct)
    {
        var adquirido = false;
        try
        {
            if (!servicio.TieneAcceso(User, Request.Headers["X-Geco-Perfil"].ToString(), Request.Headers["X-Geco-Menu"].ToString())) return Forbid();
            adquirido = await Exportaciones.WaitAsync(0, ct);
            if (!adquirido) return StatusCode(429, new { mensaje = "Hay otras exportaciones en curso. Intente nuevamente." });
            var libro = await servicio.Consultar(filtro, ct);
            var archivo = IvaComprasResumenReporte.Excel(libro, ct);
            Response.Headers.CacheControl = "no-store, private";
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(archivo, IvaComprasResumenReporte.MimeExcel, $"Libro_IVA_Compras_Resumen_{filtro.PeriodoTexto}.xlsx");
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogError("Excel IVA resumen fallido. Tipo {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
            return StatusCode(503, new { mensaje = "No se pudo generar el Excel completo. No se entregó un archivo parcial. Contacte al administrador." });
        }
        finally { if (adquirido) Exportaciones.Release(); }
    }
}
