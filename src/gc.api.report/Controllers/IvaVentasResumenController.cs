using gc.api.core.Servicios.Libros;
using gc.api.core.Servicios.Reportes;
using gc.infraestructura.Dtos.Libros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.report.Controllers;

[Authorize, ApiController, Route("api/iva-ventas-resumen")]
public class IvaVentasResumenController(IvaVentasResumenServicio servicio, ILogger<IvaVentasResumenController> logger) : ControllerBase
{
    private static readonly SemaphoreSlim Exportaciones = new(2, 2);
    [HttpPost("excel")]
    public async Task<IActionResult> Excel(IvaVentasResumenFiltro filtro, CancellationToken ct)
    {
        var adquirido = false;
        try
        {
            if (!servicio.TieneAcceso(User, Request.Headers["X-Geco-Perfil"].ToString(), Request.Headers["X-Geco-Menu"].ToString())) return Forbid();
            adquirido = await Exportaciones.WaitAsync(0, ct);
            if (!adquirido) return StatusCode(429, new { mensaje = "Hay otras exportaciones en curso. Intente nuevamente." });
            var filas = await servicio.Consultar(filtro, ct);
            var archivo = IvaVentasResumenReporte.Excel(IvaVentasResumenLibro.Crear(filtro, filas), ct);
            Response.Headers.CacheControl = "no-store, private";
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(archivo, IvaVentasResumenReporte.MimeExcel, $"Libro_IVA_Ventas_Resumen_{filtro.PeriodoTexto}.xlsx");
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
