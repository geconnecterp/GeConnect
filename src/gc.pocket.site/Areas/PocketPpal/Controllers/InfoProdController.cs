using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Seguridad;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.pocket.site.Controllers;
using gc.pocket.site.Models.ViewModels;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NuGet.Packaging.Signing;
using X.PagedList;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class InfoProdController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<InfoProdController> _logger;
        private readonly IProductoServicio _productoServicio;
        private readonly ICuentaServicio _ctaSv;
        private readonly IRubroServicio _rubSv;

        public InfoProdController(IOptions<AppSettings> options, IOptions<MenuSettings> options1,
            IHttpContextAccessor context, IProductoServicio productoServicio, ILogger<InfoProdController> logger,
            ICuentaServicio cta, IRubroServicio rubro) : base(options, options1, context)
        {
            _menuSettings = options1.Value;
            _productoServicio = productoServicio;
            _logger = logger;
            _ctaSv = cta;
            _rubSv = rubro;
        }

        /// <summary>
        /// CODIGO DEL MODULO "INFO"
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index(bool actualizar = false)
        {

            try
            {
                try
                {
                    if (!EstaAutenticado.Item1)
                    {
                        return RedirectToAction("Login", "Token", new { area = "Seguridad" });
                    }
                    if (ProveedoresLista.Count == 0 || actualizar)
                    {
                        ObtenerProveedores();
                    }

                    if (RubroLista.Count == 0 || actualizar)
                    {
                        ObtenerRubros();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en la carga de datos periféricos.");
                    TempData["error"] = "Hubo algún error al intentar cargar la vista de autenticación. Si el problema persiste, avise al administardor.";
                    var lv = new List<AdministracionLoginDto>();
                    ViewBag.Admid = HelperMvc<AdministracionLoginDto>.ListaGenerica(lv);
                    var login = new LoginDto { Fecha = DateTime.Now };
                }

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

        private void ObtenerRubros()
        {
            RubroLista = _rubSv.ObtenerListaRubros(TokenCookie);
        }

        private void ObtenerProveedores()
        {
            //se guardan los proveedores en session. Para ser utilizados posteriormente

            ProveedoresLista = _ctaSv.ObtenerListaProveedores(TokenCookie);
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

        [HttpPost]
        public ActionResult InfoProductoStkD()
        {
            GridCore<InfoProdStkD> datosIP;
            try
            {
                string id = ProductoBase.P_id;
                if (!id.Equals(InfoProdStkDId))
                {
                    InfoProdStkDId = id;

                    var regs = _productoServicio.InfoProductoStkD(id, AdministracionId, TokenCookie).GetAwaiter().GetResult();

                    datosIP = ObtenerInfoProdStkD(regs);

                }
                else
                {
                    datosIP = ObtenerInfoProdStkD(InfoProdStkDRegs);
                }
                return PartialView("_infoProdStkDGrid", datosIP);
            }
            catch (Exception ex)
            {
                TempData["warn"] = ex.Message;
            }
            var lst = new StaticPagedList<InfoProdStkD>(new List<InfoProdStkD>(), 1, 1, 0);
            return View("_infoProdStkDGrid", new GridCore<InfoProdStkD>() { ListaDatos = lst, CantidadReg = 0, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" });

        }

        private GridCore<InfoProdStkD> ObtenerInfoProdStkD(List<InfoProdStkD> listInfoProd)
        {

            var lista = new StaticPagedList<InfoProdStkD>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCore<InfoProdStkD>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }


        [HttpPost]
        public ActionResult InfoProductoStkBoxes()
        {
            GridCore<InfoProdStkBox> datosIP;
            //en documentacion GECO POCKET INFOPROD el valor default de depo = %
            string depo = "%";
            try
            {
                string id = ProductoBase.P_id;
                var ids = InfoProdStkBoxesIds;
                if (!(id.Equals(ids.Item1)))
                {
                    InfoProdStkBoxesIds = (id, depo);

                    var regs = _productoServicio.InfoProductoStkBoxes(id, AdministracionId, depo, TokenCookie).GetAwaiter().GetResult();

                    if (regs == null || regs.Count == 0)
                    {
                        throw new Exception("No se encontró el producto");
                    }
                    else
                    {
                        var data = regs.First();
                        datosIP = ObtenerInfoProdStkBox((id, depo));
                    }
                }
                else
                {
                    datosIP = ObtenerInfoProdStkBox(InfoProdStkBoxesIds, true);
                }
                return PartialView("_infoProdStkBoxGrid", datosIP);
            }
            catch (Exception ex)
            {
                TempData["warn"] = ex.Message;
            }
            var lst = new StaticPagedList<InfoProdStkBox>(new List<InfoProdStkBox>(), 1, 1, 0);
            return View("_infoProdStkBoxGrid", new GridCore<InfoProdStkBox>() { ListaDatos = lst, CantidadReg = 0, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" });

        }

        private GridCore<InfoProdStkBox> ObtenerInfoProdStkBox((string, string) ids, bool esSession = false)
        {
            List<InfoProdStkBox> listInfoProd;
            if (esSession)
            {
                listInfoProd = InfoProdStkBoxesRegs;
            }
            else
            {
                listInfoProd = _productoServicio.InfoProductoStkBoxes(ids.Item1, AdministracionId, ids.Item2, TokenCookie).GetAwaiter().GetResult();
            }

            var lista = new StaticPagedList<InfoProdStkBox>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCore<InfoProdStkBox>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }

        [HttpPost]
        public ActionResult InfoProductoStkA()
        {
            GridCore<InfoProdStkA> datosIP;
            try
            {
                string id = ProductoBase.P_id;

                if (!id.Equals(InfoProdStkAId))
                {
                    InfoProdStkAId = id;

                    var regs = _productoServicio.InfoProductoStkA(id, "%", TokenCookie).GetAwaiter().GetResult();

                    datosIP = ObtenerInfoProdStkA(regs);
                }
                else
                {
                    datosIP = ObtenerInfoProdStkA(InfoProdStkARegs);
                }
                return PartialView("_infoProdStkAGrid", datosIP);
            }
            catch (Exception ex)
            {
                TempData["warn"] = ex.Message;
            }
            var lst = new StaticPagedList<InfoProdStkA>(new List<InfoProdStkA>(), 1, 1, 0);
            return View("_infoProdStkAGrid", new GridCore<InfoProdStkA>() { ListaDatos = lst, CantidadReg = 0, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" });
        }

        private GridCore<InfoProdStkA> ObtenerInfoProdStkA(List<InfoProdStkA> listInfoProd)
        {
            var lista = new StaticPagedList<InfoProdStkA>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCore<InfoProdStkA>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }


        [HttpPost]
        public ActionResult InfoProductoMovStk()
        {
            GridCore<InfoProdMovStk> datosIP;
            string depo = "%";
            string tmov = "RP";
            DateTime desde, hasta;
            hasta = DateTime.Today;
            desde = DateTime.Today.AddDays(-7);
            try
            {
                string id = ProductoBase.P_id;
                var ids = InfoProdMovStkIds.Split('#');
                if (!(id.Equals(ids[0]) && depo.Equals(ids[1]) && tmov.Equals(ids[2]) && desde == ids[3].ToDateTime() && hasta == ids[4].ToDateTime()))
                {
                    InfoProdMovStkIds = $"{id}#{depo}#{tmov}#{desde}#{hasta}";

                    var regs = _productoServicio.InfoProductoMovStk(id, AdministracionId, depo, tmov, desde, hasta, TokenCookie).GetAwaiter().GetResult();

                    datosIP = ObtenerInfoProdMovStk(regs);
                }
                else
                {
                    datosIP = ObtenerInfoProdMovStk(InfoProdMovStkRegs);
                }
                return PartialView("_infoProdMovStkGrid", datosIP);
            }
            catch (Exception ex)
            {
                TempData["warn"] = ex.Message;
            }
            var lst = new StaticPagedList<InfoProdStkBox>(new List<InfoProdStkBox>(), 1, 1, 0);
            return View("_infoProdStkBoxGrid", new GridCore<InfoProdStkBox>() { ListaDatos = lst, CantidadReg = 0, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" });

        }

        private GridCore<InfoProdMovStk> ObtenerInfoProdMovStk(List<InfoProdMovStk> listInfoProd)
        {

            var lista = new StaticPagedList<InfoProdMovStk>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCore<InfoProdMovStk>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }

        [HttpPost]
        public ActionResult InfoProductoLP()
        {
            GridCore<InfoProdLP> datosIP;
            try
            {
                string id = ProductoBase.P_id;
                if (!id.Equals(InfoProdLPId))
                {
                    InfoProdLPId = id;

                    var regs = _productoServicio.InfoProductoLP(id, TokenCookie).GetAwaiter().GetResult();


                    datosIP = ObtenerInfoProdLP(regs);
                }
                else
                {
                    datosIP = ObtenerInfoProdLP(InfoProdLPRegs);
                }
                return PartialView("_infoProdLPGrid", datosIP);
            }
            catch (Exception ex)
            {
                TempData["warn"] = ex.Message;
            }
            var lst = new StaticPagedList<InfoProdLP>(new List<InfoProdLP>(), 1, 1, 0);
            return View("_infoProdLPGrid", new GridCore<InfoProdLP>() { ListaDatos = lst, CantidadReg = 0, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" });
        }

        private GridCore<InfoProdLP> ObtenerInfoProdLP(List<InfoProdLP> listInfoProd)
        {

            var lista = new StaticPagedList<InfoProdLP>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCore<InfoProdLP>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }

    }
}
