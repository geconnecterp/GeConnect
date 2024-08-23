using gc.infraestructura.Core.EntidadesComunes.Options;
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
            if (User.Identity.IsAuthenticated)
            {
                //var mod = ObtenerModulo("RPR");
                //if (mod != null)
                //{
                //    ViewBag.AppItem = mod;
                //    List<ProductoDto> entidades = new List<ProductoDto>();
                //    var lista = new StaticPagedList<ProductoDto>(entidades, 1, 20, entidades.Count);//productos.Item2.CurrentPage, productos.Item2.PageSize, productos.Item2.TotalCount);


                //    //return new GridCore<ProductoDto>() { ListaDatos = lista, CantidadReg = LimiteAvalancha, PaginaActual = page, CantidadPaginas = LimiteAvalancha / 20, Sort = sort, SortDir = sortdir.Equals("ASC") ? "DESC" : "ASC" };
                //   var grilla =  new GridCore<ProductoDto>() { ListaDatos = lista, CantidadReg = 100, PaginaActual = 1, CantidadPaginas = 100 / 20, Sort = "P_Id", SortDir = "ASC" };
                //    return View(grilla);
                //}
                //else
                //{
                //    return RedirectToAction("Index", "home", new { area = "", error = "No se localizó el Módulo. Verifique su funcionalidad y/o configuración." });
                //}
                return View();
            }
            else
            {
                return RedirectToAction("Index", "home", new { area = "", error = "Debe Autenticarse para acceder." });
            }
        }

        [HttpGet]
        public IActionResult BOXALM()
        {
            return View();
        }
        [HttpGet]
        public IActionResult BOXCMB()
        {
            return View();
        }
        [HttpGet]
        public IActionResult RTI()
        {
            return View();
        }
        [HttpGet]
        public IActionResult TI()
        {
            return View();
        }
        [HttpGet]
        public IActionResult CTLTI()
        {
            return View();
        }
        [HttpGet]
        public IActionResult REXPED()
        {
            return View();
        }
        [HttpGet]
        public IActionResult REXTI()
        {
            return View();
        }
        [HttpGet]
        public IActionResult CTLREXTI()
        {
            return View();
        }
        [HttpGet]
        public IActionResult DPR()
        {
            return View();
        }
        [HttpGet]
        public IActionResult OR()
        {
            return View();
        }
        [HttpGet]
        public IActionResult CTLOR()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ROTULO()
        {
            return View();
        }
        [HttpGet]
        public IActionResult PREFACTURA()
        {
            return View();
        }
        [HttpGet]
        public IActionResult INFO()
        {
            return View();
        }
        [HttpGet]
        public IActionResult INV()
        {
            return View();
        }

    }
}
