using gc.api.core.Servicios.Libros;
using gc.api.core.Servicios.Reportes;
using gc.infraestructura.Dtos.Libros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.report.Controllers;

[Authorize]
[ApiController]
[Route("api/iva-ventas")]
public class IvaVentasController(IvaVentasServicio servicio, ILogger<IvaVentasController> logger) : ControllerBase
{
    private static readonly SemaphoreSlim Exportaciones = new(2, 2);

    [HttpPost("excel")]
    public async Task<IActionResult> Excel(IvaVentasFiltro filtro, CancellationToken ct)
    {
        bool adquirido = false;
        try
        {
            if (!servicio.TieneAcceso(User, Request.Headers["X-Geco-Perfil"].ToString(), Request.Headers["X-Geco-Menu"].ToString())) return Forbid();
            adquirido = await Exportaciones.WaitAsync(0, ct);
            if (!adquirido) return StatusCode(429, new { mensaje = "Hay otras exportaciones en curso. Intente nuevamente en unos instantes." });
            var filas = await servicio.Todos(filtro, ct);
            var archivo = IvaVentasExcel.Generar(filas, filtro, ct);
            Response.Headers.CacheControl = "no-store, private";
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            logger.LogInformation("IVA Ventas Excel generado: {Cantidad} registros; referencia {Ref}", filas.Count, HttpContext.TraceIdentifier);
            return File(archivo, IvaVentasExcel.Mime, $"Libro_IVA_Ventas_{filtro.PeriodoTexto}.xlsx");
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (IvaVentasDatoInvalidoException ex)
        {
            logger.LogError("Dato IVA inválido en exportación. Campo: {Campo}; referencia: {Ref}", ex.Campo, HttpContext.TraceIdentifier);
            return UnprocessableEntity(new { codigo = IvaVentasDatoInvalidoException.Codigo, campo = ex.Campo, mensaje = ex.Message });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogError("Exportación IVA rechazada. Tipo: {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
            return Problem("No se pudo generar el Excel completo. No se entregó un archivo parcial. Vuelva a consultar o contacte al administrador.", statusCode: 503);
        }
        finally { if (adquirido) Exportaciones.Release(); }
    }
}
