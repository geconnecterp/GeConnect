using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace gc.sitio.Controllers
{
    public class HomeController : ControladorBase
    {
        private new readonly IHttpContextAccessor _context;
        private readonly DocsManager _docsManager;
        public HomeController(ILogger<HomeController> logger, IOptions<AppSettings> options, IHttpContextAccessor context, 
            IOptions<DocsManager> docMgr) : base(options, context)
        {
            _context = context;
            _docsManager = docMgr.Value;
        }

        public IActionResult Index()
        {
            ViewBag.Title = NombreSitio;
            if (UserPerfiles.Count() == 0) {
                return RedirectToAction("login", "token", new { area = "seguridad" });
            }
            ViewData["Titulo"] = "Bienvenidos al GECONet";
            ViewBag.Administracion = AdministracionId;
            ViewBag.RepoUrl = _docsManager.ApiReporteUrl;
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
