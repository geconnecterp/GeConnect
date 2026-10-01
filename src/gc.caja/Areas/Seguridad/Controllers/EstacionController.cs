using gc.caja.Models.Estacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace gc.caja.Areas.Seguridad.Controllers;

[Area("Seguridad"), AllowAnonymous]
public sealed class EstacionController(EstacionPuestoServicio puestos) : Controller
{
    [HttpPost, RequestSizeLimit(4096)]
    public IActionResult Preparar([FromBody] EstacionPuesto? puesto)
    {
        Response.Headers.CacheControl = "no-store";
        if (puesto == null || !ModelState.IsValid) return BadRequest(new { mensaje = "La configuración del puesto es inválida." });
        try { return Json(new { pase = puestos.Preparar(puesto) }); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { mensaje = ex.Message }); }
    }

    [HttpGet]
    public IActionResult Iniciar(string? pase)
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        var puesto = puestos.Consumir(pase);
        if (puesto == null) return Content("El inicio venció o ya fue utilizado. Abra nuevamente el iniciador de GECO Caja.", "text/plain; charset=utf-8");
        if (User.Identity?.IsAuthenticated == true)
        {
            var actual = puestos.Obtener(HttpContext);
            if (actual != null && JsonSerializer.Serialize(actual) == JsonSerializer.Serialize(puesto))
                return RedirectToAction("Index", "Home", new { area = "" });
            return Content("Hay una sesión abierta para otro puesto. Cierre la sesión de Caja y vuelva a ejecutar el iniciador.", "text/plain; charset=utf-8");
        }
        HttpContext.Session.Clear();
        puestos.Guardar(HttpContext, puesto);
        return RedirectToAction("Login", "Token", new { area = "Seguridad" });
    }
}
