using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Seguridad;
using gc.sitio.Controllers;
using gc.sitio.Models.Configuracion;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Configuracion.Controllers
{
    [Area("Configuracion")]
    [Authorize]
    public class ConfiguracionController : ControladorBase
    {
        private readonly IConfiguracionSeguridadServicio _servicio;

        public ConfiguracionController(IConfiguracionSeguridadServicio servicio,
            IOptions<AppSettings> options, IHttpContextAccessor context,
            ILogger<ConfiguracionController> logger) : base(options, context, logger)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Titulo"] = "CONFIGURACIÓN";
            var model = new CambioClaveViewModel
            {
                Politica = await _servicio.ObtenerPoliticaClave(TokenCookie)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarClave(CambioClaveViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ClaveActual))
                return Json(new { ok = false, warn = true, msg = "Debe ingresar la contraseña actual.", focus = "ClaveActual" });
            if (string.IsNullOrWhiteSpace(model.ClaveNueva))
                return Json(new { ok = false, warn = true, msg = "Debe ingresar la contraseña nueva.", focus = "ClaveNueva" });
            if (!string.Equals(model.ClaveNueva, model.ConfirmacionClave, StringComparison.Ordinal))
                return Json(new { ok = false, warn = true, msg = "La confirmación no coincide con la contraseña nueva.", focus = "ConfirmacionClave" });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var resultado = await _servicio.CambiarClave(new CambioClaveRequestDto
            {
                ClaveActual = model.ClaveActual,
                ClaveNueva = model.ClaveNueva
            }, TokenCookie, ip);

            if (resultado.resultado != 0)
                return Json(new { ok = false, warn = resultado.resultado > 0, msg = resultado.resultado_msj, focus = resultado.resultado_setfocus });

            var etiqueta = Etiqueta;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!string.IsNullOrWhiteSpace(etiqueta))
                Response.Cookies.Delete(etiqueta, new CookieOptions { Path = "/" });
            HttpContext.Session.Clear();

            return Json(new
            {
                ok = true,
                msg = "La contraseña se modificó correctamente. Ingrese nuevamente.",
                redirect = Url.Action("Login", "Token", new { area = "Seguridad", cambioClave = "ok" })
            });
        }
    }
}
