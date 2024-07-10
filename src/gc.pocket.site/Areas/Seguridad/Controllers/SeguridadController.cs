using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;

namespace gc.pocket.site.Areas.Seguridad.Controllers
{
    public class SeguridadController : Controller
    {
        private readonly ITipoDocumentoServicio _sv;
        private readonly ILogger<SeguridadController> _logger;
        public SeguridadController(ITipoDocumentoServicio servicio, ILogger<SeguridadController> logger)
        {
            _sv = servicio;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Registro()
        {
            var tp = _sv.BuscarAsync(new QueryFilters(), "");
            return View();
        }

        //[HttpPost]
        //[ActionName("Registro")]
        //public IActionResult ResultadoPost(UsuarioDto usuario)
        //{

        //}
    }
}
