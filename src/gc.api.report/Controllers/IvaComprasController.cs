using gc.api.core.Servicios.Libros;
using gc.api.core.Servicios.Reportes;
using gc.infraestructura.Dtos.Libros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.report.Controllers;

[Authorize]
[ApiController]
[Route("api/iva-compras")]
public class IvaComprasController(IvaComprasServicio servicio, ILogger<IvaComprasController> logger) : ControllerBase
{
    private static readonly SemaphoreSlim Exportaciones = new(1, 1);

    [HttpPost("excel")]
    public async Task<IActionResult> Excel(IvaComprasFiltro filtro, CancellationToken ct)
    {
        bool adquirido = false;
        FileStream? archivo = null;
        try
        {
            if (!servicio.TieneAcceso(User, Request.Headers["X-Geco-Perfil"].ToString(), Request.Headers["X-Geco-Menu"].ToString())) return Forbid();
            adquirido = await Exportaciones.WaitAsync(0, ct);
            if (!adquirido) return StatusCode(429, new { mensaje = "Hay otras exportaciones en curso. Intente nuevamente en unos instantes." });
            var filas = await servicio.Todos(filtro, ct);
            archivo = new FileStream(Path.Combine(Path.GetTempPath(), "geco-iva-compras-" + Guid.NewGuid().ToString("N") + ".xlsx"),
                FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
            IvaComprasExcel.Escribir(filas, filtro, archivo, ct);
            archivo.Position = 0;
            Response.Headers.CacheControl = "no-store, private";
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            logger.LogInformation("IVA Compras Excel generado: {Cantidad} registros; referencia {Ref}", filas.Count, HttpContext.TraceIdentifier);
            var resultado = File(archivo, IvaComprasExcel.Mime, $"Libro_IVA_Compras_{filtro.PeriodoTexto}.xlsx");
            archivo = null; // MVC dispone el stream después de completar la transferencia.
            return resultado;
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (IvaComprasDatoInvalidoException ex)
        {
            logger.LogError("Dato IVA inválido en exportación. Campo: {Campo}; referencia: {Ref}", ex.Campo, HttpContext.TraceIdentifier);
            return UnprocessableEntity(new { codigo = IvaComprasDatoInvalidoException.Codigo, campo = ex.Campo, mensaje = ex.Message });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogError("Exportación IVA rechazada. Tipo: {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
            return Problem("No se pudo generar el Excel completo. No se entregó un archivo parcial. Vuelva a consultar o contacte al administrador.", statusCode: 503);
        }
        finally
        {
            if (archivo != null) await archivo.DisposeAsync();
            if (adquirido) Exportaciones.Release();
        }
    }
}
