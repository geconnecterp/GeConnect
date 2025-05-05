using Azure;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Info;
using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.Deposito;
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
        private readonly IProducto2Servicio _producto2Servicio;
        private readonly IDepositoServicio _depositoServicio;
        private readonly ICuentaServicio _ctaSv;
        private readonly IRubroServicio _rubSv;
        private readonly AppSettings _settings;

        public InfoProdController(IOptions<AppSettings> options, IOptions<MenuSettings> options1,
            IHttpContextAccessor context, IProductoServicio productoServicio, ILogger<InfoProdController> logger,
            ICuentaServicio cta, IRubroServicio rubro, IDepositoServicio depositoServicio, IProducto2Servicio producto2Servicio) : base(options, options1, context,logger)
        {
            _menuSettings = options1.Value;
            _productoServicio = productoServicio;
            _logger = logger;
            _ctaSv = cta;
            _rubSv = rubro;
            _settings = options.Value;
            _depositoServicio = depositoServicio;
            _producto2Servicio = producto2Servicio;
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
            RubroLista = _rubSv.ObtenerListaRubros("",TokenCookie);
        }

        private void ObtenerProveedores()
        {
            //se guardan los proveedores en session. Para ser utilizados posteriormente

            ProveedoresLista = _ctaSv.ObtenerListaProveedores("BI", TokenCookie);
        }


        [HttpGet]
        public async Task<IActionResult> Producto(string callback = "")
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || (auth.Item1 && !auth.Item2.HasValue) || (auth.Item1 && auth.Item2.HasValue && auth.Item2.Value < DateTime.Now))
            {
                return RedirectToAction("Login", "Token", new { area = "Seguridad" });
            }

            if (ProveedoresLista.Count == 0 )
            {
                ObtenerProveedores();
            }

            if (RubroLista.Count == 0 )
            {
                ObtenerRubros();
            }

            await ObtenerTiposMotivo();

            if (!string.IsNullOrEmpty(callback))
            {
                ViewBag.Callback = callback;
            }

            string volver = Url.Action("info", "almacen", new { area = "gestion" })??"#";
            ViewBag.AppItem = new AppItem { Nombre = "Información de Productos", VolverUrl = volver ?? "#" };

            TempData["cota"] = _settings.FechaVtoCota;

            return View();
        }

        private async Task ObtenerTiposMotivo()
        {
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
        }

        [HttpGet]
        public IActionResult Box()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || (auth.Item1 && !auth.Item2.HasValue) || (auth.Item1 && auth.Item2.HasValue && auth.Item2.Value < DateTime.Now))
            {
                return RedirectToAction("Login", "Token", new { area = "Seguridad" });
            }

            string volver = Url.Action("info", "almacen", new { area = "gestion" })??"#";
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

            string volver = Url.Action("info", "almacen", new { area = "gestion" })??"#";
            ViewBag.AppItem = new AppItem { Nombre = "Información de Productos", VolverUrl = volver ?? "#" };

            return View();
        }

        [HttpPost]
        public ActionResult InfoProductoStkD()
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<InfoProdStkD> grillaDatos;

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

        private GridCoreSmart<InfoProdStkD> ObtenerInfoProdStkD(List<InfoProdStkD> listInfoProd)
        {

            var lista = new StaticPagedList<InfoProdStkD>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCoreSmart<InfoProdStkD>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }


        [HttpPost]
        public ActionResult InfoProductoStkBoxes()
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<InfoProdStkBox> grillaDatos;
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

        private GridCoreSmart<InfoProdStkBox> ObtenerInfoProdStkBox((string, string) ids, bool esSession = false)
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

            return new GridCoreSmart<InfoProdStkBox>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }

        [HttpPost]
        public ActionResult InfoProductoStkA()
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<InfoProdStkA> grillaDatos;
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

        private GridCoreSmart<InfoProdStkA> ObtenerInfoProdStkA(List<InfoProdStkA> listInfoProd)
        {
            var lista = new StaticPagedList<InfoProdStkA>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCoreSmart<InfoProdStkA>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }


        [HttpPost]
        public ActionResult InfoProductoMovStk(string idTm, DateTime fdesde, DateTime fhasta)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<InfoProdMovStk> grillaDatos;
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

        private GridCoreSmart<InfoProdMovStk> ObtenerInfoProdMovStk(List<InfoProdMovStk> listInfoProd)
        {

            var lista = new StaticPagedList<InfoProdMovStk>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCoreSmart<InfoProdMovStk>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }

        [HttpPost]
        public ActionResult InfoProductoLP()
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<InfoProdLP> grillaDatos;
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

        private GridCoreSmart<InfoProdLP> ObtenerInfoProdLP(List<InfoProdLP> listInfoProd)
        {

            var lista = new StaticPagedList<InfoProdLP>(listInfoProd, 1, 999, listInfoProd.Count);

            return new GridCoreSmart<InfoProdLP>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
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

            string volver = Url.Action("info", "almacen", new { area = "gestion" })??"#";
            ViewBag.AppItem = new AppItem { Nombre = "Información de Productos - Gestión de Depósitos", VolverUrl = volver ?? "#" };

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
            GridCoreSmart<DepositoInfoBoxDto> grillaDatos;

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

        private GridCoreSmart<DepositoInfoBoxDto> PreparaGridBoxLibres(List<DepositoInfoBoxDto> regs)
        {
            var lista = new StaticPagedList<DepositoInfoBoxDto>(regs, 1, 999, regs.Count);

            return new GridCoreSmart<DepositoInfoBoxDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }

        [HttpPost]
        public async Task<IActionResult> BuscarDepoStk(string depo_id)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<DepositoInfoStkDto> grillaDatos;

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

        private GridCoreSmart<DepositoInfoStkDto> PreparaGridInfoStk(List<DepositoInfoStkDto> regs)
        {
            var lista = new StaticPagedList<DepositoInfoStkDto>(regs, 1, 999, regs.Count);

            return new GridCoreSmart<DepositoInfoStkDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }

        [HttpPost]
        public async Task<IActionResult> BuscarDepoStkVAl(string depo_id, string concepto)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<DepositoInfoStkValDto> grillaDatos;

            try
            {
                List<DepositoInfoStkValDto> regs = await _depositoServicio.BuscarDepositoInfoStkVal(AdministracionId, depo_id, concepto, TokenCookie);

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

        private GridCoreSmart<DepositoInfoStkValDto> PreparaGridInfoStkVal(List<DepositoInfoStkValDto> regs)
        {
            var lista = new StaticPagedList<DepositoInfoStkValDto>(regs, 1, 999, regs.Count);

            return new GridCoreSmart<DepositoInfoStkValDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };
        }
        #endregion

        #region Vistas INFO BOX
        [HttpGet]
        public async Task<IActionResult> InfoBox()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || (auth.Item1 && !auth.Item2.HasValue) || (auth.Item1 && auth.Item2.HasValue && auth.Item2.Value < DateTime.Now))
            {
                return RedirectToAction("Login", "Token", new { area = "Seguridad" });
            }
            ComboDepositos();
            await ObtenerTiposMotivo();

            string volver = Url.Action("info", "almacen", new { area = "gestion" })??"#";
            ViewBag.AppItem = new AppItem { Nombre = "Información de Productos - Gestión de Box", VolverUrl = volver ?? "#" };

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ValidarBox(string boxId)
        {
            try
            {
                if (string.IsNullOrEmpty(boxId) || string.IsNullOrWhiteSpace(boxId))
                {
                    throw new NegocioException("No se recepcionó el Box. Verifique");
                }
                var res = await _productoServicio.ValidarBox(boxId, AdministracionId, TokenCookie);
                if (res.Resultado == 0)
                {
                    //se llama al sp con información del box
                    RespuestaGenerica<BoxInfoDto> info = await _producto2Servicio.ObtenerBoxInfo(boxId, TokenCookie);
                    if (info.Ok)
                    {
                        return Json(new { error = false, warn = false, info = info.Entidad, msg = "EL BOX SE VALIDO, SATISFACTORIAMENTE." });
                    }
                    else
                    {
                        throw new NegocioException(info.Mensaje??"Hubo un problema para Validar el Box");
                    }
                }
                else
                {
                    throw new NegocioException(res.Resultado_msj);
                }
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerBoxInfoStk(string boxId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<BoxInfoStkDto> grillaDatos;
            try
            {
                if (string.IsNullOrEmpty(boxId) || string.IsNullOrWhiteSpace(boxId))
                {
                    throw new NegocioException("No se recepcionó el Box. Verifique");
                }
                var res = await _producto2Servicio.ObtenerBoxInfoStk(boxId, TokenCookie);
                if (res.Ok)
                {
                    grillaDatos = GenerarGrilla(res.ListaEntidad, "P_id");
                }
                else
                {
                    throw new NegocioException(res.Mensaje ?? "Hubo un problema al obtener info de Stk en el Box");
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
            return PartialView("_gridBoxInfoStk", grillaDatos);
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerBoxInfoMovStk(string boxId, string sm, DateTime desde, DateTime hasta)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCoreSmart<BoxInfoMovStkDto> grillaDatos;
            try
            {
                if (string.IsNullOrEmpty(boxId) || string.IsNullOrWhiteSpace(boxId))
                {
                    throw new NegocioException("No se recepcionó el Box. Verifique");
                }

                //sm = sm ?? "%";

                var res = await _producto2Servicio.ObtenerBoxInfoMovStk(boxId, sm, desde, hasta, TokenCookie);
                if (res.Ok)
                {
                    grillaDatos = GenerarGrilla(res.ListaEntidad, "P_id");
                }
                else
                {
                    throw new NegocioException(res.Mensaje ?? "Hubo un problema para obtener la información de los Movimientos de Productos.");
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
            return PartialView("_gridBoxInfoMovStk", grillaDatos);
        }

        #endregion

        #region INFO UL
        [HttpGet]
        public IActionResult InfoUL()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || (auth.Item1 && !auth.Item2.HasValue) || (auth.Item1 && auth.Item2.HasValue && auth.Item2.Value < DateTime.Now))
            {
                return RedirectToAction("Login", "Token", new { area = "Seguridad" });
            }

            string volver = Url.Action("info", "almacen", new { area = "gestion" })??"#";
            ViewBag.AppItem = new AppItem { Nombre = "Información de Productos - CONSULTA DE UL", VolverUrl = volver ?? "#" };

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConsultarUL(string tipo,DateTime desde,DateTime hasta)
        {
            GridCoreSmart<ConsULDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (desde > hasta)
                {
                    throw new NegocioException("Verifique el Periodo de fechas, ya que es incorrecta.");
                }
                var res = await _producto2Servicio.ConsultaUL(tipo, desde, hasta, AdministracionId, TokenCookie);
                if (res.Ok)
                {
                    if (res.ListaEntidad?.Count > 0)
                    {
                        foreach (var item in res.ListaEntidad)
                        {
                            //generando imagen png en base 64 con el code 3of9
                            item.ImgB64 = HelperGen.GeneraIdEnCodeBar3of9WithText(item.UL_id);
                        }

                        grillaDatos = GenerarGrilla<ConsULDto>(res.ListaEntidad, "UL_id");
                        return PartialView("_gridListadoUL", grillaDatos);
                    }
                    else
                    {
                        if (tipo == "F")
                        {
                            response.Mensaje = $"No se encontraron datos para el periodo {desde.ToShortDateString()} - {hasta.ToShortDateString()}";
                        }
                        else
                        {
                            response.Mensaje = $"No se encontraron UL sin almacenar.";
                        }
                        response.Ok = true;
                        response.EsWarn = false;
                        response.EsError = false;
                        return PartialView("_gridMensaje", response);
                    }
                }
                else
                {
                    throw new NegocioException(res.Mensaje ?? "Hubo un problema para consultar la UL");
                }
            }
            catch(NegocioException ex)
            {
                _logger.LogError(ex, "Error al consultar las ULs");
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
                string msg = "Error al Intentar consultar las ULs. Verifique.";
                _logger.LogError(ex, "Error al consultar las ULs");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
        #endregion
    }
}
