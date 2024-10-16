using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class RTIController : ControladorBase
    {
        private readonly AppSettings _settings;

        public RTIController(IOptions<AppSettings> option, IHttpContextAccessor context):base(option,context)
        {
                _settings = option.Value;
        }

        public IActionResult Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            string? volver = Url.Action("index", "home", new { area = "" });
            ViewBag.AppItem = new AppItem { Nombre = "Recepción de Transferencia de otra Sucursal", VolverUrl = volver ?? "#" };
            return View();
        }
    }
}
