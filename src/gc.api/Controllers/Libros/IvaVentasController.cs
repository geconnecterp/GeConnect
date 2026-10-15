using gc.api.core.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Libros;

[Authorize]
[ApiController]
[Route("api/iva-ventas")]
public class IvaVentasController(IvaVentasServicio servicio, ILogger<IvaVentasController> logger) : ControllerBase
{
    private bool Permitido() => servicio.TieneAcceso(User, Request.Headers["X-Geco-Perfil"].ToString(), Request.Headers["X-Geco-Menu"].ToString());

    [HttpGet("opciones")]
    public async Task<IActionResult> Opciones(CancellationToken ct)
    {
        try
        {
            if (!Permitido()) return Forbid();
            return Ok(await servicio.Opciones(ct));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogError("Error cargando listas IVA. Tipo: {Tipo}; referencia: {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
            return Problem("No se pudieron cargar las listas de IVA. Verifique los procedimientos de filtro.", statusCode: 503);
        }
    }

    [HttpPost("consultar")]
    public async Task<IActionResult> Consultar(IvaVentasFiltro filtro, CancellationToken ct)
    {
        try
        {
            if (!Permitido()) return Forbid();
            return Ok(await servicio.Consultar(filtro, ct));
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (IvaVentasDatoInvalidoException ex)
        {
            logger.LogError("Dato IVA inválido. Campo: {Campo}; referencia: {Ref}", ex.Campo, HttpContext.TraceIdentifier);
            return UnprocessableEntity(new { codigo = IvaVentasDatoInvalidoException.Codigo, campo = ex.Campo, mensaje = ex.Message });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            logger.LogError("Error consultando IVA. Tipo: {Tipo}; referencia: {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
            return Problem("No se pudo consultar el Libro IVA Ventas. Verifique el SP y vuelva a intentar.", statusCode: 503);
        }
    }
}
