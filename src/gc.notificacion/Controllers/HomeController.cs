using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.notificacion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace gc.notificacion.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _contexto;
        private readonly AppSettings _appSettings;
        public HomeController(ILogger<HomeController> logger, IHttpContextAccessor context,IOptions<AppSettings> options)
        {
            _logger = logger;
            _contexto = context;
            _appSettings = options.Value;
        }

        public IActionResult Index()
        {
            _logger.Log(LogLevel.Information, $"CONTEXTO Header: {_contexto.HttpContext.Request.Headers.ToString()} Body: {_contexto.HttpContext.Request.ToString()}");
            ViewBag.Title=_appSettings.Nombre;
            return View();
        }

        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
