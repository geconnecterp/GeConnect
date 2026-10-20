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
public class IvaComprasCierreController(IOptions<AppSettings> options, IHttpContextAccessor contexto,
    ILogger<IvaComprasCierreController> logger, IvaComprasCierreCliente cliente) : ControladorBase(options, contexto, logger)
{
    private string Perfil() => JsonConvert.DeserializeObject<PerfilUserDto>(HttpContext.Session.GetString("UserPerfilSeleccionado") ?? "{}")?.perfil_id
        ?? throw new UnauthorizedAccessException("Debe seleccionar un perfil.");

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!VerificarAutenticacion(out var redirect)) return redirect;
        ViewData["Titulo"] = "Libro IVA Compras - Cierres";
        try { await cliente.Acceso(TokenCookie, Perfil(), options.Value.MenuId, ct); }
        catch (IvaComprasCierreHttpException ex) when (ex.Estado is 401 or 403) { return StatusCode(ex.Estado, ex.Message); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning("No se pudo abrir Cierre IVA Compras. Tipo {Tipo}", ex.GetType().Name);
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
    public async Task<IActionResult> Cerrar(IvaComprasCierrePedido pedido, CancellationToken ct)
    {
        if (!VerificarAutenticacion(out _)) return Unauthorized(new { mensaje = "La sesión venció. Ingrese nuevamente." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Revise el período que se desea cerrar." });
        try { pedido.Validar(); return Json(await cliente.Cerrar(pedido, TokenCookie, Perfil(), options.Value.MenuId, ct)); }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex, true); }
    }
    [HttpPost]
    public async Task<IActionResult> Rg3685(IvaComprasCierreDescargaPedido pedido, CancellationToken ct)
    {
        if (!VerificarAutenticacion(out _)) return Unauthorized(new { mensaje = "La sesión venció. Ingrese nuevamente." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Seleccione un registro cerrado válido." });
        try
        {
            pedido.Validar();
            var file = await cliente.Rg3685(pedido, TokenCookie, Perfil(), options.Value.MenuId, ct);
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(file, "application/zip", $"IVA_Compras_3685_{IvaComprasCierrePeriodo.Texto(pedido.Periodo)}.zip");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested) { return Error(ex, false); }
    }
    private IActionResult Error(Exception ex, bool cierre)
    {
        if (ex is IvaComprasCierreHttpException http) return StatusCode(http.Estado, new { mensaje = http.Message });
        if (ex is UnauthorizedAccessException) return StatusCode(403, new { mensaje = ex.Message });
        if (ex is ArgumentException) return BadRequest(new { mensaje = ex.Message });
        logger.LogWarning("IVA Compras cierre fallido. Operación {Operacion}; tipo {Tipo}; referencia {Ref}",
            cierre ? "cerrar" : "consultar/exportar", ex.GetType().Name, HttpContext.TraceIdentifier);
        return StatusCode(503, new { mensaje = cierre
            ? "No se pudo confirmar el resultado del cierre. Actualice el historial antes de volver a intentarlo. No hubo reintentos automáticos."
            : "No se pudo completar la consulta o descarga. Revise la conexión e intente nuevamente." });
    }
}

