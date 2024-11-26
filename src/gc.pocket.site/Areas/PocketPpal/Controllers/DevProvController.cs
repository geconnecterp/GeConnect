using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class DevProvController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<RPRController> _logger;
        private readonly IProductoServicio _productoServicio;
        private readonly AppSettings _settings;

        public DevProvController(IOptions<AppSettings> options, IHttpContextAccessor context, IOptions<MenuSettings> options1,
            ILogger<RPRController> logger, IProductoServicio productoServicio, IDepositoServicio depositoServicio) : base(options, context)
        {
            _menuSettings = options1.Value;
            _logger = logger;
            _productoServicio = productoServicio;
            _settings = options.Value;
        }

        public IActionResult Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            string volver = Url.Action("cprev", "almacen", new { area = "gestion" });
            ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Devoluciones de Proveedores", VolverUrl = volver ?? "#" };

            return View();
        }
    }
}
