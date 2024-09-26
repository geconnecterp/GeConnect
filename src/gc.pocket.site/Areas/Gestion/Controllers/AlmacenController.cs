using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.pocket.site.Areas.Gestion.Controllers
{
    [Area("Gestion")]
    public class AlmacenController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<AlmacenController> _logger;

        public AlmacenController(ILogger<AlmacenController> logger, IOptions<MenuSettings> options,
            IOptions<AppSettings> options1, IHttpContextAccessor context) : base(options1, options, context)
        {
            _logger = logger;
            _menuSettings = options.Value;

        }

        [HttpGet]
        public IActionResult RPR()
        {
            var sigla = "rpr";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View("gen",modulo);
        }

        [HttpGet]
        public IActionResult BOXALM()
        {
            var sigla = "boxalm";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
        [HttpGet]
        public IActionResult BOXCMB()
        {
            var sigla = "boxcmb";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
        [HttpGet]
        public IActionResult RTI()
        {
            var sigla = "rti";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
        [HttpGet]
        public IActionResult TI()
        {
            var sigla = "ti";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View("gen",modulo);
        }
        [HttpGet]
        public IActionResult CTLTI()
        {
            var sigla = "ctl ti";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
        [HttpGet]
        public IActionResult REXPED()
        {
            var sigla = "rex ped";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
        [HttpGet]
        public IActionResult REXTI()
        {
            var sigla = "rexti";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
        [HttpGet]
        public IActionResult CTLREXTI()
        {
            var sigla = "ctlrexti";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
        [HttpGet]
        public IActionResult DPR()
        {
            var sigla = "drp";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
        [HttpGet]
        public IActionResult OR()
        {
            var sigla = "or";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
        [HttpGet]
        public IActionResult CTLOR()
        {
            var sigla = "ctlor";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
        [HttpGet]
        public IActionResult ROTULO()
        {
            var sigla = "rotulo";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
        [HttpGet]
        public IActionResult PREFACTURA()
        {
            var sigla = "prefactura";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
        [HttpGet]
        public IActionResult INFO()
        {
            var sigla = "info";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }
        [HttpGet]
        public IActionResult INV()
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
