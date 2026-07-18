using gc.auth.remoto.Models;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace gc.auth.remoto.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppSettings _appSettings;

        public HomeController(IOptions<AppSettings> appSettings, ILogger<HomeController> logger)
        {
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Token", new { area = "Seguridad" });
            }

            // Pass API settings to the view so they can be emitted as <meta> tags.
            // This means JS never has a hardcoded URL — it reads from the DOM.
            ViewData["ApiHubUrl"] = _appSettings.HubUrl;
            ViewBag.RutaApi = "/visor-api/solicitudes/";
            ViewData["AuthenticatedUserId"] = User.FindFirst("user")?.Value ?? User.Identity.Name;
            ViewData["AuthenticatedUserName"] = User.FindFirst("nya")?.Value
                ?? ViewData["AuthenticatedUserId"];
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
