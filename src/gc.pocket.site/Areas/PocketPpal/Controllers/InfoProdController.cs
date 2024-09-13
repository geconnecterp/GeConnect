using Azure;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Seguridad;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private readonly AppSettings _settings;

        public InfoProdController(IOptions<AppSettings> options, IOptions<MenuSettings> options1,
            IHttpContextAccessor context, IProductoServicio productoServicio, ILogger<InfoProdController> logger,
            ICuentaServicio cta, IRubroServicio rubro) : base(options, options1, context)
        {
            _menuSettings = options1.Value;
            _productoServicio = productoServicio;
            _logger = logger;
            _ctaSv = cta;
            _rubSv = rubro;
            _settings= options.Value;
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
                    var auth = EstaAutenticado;
                    if (!auth.Item1 || (auth.Item1 && !auth.Item2.HasValue) || (auth.Item1 && auth.Item2.HasValue && auth.Item2.Value < DateTime.Now))
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
            var auth = EstaAutenticado;
            if (!auth.Item1 || (auth.Item1 && !auth.Item2.HasValue) || (auth.Item1 && auth.Item2.HasValue && auth.Item2.Value < DateTime.Now))
            {
                return RedirectToAction("Login", "Token", new { area = "Seguridad" });
            }

            TempData["cota"] = _settings.FechaVtoCota;

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
            RespuestaGenerica<EntidadBase> response = new() ;
            GridCore<InfoProdStkD> grillaDatos;

            try
            {
                string id = ProductoBase.P_id;
                if (!id.Equals(InfoProdStkDId))
                {
                    InfoProdStkDId = id;

                    var regs = _productoServicio.InfoProductoStkD(id, AdministracionId, TokenCookie).GetAwaiter().GetResult();
                    InfoProdStkDRegs = regs;
                    grillaDatos = ObtenerInfoProdStkD(regs);

                }
                else
                {
                    grillaDatos = ObtenerInfoProdStkD(InfoProdStkDRegs);
                }
               
            }
            catch(NegocioException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (UnauthorizedException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
            return PartialView("_infoProdStkDGrid", grillaDatos);
        }

        private GridCore<InfoProdStkD> ObtenerInfoProdStkD(List<InfoProdStkD> listInfoProd)
        {

            var lista = new StaticPagedList<InfoProdStkD>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCore<InfoProdStkD>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }


        [HttpPost]
        public ActionResult InfoProductoStkBoxes()
        {
            RespuestaGenerica<EntidadBase> response=new();
            GridCore<InfoProdStkBox> grillaDatos;
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
                        grillaDatos = ObtenerInfoProdStkBox((id, depo));
                    }
                }
                else
                {
                    grillaDatos = ObtenerInfoProdStkBox(InfoProdStkBoxesIds, true);
                }
              
            }
            catch (NegocioException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (UnauthorizedException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
            return PartialView("_infoProdStkBoxGrid", response);
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
            RespuestaGenerica<EntidadBase> response=new();
            GridCore<InfoProdStkA> grillaDatos;
            try
            {
                string id = ProductoBase.P_id;

                if (!id.Equals(InfoProdStkAId))
                {
                    InfoProdStkAId = id;

                    var regs = _productoServicio.InfoProductoStkA(id, "%", TokenCookie).GetAwaiter().GetResult();

                    grillaDatos = ObtenerInfoProdStkA(regs);
                }
                else
                {
                    grillaDatos = ObtenerInfoProdStkA(InfoProdStkARegs);
                }
                
            }
            catch (NegocioException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;                
                return PartialView("_gridMensaje", response);
            }
            catch (UnauthorizedException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
            return PartialView("_infoProdStkAGrid", response);
        }

        private GridCore<InfoProdStkA> ObtenerInfoProdStkA(List<InfoProdStkA> listInfoProd)
        {
            var lista = new StaticPagedList<InfoProdStkA>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCore<InfoProdStkA>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }


        [HttpPost]
        public ActionResult InfoProductoMovStk()
        {
            RespuestaGenerica<EntidadBase> response = new()  ;
            GridCore<InfoProdMovStk> grillaDatos;
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

                    grillaDatos = ObtenerInfoProdMovStk(regs);
                }
                else
                {
                    grillaDatos = ObtenerInfoProdMovStk(InfoProdMovStkRegs);
                }
                
            }
            catch (NegocioException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (UnauthorizedException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
            return PartialView("_infoProdMovStkGrid", response);
        }

        private GridCore<InfoProdMovStk> ObtenerInfoProdMovStk(List<InfoProdMovStk> listInfoProd)
        {

            var lista = new StaticPagedList<InfoProdMovStk>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCore<InfoProdMovStk>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }

        [HttpPost]
        public ActionResult InfoProductoLP()
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<InfoProdLP> grillaDatos;
            try
            {
                string id = ProductoBase.P_id;
                if (!id.Equals(InfoProdLPId))
                {
                    InfoProdLPId = id;

                    var regs = _productoServicio.InfoProductoLP(id, TokenCookie).GetAwaiter().GetResult();


                    grillaDatos = ObtenerInfoProdLP(regs);
                }
                else
                {
                    grillaDatos = ObtenerInfoProdLP(InfoProdLPRegs);
                }
                
            }
            catch (NegocioException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (UnauthorizedException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }

            return PartialView("_infoProdLPGrid", response );
        }

        private GridCore<InfoProdLP> ObtenerInfoProdLP(List<InfoProdLP> listInfoProd)
        {

            var lista = new StaticPagedList<InfoProdLP>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCore<InfoProdLP>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }

    }
}
