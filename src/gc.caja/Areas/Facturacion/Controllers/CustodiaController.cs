using gc.caja.Controllers;
using gc.caja.Models.Administracion;
using gc.caja.Models.Estacion;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.caja.Areas.Facturacion.Controllers;

[Area("Facturacion"), Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CustodiaController(IOptions<AppSettings> options, IHttpContextAccessor context,
    ILogger<CustodiaController> logger, CustodiaApiServicio servicio, EstacionPuestoServicio estaciones)
    : ControladorBaseCaja(options, context, logger)
{
    private string? ValidarContexto()
    {
        if (User.Identity?.IsAuthenticated != true || !TieneTokenValido() || string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(AdministracionId))
            return "La sesión expiró. Ingrese nuevamente.";
        // Configuración local hidratada por el iniciador. No requiere tener el PV abierto.
        var sucursalPuesto = estaciones.Obtener(HttpContext)?.AdmId ?? CajaActual?.AdmId;
        if (string.IsNullOrWhiteSpace(sucursalPuesto)) return "No se identificó la configuración del puesto. Ingrese mediante el iniciador de Caja.";
        if (!string.Equals(sucursalPuesto.Trim(), AdministracionId.Trim(), StringComparison.OrdinalIgnoreCase))
            return "La sucursal de la sesión no coincide con la configuración del puesto. Ingrese nuevamente desde el iniciador.";
        return null;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var error = ValidarContexto();
        if (error != null) { TempData["error"] = error; return RedirectToAction("Index", "Home", new { area = "" }); }
        ViewBag.Sucursal = AdministracionId + " · " + AdministracionName;
        ViewBag.Operador = UserName;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Consulta()
    {
        if (ValidarContexto() is { } error) return Unauthorized(new { ok = false, mensaje = error });
        try { return Json(new { ok = true, data = await servicio.Consultar(TokenCookie) }); }
        catch (Exception ex) { logger.LogError(ex, "[CUSTODIA] Consulta pendiente fallida, sucursal={Sucursal}", AdministracionId); return StatusCode(503, new { ok = false, mensaje = "No se pudieron consultar las rendiciones. Puede actualizar para volver a intentarlo." }); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Corregir([FromBody] CustodiaOperacionDto operacion) => Ejecutar(false, operacion);
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Entregar([FromBody] CustodiaOperacionDto operacion) => Ejecutar(true, operacion);

    private async Task<IActionResult> Ejecutar(bool entrega, CustodiaOperacionDto operacion)
    {
        if (ValidarContexto() is { } error) return Unauthorized(new CustodiaResultadoDto(false, error));
        if (!ModelState.IsValid || operacion == null) return BadRequest(new CustodiaResultadoDto(false, "La solicitud está incompleta."));
        return Json(await servicio.Ejecutar(entrega, operacion, TokenCookie));
    }

    [HttpGet]
    public async Task<IActionResult> Acuse(string entrega)
    {
        if (ValidarContexto() is { } error) return Unauthorized(new { mensaje = error });
        if (string.IsNullOrWhiteSpace(entrega) || entrega.Length > 20) return BadRequest(new { mensaje = "Identificador de entrega inválido." });
        try { return File(await servicio.Acuse(entrega, TokenCookie), "application/pdf"); }
        catch (Exception ex) { logger.LogError(ex, "[CUSTODIA] Acuse no disponible. Entrega={Entrega}, Sucursal={Sucursal}", entrega, AdministracionId); return StatusCode(503, new { mensaje = "La entrega conserva su registro. No se pudo obtener el acuse; puede volver a solicitarlo sin confirmar otra entrega." }); }
    }
}
