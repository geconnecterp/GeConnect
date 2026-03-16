using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.OrdenReparto;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class ORCtlController : PocketControllerBase
    {
        private readonly MenuSettings _menuSettings;

        public ORCtlController(IOptions<AppSettings> options,
            IHttpContextAccessor context,
            ILogger<TrIntController> logger,
            IOptions<MenuSettings> options1) : base(options, context, logger)
        {
            _menuSettings = options1.Value;
        }



        public IActionResult Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            //este viewbag es para que aparezca en la segunda fila del encabezado la leyenda que se quiera.
            //en este caso presenta el numero de autorización pendiente y el proveedor al que le pertenece.
            var sigla = "CTL-OR";
            string? volver = Url.Action("index", "home", new { area = "" });
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;

            return View();
        }


        [HttpGet]
        public IActionResult PresentaProductosOrCtl(string or_compte)
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            if (string.IsNullOrEmpty(or_compte))
            {
                TempData["error"] = "No se recepcionó el Nro de Comprobante de la OR.";
                return RedirectToAction("index");
            }

            // ✅ REFACTORIZADO: Usar ORSession
            //la inicializacion con nuevo comprobante
            var session = new ORSessionDto();
            session.ORComprobanteActual = or_compte;
            session.UltimaActualizacion = DateTime.Now;
            ORSession = session;

            _logger?.LogInformation("📝 OR Seleccionada: {OrCompte}", or_compte);

            var sigla = "OR";
            string? volver = Url.Action("index", "or", new { area = "PocketPpal" });
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));

            if (modulo == null)
            {
                throw new NegocioException("No se logró encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }

            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;
            ViewBag.Compte = session.ORComprobanteActual;

            return View();
        }

        [HttpPost]
        public IActionResult CargaProductosOrCtl(string or_compte)
        {
            
            if (string.IsNullOrEmpty(or_compte))
            {
                TempData["error"] = "No se recepcionó el Nro de Comprobante de la OR.";
                return RedirectToAction("index");
            }
           

        }
    }
}
