using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Asientos.Controllers
{
    [Area("Asientos")]
    public class AsientoTemporalController : ControladorBase
    {
        public AsientoTemporalController(IOptions<AppSettings> options,IHttpContextAccessor contexto,ILogger<AsientoTemporalController> logger):base(options,contexto,logger)
        {

        }
        public IActionResult Index()
        {
            string titulo = "Asientos Temporales";
            ViewData["Titulo"] = titulo;
            return View();
        }
    }
}
