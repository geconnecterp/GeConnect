using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Consultas.Controllers
{
    /// <summary>
    /// Corresponde el controlador a las CUENTAS COMERCIALES
    /// </summary>
    [Area("Consultas")]
    public class CuentasCController : ControladorBase
    {
        public CuentasCController(IOptions<AppSettings> options, IHttpContextAccessor contexto ):base(options,contexto)
        {
            
        }

        public IActionResult Index()
        {

            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            return View();

        }
    }
}
