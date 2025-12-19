using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.pocket.site.Areas.Gestion.Controllers
{
    [Area("Gestion")]
    public class InventarioController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<AlmacenController> _logger;

        public InventarioController(ILogger<AlmacenController> logger, IOptions<MenuSettings> options,
            IOptions<AppSettings> options1, IHttpContextAccessor context) : base(options1, options, context, logger)
        {
            _logger = logger;
            _menuSettings = options.Value;

        }
        public IActionResult Index()
        {
            var sigla = "inv";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
    }
}
