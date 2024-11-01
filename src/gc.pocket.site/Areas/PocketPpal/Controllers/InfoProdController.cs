using Azure;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen.Info;
using gc.infraestructura.Dtos.Deposito;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Seguridad;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.ObjectModelRemoting;
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
        private readonly IDepositoServicio _depositoServicio;
        private readonly ICuentaServicio _ctaSv;
        private readonly IRubroServicio _rubSv;
        private readonly AppSettings _settings;

        public InfoProdController(IOptions<AppSettings> options, IOptions<MenuSettings> options1,
            IHttpContextAccessor context, IProductoServicio productoServicio, ILogger<InfoProdController> logger,
            ICuentaServicio cta, IRubroServicio rubro, IDepositoServicio depositoServicio) : base(options, options1, context)
        {
            _menuSettings = options1.Value;
            _productoServicio = productoServicio;
            _logger = logger;
            _ctaSv = cta;
            _rubSv = rubro;
            _settings = options.Value;
            _depositoServicio = depositoServicio;
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
        public async Task<IActionResult> Producto(string callback = "")
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || (auth.Item1 && !auth.Item2.HasValue) || (auth.Item1 && auth.Item2.HasValue && auth.Item2.Value < DateTime.Now))
            {
                return RedirectToAction("Login", "Token", new { area = "Seguridad" });
            }

            //obtengo datos TipoMotivo
            if (TiposMotivo.Count == 0)
            {
                var res = await _productoServicio.ObtenerTiposMotivo(TokenCookie);
                if (res != null)
                {
                    TiposMotivo = res;
                }
            }
            var lista = TiposMotivo.Select(x => new ComboGenDto { Id = x.Sm_tipo, Descripcion = x.Sm_Desc });

            ViewBag.TmId = HelperMvc<ComboGenDto>.ListaGenerica(lista);

            if (!string.IsNullOrEmpty(callback))
            {
                ViewBag.Callback = callback;
            }

            string volver = Url.Action("info", "almacen", new { area = "gestion" });
            ViewBag.AppItem = new AppItem { Nombre = "Información de Productos", VolverUrl = volver ?? "#" };

            TempData["cota"] = _settings.FechaVtoCota;

            return View();
        }



        [HttpGet]
        public IActionResult Box()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || (auth.Item1 && !auth.Item2.HasValue) || (auth.Item1 && auth.Item2.HasValue && auth.Item2.Value < DateTime.Now))
            {
                return RedirectToAction("Login", "Token", new { area = "Seguridad" });
            }

            string volver = Url.Action("info", "almacen", new { area = "gestion" });
            ViewBag.AppItem = new AppItem { Nombre = "Información de Productos", VolverUrl = volver ?? "#" };

            return View();
        }

        [HttpGet]
        public IActionResult Ul()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || (auth.Item1 && !auth.Item2.HasValue) || (auth.Item1 && auth.Item2.HasValue && auth.Item2.Value < DateTime.Now))
            {
                return RedirectToAction("Login", "Token", new { area = "Seguridad" });
            }

            string volver = Url.Action("info", "almacen", new { area = "gestion" });
            ViewBag.AppItem = new AppItem { Nombre = "Información de Productos", VolverUrl = volver ?? "#" };

            return View();
        }

        [HttpPost]
        public ActionResult InfoProductoStkD()
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<InfoProdStkD> grillaDatos;

            try
            {
                string id = ProductoBase.P_id;
                if (!id.Equals(InfoProdStkDId))
                {


                    var regs = _productoServicio.InfoProductoStkD(id, AdministracionId, TokenCookie).GetAwaiter().GetResult();
                    InfoProdStkDRegs = regs;
                    InfoProdStkDId = id;
                    grillaDatos = ObtenerInfoProdStkD(regs);

                }
                else
                {
                    grillaDatos = ObtenerInfoProdStkD(InfoProdStkDRegs);
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
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<InfoProdStkBox> grillaDatos;
            //en documentacion GECO POCKET INFOPROD el valor default de depo = %
            string depo = "%";
            try
            {
                string id = ProductoBase.P_id;
                var ids = InfoProdStkBoxesIds;
                if (!(id.Equals(ids.Item1)))
                {
                    var regs = _productoServicio.InfoProductoStkBoxes(id, AdministracionId, depo, TokenCookie).GetAwaiter().GetResult();

                    if (regs == null || regs.Count == 0)
                    {
                        throw new Exception("No se encontró el producto");
                    }
                    else
                    {
                        InfoProdStkBoxesIds = (id, depo);
                        InfoProdStkBoxesRegs = regs;
                        var data = regs.First();
                        grillaDatos = ObtenerInfoProdStkBox((id, depo));
                    }
                }
                else
                {
                    grillaDatos = ObtenerInfoProdStkBox((id, depo));
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
            return PartialView("_infoProdStkBoxGrid", grillaDatos);
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
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<InfoProdStkA> grillaDatos;
            try
            {
                string id = ProductoBase.P_id;

                if (!id.Equals(InfoProdStkAId))
                {

                    var regs = _productoServicio.InfoProductoStkA(id, "%", TokenCookie).GetAwaiter().GetResult();
                    InfoProdStkAId = id;
                    InfoProdStkARegs = regs;
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
            return PartialView("_infoProdStkAGrid", grillaDatos);
        }

        private GridCore<InfoProdStkA> ObtenerInfoProdStkA(List<InfoProdStkA> listInfoProd)
        {
            var lista = new StaticPagedList<InfoProdStkA>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCore<InfoProdStkA>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }


        [HttpPost]
        public ActionResult InfoProductoMovStk(string idTm, DateTime fdesde, DateTime fhasta)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<InfoProdMovStk> grillaDatos;
            idTm = idTm ?? "%";
            string depo = "%";

            try
            {
                string id = ProductoBase.P_id;
                //var ids = InfoProdMovStkIds.Split('#');
                //if (!(id.Equals(ids[0]) && depo.Equals(ids[1]) && idTm.Equals(ids[2]) && fdesde == ids[3].ToDateTime() && fhasta == ids[4].ToDateTime()))
                //{

                var regs = _productoServicio.InfoProductoMovStk(id, AdministracionId, depo, idTm, fdesde, fhasta, TokenCookie).GetAwaiter().GetResult();
                InfoProdMovStkIds = $"{id}#{depo}#{idTm}#{fdesde}#{fhasta}";
                InfoProdMovStkRegs = regs;
                grillaDatos = ObtenerInfoProdMovStk(regs);
                //}
                //else
                //{
                //    grillaDatos = ObtenerInfoProdMovStk(InfoProdMovStkRegs);
                //}

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
            return PartialView("_infoProdMovStkGrid", grillaDatos);
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

                    var regs = _productoServicio.InfoProductoLP(id, TokenCookie).GetAwaiter().GetResult();
                    InfoProdLPId = id;
                    InfoProdLPRegs = regs;

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

            return PartialView("_infoProdLPGrid", grillaDatos);
        }

        private GridCore<InfoProdLP> ObtenerInfoProdLP(List<InfoProdLP> listInfoProd)
        {

            var lista = new StaticPagedList<InfoProdLP>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCore<InfoProdLP>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }


        #region Vista DEPO
        [HttpGet]
        public IActionResult Depo()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || (auth.Item1 && !auth.Item2.HasValue) || (auth.Item1 && auth.Item2.HasValue && auth.Item2.Value < DateTime.Now))
            {
                return RedirectToAction("Login", "Token", new { area = "Seguridad" });
            }
            ComboDepositos();

            string volver = Url.Action("info", "almacen", new { area = "gestion" });
            ViewBag.AppItem = new AppItem { Nombre = "Información de Productos", VolverUrl = volver ?? "#" };

            return View();
        }

        private void ComboDepositos()
        {
            var adms = _depositoServicio.ObtenerDepositosDeAdministracion(AdministracionId, TokenCookie);
            var lista = adms.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
            ViewBag.DepoId = HelperMvc<ComboGenDto>.ListaGenerica(lista);
        }

        [HttpPost]
        public async Task<IActionResult> BuscarBoxLibres(string depo_id, bool soloLibres)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<DepositoInfoBoxDto> grillaDatos;

            try
            {
                List<DepositoInfoBoxDto> regs = await _depositoServicio.BuscarBoxLibres(depo_id, soloLibres, TokenCookie);

                grillaDatos = PreparaGridBoxLibres(regs);
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

            return PartialView("_gridBoxLibres", grillaDatos);
        }

        private GridCore<DepositoInfoBoxDto> PreparaGridBoxLibres(List<DepositoInfoBoxDto> regs)
        {
            var lista = new StaticPagedList<DepositoInfoBoxDto>(regs, 1, 999, regs.Count);

            return new GridCore<DepositoInfoBoxDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }

        [HttpPost]
        public async Task<IActionResult> BuscarDepoStk(string depo_id)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<DepositoInfoStkDto> grillaDatos;

            try
            {
                List<DepositoInfoStkDto> regs = await _depositoServicio.BuscarDepositoInfoStk(depo_id, TokenCookie);

                grillaDatos = PreparaGridInfoStk(regs);
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

            return PartialView("_gridDepoInfoStk", grillaDatos);
        }

        private GridCore<DepositoInfoStkDto> PreparaGridInfoStk(List<DepositoInfoStkDto> regs)
        {
            var lista = new StaticPagedList<DepositoInfoStkDto>(regs, 1, 999, regs.Count);

            return new GridCore<DepositoInfoStkDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }

        [HttpPost]
        public async Task<IActionResult> BuscarDepoStkVAl(string depo_id, string concepto)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<DepositoInfoStkValDto> grillaDatos;

            try
            {
                List<DepositoInfoStkValDto> regs = await _depositoServicio.BuscarDepositoInfoStkVal(AdministracionId,depo_id,concepto, TokenCookie);

                grillaDatos = PreparaGridInfoStkVal(regs);
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

            return PartialView("_gridDepoInfoStkVal", grillaDatos);
        }

        private GridCore<DepositoInfoStkValDto> PreparaGridInfoStkVal(List<DepositoInfoStkValDto> regs)
        {
            var lista = new StaticPagedList<DepositoInfoStkValDto>(regs, 1, 999, regs.Count);

            return new GridCore<DepositoInfoStkValDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }
        #endregion
    }
}
