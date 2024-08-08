using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Options;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class InfoProdController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly IProductoServicio _productoServicio;
        public InfoProdController(IOptions<AppSettings> options, IOptions<MenuSettings> options1,
            IHttpContextAccessor context, IProductoServicio productoServicio) : base(options, options1, context)
        {
            _menuSettings = options1.Value;
            _productoServicio = productoServicio;
        }

        /// <summary>
        /// CODIGO DEL MODULO "INFO"
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals("info", StringComparison.OrdinalIgnoreCase));
                if (modulo == null)
                {
                    throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
                }
                return View(modulo);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("index", "home", new { area = string.Empty });
            }
        }

        [HttpGet]
        public IActionResult Producto()
        {

            return View();
        }

        [HttpGet]
        public IActionResult Depo()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Box()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Ul()
        {
            return View();
        }

        //[HttpPost]
        //public JsonResult InfoProdStkD(string id)
        //{
        //    try
        //    {
        //        var regs = _productoServicio.InfoProdStkD(id, AdministracionId);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["error"] = ex.Message;
        //        // return RedirectToAction("index", "InfoProd", new { area = "PocketPpal" });
        //    }
        //}
    }
}
