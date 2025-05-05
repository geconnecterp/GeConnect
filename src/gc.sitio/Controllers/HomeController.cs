using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace gc.sitio.Controllers
{
    public class HomeController : ControladorBase
    {
        private readonly ILogger<HomeController> _logger;
        private new readonly IHttpContextAccessor _context;

        public HomeController(ILogger<HomeController> logger, IOptions<AppSettings> options, IHttpContextAccessor context) : base(options, context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Title = NombreSitio;
            if (UserPerfiles.Count() == 0) {
                return RedirectToAction("login", "token", new { area = "seguridad" });
            }
            ViewData["Titulo"] = "Bienvenidos al GECONet";
            ViewBag.Administracion = AdministracionId;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
