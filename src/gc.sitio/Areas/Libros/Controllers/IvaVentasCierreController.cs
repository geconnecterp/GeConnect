using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Implementacion.Libros;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Libros.Controllers;

[Area("Libros"), AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class IvaVentasCierreController(IOptions<AppSettings> options, IHttpContextAccessor contexto,
    ILogger<IvaVentasCierreController> logger, IvaVentasCierreCliente cliente) : ControladorBase(options, contexto, logger)
{
    private string Perfil() => JsonConvert.DeserializeObject<PerfilUserDto>(HttpContext.Session.GetString("UserPerfilSeleccionado") ?? "{}")?.perfil_id
        ?? throw new UnauthorizedAccessException("Debe seleccionar un perfil.");
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!VerificarAutenticacion(out var redirect)) return redirect;
        ViewData["Titulo"] = "Libro IVA Ventas - Cierres";
        try { await cliente.Acceso(TokenCookie, Perfil(), options.Value.MenuId, ct); }
        catch (IvaVentasCierreHttpException ex) when (ex.Estado is 401 or 403) { return StatusCode(ex.Estado, ex.Message); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning("No se pudo abrir Cierre IVA. Tipo {Tipo}", ex.GetType().Name);
            ViewBag.Error = "No se pudo verificar el acceso al módulo. Revise la conexión con la API y vuelva a ingresar.";
        }
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Estado(CancellationToken ct)
    {
        if (!VerificarAutenticacion(out _)) return Unauthorized(new { mensaje = "La sesión venció. Ingrese nuevamente." });
        try { return PartialView("_Historial", await cliente.Estado(TokenCookie, Perfil(), options.Value.MenuId, ct)); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex, false); }
    }
    [HttpPost]
    public async Task<IActionResult> Cerrar(IvaVentasCierrePedido pedido, CancellationToken ct)
    {
        if (!VerificarAutenticacion(out _)) return Unauthorized(new { mensaje = "La sesión venció. Ingrese nuevamente." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Revise el período que se desea cerrar." });
        try { pedido.Validar(); return Json(await cliente.Cerrar(pedido, TokenCookie, Perfil(), options.Value.MenuId, ct)); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex, true); }
    }
    [HttpPost]
    public Task<IActionResult> Rg3685(IvaVentasCierrePedido pedido, CancellationToken ct) => Descargar(pedido, false, ct);
    [HttpPost]
    public Task<IActionResult> Duplicados(IvaVentasCierrePedido pedido, CancellationToken ct) => Descargar(pedido, true, ct);
    private async Task<IActionResult> Descargar(IvaVentasCierrePedido pedido, bool duplicados, CancellationToken ct)
    {
        if (!VerificarAutenticacion(out _)) return Unauthorized(new { mensaje = "La sesión venció. Ingrese nuevamente." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Seleccione un período cerrado válido." });
        try
        {
            pedido.Validar();
            var file = duplicados
                ? await cliente.Duplicados(pedido, TokenCookie, Perfil(), options.Value.MenuId, ct)
                : await cliente.Rg3685(pedido, TokenCookie, Perfil(), options.Value.MenuId, ct);
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(file, duplicados ? "text/plain; charset=utf-8" : "application/zip",
                $"IVA_Ventas_{(duplicados ? "Duplicados" : "3685")}_{IvaVentasCierrePeriodo.Texto(pedido.Periodo)}.{(duplicados ? "txt" : "zip")}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex, false); }
    }
    private IActionResult Error(Exception ex, bool cierre)
    {
        if (ex is IvaVentasCierreHttpException http) return StatusCode(http.Estado, new { mensaje = http.Message });
        if (ex is UnauthorizedAccessException) return StatusCode(403, new { mensaje = ex.Message });
        if (ex is ArgumentException) return BadRequest(new { mensaje = ex.Message });
        logger.LogWarning("IVA cierre fallido. Operación {Operacion}; tipo {Tipo}; referencia {Ref}", cierre ? "cerrar" : "consultar/exportar", ex.GetType().Name, HttpContext.TraceIdentifier);
        return StatusCode(503, new { mensaje = cierre
            ? "No se pudo confirmar el resultado del cierre. Actualice el historial antes de volver a intentarlo. No hubo reintentos automáticos."
            : "No se pudo completar la consulta o descarga. Revise la conexión e intente nuevamente." });
    }
}
