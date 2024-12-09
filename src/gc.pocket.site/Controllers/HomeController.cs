using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace gc.pocket.site.Controllers
{
    public class HomeController : ControladorBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger,IOptions<AppSettings> options, IHttpContextAccessor context) :base(options,context, logger) 
        {
            _logger = logger;
        }

        public IActionResult Index(string error = "", string warn = "", string info = "")
        {
            //if (string.IsNullOrEmpty(TokenCookie))
            //{
            //    return RedirectToAction("Logout", "token", new { area = "Seguridad" });
            //}
            PresentaMensaje(error, warn, info);
            ViewBag.Botones =new  List<AppItem>();
            return View();
        }

             

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
