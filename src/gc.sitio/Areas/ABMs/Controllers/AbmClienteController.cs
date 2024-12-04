using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.ABMs.Controllers
{
    [Area("ABMs")]
    public class AbmClienteController : ClienteControladorBase
    {
        private readonly AppSettings _settings;
        private readonly ITipoNegocioServicio _tipoNegocioServicio;
        private readonly IZonaServicio _zonaServicio;

        public AbmClienteController(IZonaServicio zonaServicio, ITipoNegocioServicio tipoNegocioServicio, IOptions<AppSettings> options, 
                                    IHttpContextAccessor accessor) : base(options, accessor)
        {
            _settings = options.Value;
            _tipoNegocioServicio = tipoNegocioServicio;
            _zonaServicio = zonaServicio;
        }

        [HttpGet]
        public async Task<IActionResult> Index(bool actualizar = false)
        {
            MetadataGrid metadata;

            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            if (TipoNegocioLista.Count == 0 || actualizar)
            {
                ObtenerTiposNegocio(_tipoNegocioServicio);
            }

            if (ZonasLista.Count == 0 || actualizar)
            {
                ObtenerZonas(_zonaServicio);
            }
            return View();
        }
    }
}
