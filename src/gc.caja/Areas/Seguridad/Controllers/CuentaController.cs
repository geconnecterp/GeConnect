using gc.caja.Controllers;
using gc.caja.Models.Cuenta;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Seguridad;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.caja.Areas.Seguridad.Controllers;

[Area("Seguridad"), Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CuentaController : ControladorBaseCaja
{
    private readonly IConfiguracionSeguridadServicio _servicio;
    public CuentaController(IConfiguracionSeguridadServicio servicio, IOptions<AppSettings> options,
        IHttpContextAccessor context, ILogger<CuentaController> logger) : base(options, context, logger)
        => _servicio = servicio;

    [HttpGet]
    public Task<IActionResult> Index() => Mostrar(false);
    [HttpGet]
    public Task<IActionResult> ClaveObligatoria() => Mostrar(true);

    private async Task<IActionResult> Mostrar(bool obligatoria)
    {
        if (obligatoria != AccesoCuentaCaja.Forzada(User))
            return RedirectToAction(AccesoCuentaCaja.Forzada(User) ? nameof(ClaveObligatoria) : nameof(Index));
        var model = new CuentaViewModel {
            Operador = OperadorCuenta.Desde(User), Obligatoria = obligatoria, Vencida = AccesoCuentaCaja.Vencida(User)
        };
        model.Operador.Perfil = UserPerfilSeleccionado?.perfil_descripcion ?? "No informado";
        try { model.Politica = await _servicio.ObtenerPoliticaClave(TokenCookie); }
        catch (UnauthorizedException) { return RedirectToAction("Login", "Token"); }
        catch (Exception) {
            // No registrar excepciones que puedan incluir cuerpos o credenciales de la API.
            _logger?.LogWarning("No se pudo consultar la política de claves en Caja.");
            model.Error = "No se pudieron consultar los requisitos de seguridad. Reintentá en unos instantes.";
        }
        return View("Index", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CambiarClave(CuentaViewModel model) => Guardar(model, false);
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CambiarClaveObligatoria(CuentaViewModel model) => Guardar(model, true);

    private async Task<IActionResult> Guardar(CuentaViewModel model, bool obligatoria)
    {
        if (AccesoCuentaCaja.Forzada(User) != obligatoria)
            return Json(new { ok = false, warn = true, msg = "El tipo de cambio de contraseña no corresponde a esta sesión." });
        if (!obligatoria && string.IsNullOrWhiteSpace(model.ClaveActual))
            return Invalido("Debe ingresar la contraseña actual.", "ClaveActual");
        if (string.IsNullOrWhiteSpace(model.ClaveNueva))
            return Invalido("Debe ingresar la contraseña nueva.", "ClaveNueva");
        if ((!obligatoria && model.ClaveActual.Length > 128) || model.ClaveNueva.Length > 128 || model.ConfirmacionClave?.Length > 128)
            return Invalido("La contraseña supera la longitud máxima admitida (128 caracteres).", "ClaveNueva");
        if (!string.Equals(model.ClaveNueva, model.ConfirmacionClave, StringComparison.Ordinal))
            return Invalido("La confirmación no coincide con la contraseña nueva.", "ConfirmacionClave");

        try {
            // La API y el SP aplican la política vigente, historial, vencimiento y contraseña actual.
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var resultado = obligatoria
                ? await _servicio.CambiarClaveForzada(new CambioClaveForzadaRequestDto { ClaveNueva = model.ClaveNueva }, TokenCookie, ip)
                : await _servicio.CambiarClave(new CambioClaveRequestDto { ClaveActual = model.ClaveActual, ClaveNueva = model.ClaveNueva }, TokenCookie, ip);
            if (resultado.resultado != 0)
                return Json(new { ok = false, warn = resultado.resultado > 0, msg = resultado.resultado_msj, focus = resultado.resultado_setfocus });

            var etiqueta = Etiqueta;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!string.IsNullOrEmpty(etiqueta)) Response.Cookies.Delete(etiqueta, new CookieOptions { Path = "/" });
            HttpContext.Session.Clear();
            return Json(new { ok = true, msg = "La contraseña se modificó correctamente. Ingresá nuevamente.",
                redirect = Url.Action("Login", "Token", new { area = "Seguridad", cambioClave = "ok" }) });
        }
        catch (UnauthorizedException) {
            return StatusCode(401, new { ok = false, msg = "La sesión venció. Ingresá nuevamente.",
                redirect = Url.Action("Login", "Token", new { area = "Seguridad" }) });
        }
        catch (Exception) {
            _logger?.LogWarning("No se pudo comprobar el resultado del cambio de contraseña en Caja.");
            return StatusCode(503, new { ok = false, incierto = true,
                msg = "No se pudo comprobar el resultado. Ingresá nuevamente para verificar tu acceso antes de reintentar.",
                redirect = Url.Action("Login", "Token", new { area = "Seguridad" }) });
        }
    }
    private JsonResult Invalido(string msg, string focus) => Json(new { ok = false, warn = true, msg, focus });
}
