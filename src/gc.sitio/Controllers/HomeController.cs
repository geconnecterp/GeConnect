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

        public HomeController(ILogger<HomeController> logger,IOptions<AppSettings> options):base(options) 
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.Title=NombreSitio;
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
