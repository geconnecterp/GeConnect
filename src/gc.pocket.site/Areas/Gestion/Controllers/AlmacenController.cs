using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.X509;

namespace gc.pocket.site.Areas.Gestion.Controllers
{
    [Area("Gestion")]
    public class AlmacenController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private new readonly ILogger<AlmacenController> _logger;

        public AlmacenController(ILogger<AlmacenController> logger, IOptions<MenuSettings> options,
            IOptions<AppSettings> options1, IHttpContextAccessor context) : base(options1, options, context, logger)
        {
            _logger = logger;
            _menuSettings = options.Value;

        }

        [HttpGet]
        public IActionResult RPR()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            var sigla = "rpr";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            string? volver = Url.Action("index", "home", new { area = "" });           
            ViewBag.AppItem = new AppItem { Nombre = modulo!.Nombre, VolverUrl = volver ?? "#" };
            return View("gen", modulo); //GEN HAY QUE UTILIZARLO CUANDO SE TIENE SECCIONES VARIAS EN EL MISMO "MODULO"
        }

        [HttpGet]
        public IActionResult BOXALM()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }


            var sigla = "box alm";
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
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

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
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            var sigla = "rti";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            string? volver = Url.Action("index", "home", new { area = "" });
            ViewBag.AppItem = new AppItem { Nombre = modulo!.Nombre, VolverUrl = volver ?? "#" };
            return View("gen", modulo);
        }
        [HttpGet]
        public IActionResult TI()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }


            var sigla = "ti";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View("gen", modulo);
        }
        [HttpGet]
        public IActionResult CTLTI()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            var sigla = "ctl ti";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View("gen", modulo);
        }
        [HttpGet]
        public IActionResult REXPED()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

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
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

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
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }


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
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

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
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

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
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }


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
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

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
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

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
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }


            var sigla = "info";
            string? volver = Url.Action("index", "home", new { area = "" });
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;

            return View(modulo);
        }
        [HttpGet]
        public IActionResult INV()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }


            var sigla = "inv";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }

        [HttpGet]
        public IActionResult cprev()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }


            var sigla = "cprev";
            string? volver = Url.Action("index", "home", new { area = "" });
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;

            return View("gen", modulo);
        }
    }
}
