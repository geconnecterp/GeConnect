using gc.api.core.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Libros;

[Authorize, ApiController, Route("api/iva-compras-resumen")]
public class IvaComprasResumenController(IvaComprasResumenServicio servicio, ILogger<IvaComprasResumenController> logger) : ControllerBase
{
    private bool Permitido() => servicio.TieneAcceso(User, Request.Headers["X-Geco-Perfil"].ToString(), Request.Headers["X-Geco-Menu"].ToString());
    [HttpGet("acceso")]
    public IActionResult Acceso() => Permitido() ? Ok() : Forbid();

    [HttpPost("consultar")]
    public async Task<IActionResult> Consultar(IvaComprasResumenFiltro filtro, CancellationToken ct)
    {
        try
        {
            if (!Permitido()) return Forbid();
            return Ok(await servicio.Consultar(filtro, ct));
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogError("Resumen IVA fallido. Tipo {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
            return StatusCode(503, new { mensaje = "No se pudo consultar el resumen de IVA Compras. No se muestran datos parciales. Verifique la conexión y el SP con el administrador." });
        }
    }
}
