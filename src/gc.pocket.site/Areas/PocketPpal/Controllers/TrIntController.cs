using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Reflection;
using X.PagedList;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class TrIntController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<TrIntController> _logger;
        private readonly IProductoServicio _productoServicio;
        private readonly IAdministracionServicio _administracionServicio;
        private readonly AppSettings _settings;

        public TrIntController(IOptions<AppSettings> options, IHttpContextAccessor context, IOptions<MenuSettings> options1,
            ILogger<TrIntController> logger, IProductoServicio productoServicio, IAdministracionServicio admiServicio) : base(options, context)
        {
            _menuSettings = options1.Value;
            _logger = logger;
            _productoServicio = productoServicio;
            _administracionServicio = admiServicio;
            _settings = options.Value;
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
            var sigla = "ti";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            ViewBag.AppItem = modulo;
            return View();
        }

        #region VISTA 01

        /// <summary>
        /// Primer vista para presentar las autorizaciones pendientes para TI
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> TiEntreSuc()
        {
            // GENERAR VARIABLE QUE ME INDIQUE QUE TIPO DE TR ESTOY HACIENDO PARA AUTOMATIZAR MENSAJES DE CABECERA
            TIModuloActual = "S";


            string? volver = Url.Action("index", "trint", new { area = "pocketppal" });
            ViewBag.AppItem = new AppItem { Nombre = "TR Interna entre Sucursales", VolverUrl = volver ?? "#" };

            ViewBag.ModuloActual = TIModuloActual;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerAutorizacionesPendientes()
        {
            GridCore<AutorizacionTIDto> grid;
            try
            {
                var autos = await _productoServicio.TRObtenerAutorizacionesPendientes(AdministracionId, UserName, "S", TokenCookie);

                ListadoTIAutoPendientes = autos;

                grid = ObtenerAutorizaciones(autos);
            }
            catch (NegocioException ex)
            {
                TempData["warn"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex.Message);
                TempData["warn"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");
            }

            return PartialView("_gridTRAutoPendientes", grid);

        }

        /// <summary>
        /// Se valida el usuario para verificar que solo esta "trabajado" en un solo lugar y no tiene otra autorización pendiente por otro lado.
        /// Se resguarda la autorización seleccionada.
        /// </summary>
        /// <param name="auId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ValidarUsuario(string auId)
        {
            string user = string.Empty;
            try
            {
                user = UserName;
                ResponseBaseDto resp = await _administracionServicio.ValidarUsuario(userId: user, "TR", auId, token: TokenCookie);

                if (resp == null)
                {
                    throw new NegocioException("Algo no fue bíen para Validar el usuario");
                }
                if (resp.Resultado == 0)
                {
                    var ti = ListadoTIAutoPendientes.SingleOrDefault(x => x.Ti.Equals(auId));
                    if (ti == null)
                    {
                        //no se encontro la TI
                        throw new NegocioException("No se encontró la TI a procesar.");
                    }
                    ti.TipoTI=TIModuloActual;
                    TIActual = ti;

                    return Json(new { error = false, warn = false, msg = "Validación Exitosa" });
                }
                else
                {
                    //resguardamos la autorizacion seleccionada
                    _logger.LogWarning($"{resp.Resultado_msj} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} Usuario:{user}");
                    return Json(new { error = false, warn = true, msg = resp.Resultado_msj });
                }
            }
            catch (NegocioException ex)
            {

                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} Usuario:{user}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} Usuario:{user}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} Usuario:{user}");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        #endregion  //VISTA 01

        #region VISTA 02

        /// <summary>
        /// SELECCIONADA LA AUTORIZACION PENDIENTE SE PRESENTARAN EN PANTALLA LOS PRODUCTOS A INCLUIR BUSCANDOLOS POR BOX O RUBRO
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> TiEntreSucBoxRubro()
        {
            string? volver;
            var ti = TIActual;
            if (ti.TipoTI.Equals("S"))
            {
                volver = Url.Action("TiEntreSuc", "trint", new { area = "pocketppal" });
            }
            else
            {
                volver = Url.Action("index", "trint", new { area = "pocketppal" });
            }
            ViewBag.AppItem = new AppItem { Nombre = "TI e/ Sucs - Vista de Box o Rubros ", VolverUrl = volver ?? "#" };

            return View(TIActual);
        }


        [HttpPost]
        public async Task<IActionResult> PresentarBoxDeProductos()
        {
            GridCore<BoxRubProductoDto> grid;
            try
            {
                var regs = await _productoServicio.PresentarBoxDeProductos(tr: TIActual.Ti, admId: AdministracionId, usuId: UserName, token: TokenCookie);
                grid = ObtenerGrillaDeBoxRubros(regs);
            }
            catch (NegocioException ex)
            {
                TempData["warn"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex.Message);
                TempData["warn"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");

            }
            return PartialView("_gridTIBox", grid);
        }

        private GridCore<BoxRubProductoDto> ObtenerGrillaDeBoxRubros(List<BoxRubProductoDto> regs)
        {
            var lista = new StaticPagedList<BoxRubProductoDto>(regs, 1, 999, regs.Count);

            return new GridCore<BoxRubProductoDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Depo_nombre", SortDir = "ASC" };

        }

        [HttpPost]
        public async Task<IActionResult> PresentarRubroDeProductos()
        {
            GridCore<BoxRubProductoDto> response;
            try
            {
                var regs = await _productoServicio.PresentarRubrosDeProductos(tr: TIActual.Ti, admId: AdministracionId, usuId: UserName, token: TokenCookie);
                response = ObtenerGrillaDeBoxRubros(regs);
            }
            catch (NegocioException ex)
            {
                TempData["warn"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex.Message);
                TempData["warn"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");

            }
            return PartialView("_gridTIRubro", response);
        }


        private GridCore<AutorizacionTIDto> ObtenerAutorizaciones(List<AutorizacionTIDto> autos)
        {

            var lista = new StaticPagedList<AutorizacionTIDto>(autos, 1, 999, autos.Count);

            return new GridCore<AutorizacionTIDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Ti", SortDir = "ASC" };
        }

        #endregion


        #region VISTA 03

        /// <summary>
        /// VISTA INICIAL PARA CARGAR LOS PRODUCTOS. LA PRIMERA VEZ SE TRAE LOS DATOS DESDE EL SERVER. 
        /// 
        /// SE DEBE VERIFICAR SI PARA EL BUCLE QUE HAY QUE HACER POR CADA PRODUCGT
        /// </summary>
        /// <param name="esrubro"></param>
        /// <param name="esbox"></param>
        /// <param name="boxid"></param>
        /// <param name="rubroid"></param>
        /// <param name="rubrogid"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult TIentreSucCargaCarrito(bool esrubro, bool esbox, string? boxid, string? rubroid, string? rubrogid)
        {
            string? volver;
            try
            {
                //resguardamos la seleccion realizada
                var ti = TIActual;
                ti.EsRubro = esrubro;
                ti.RubroGId = rubrogid;
                ti.RubroId = rubroid ?? "%";
                ti.EsBox = esbox;
                ti.BoxId = boxid ?? "%";

                TIActual = ti;
                volver = Url.Action("TiEntreSucBoxRubro", "trint", new { area = "pocketppal" });

                ViewBag.AppItem = new AppItem { Nombre = "TI e/ Sucs - Producto a colectar en Carrito", VolverUrl = volver ?? "#" };
                return View(TIActual);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscaTIListaProductos()
        {
            GridCore<TiListaProductoDto> grid;
            try
            {
                var selec = TIActual;
                List<TiListaProductoDto> regs = await _productoServicio.BuscaTIListaProductos(tr: TIActual.Ti, admId: AdministracionId, usuId: UserName, boxid: selec.BoxId, rubId: selec.RubroId, token: TokenCookie);
                ListaProductosSegunRubro = regs;
                grid = ObtenerGrillaTIListaProductos(regs);
            }
            catch (NegocioException ex)
            {
                TempData["warn"] = ex.Message;
                return RedirectToAction("Index");   //para mensajes en pantalla debere generar una vista generica de errores.
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(ex.Message);
                TempData["warn"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");

            }

            return PartialView("_gridTIListaProducto", grid);
        }

        private GridCore<TiListaProductoDto> ObtenerGrillaTIListaProductos(List<TiListaProductoDto> regs)
        {
            var lista = new StaticPagedList<TiListaProductoDto>(regs, 1, 999, regs.Count);

            return new GridCore<TiListaProductoDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Ti", SortDir = "ASC" };

        }

        [HttpGet]
        public async Task<IActionResult> TIValidaProducto(string pId)
        {
            string? volver;
            try
            {
                var prod = await _productoServicio.BusquedaBaseProductos(new BusquedaBase { Busqueda = pId, Administracion = AdministracionId, DescuentoCli = 0, ListaPrecio = "", TipoOperacion = "CR" }, TokenCookie);

                var sel = TIActual;

                volver = Url.Action("TIentreSucCargaCarrito", "trint", new { area = "pocketppal", esrubro = sel.EsRubro, esbox = sel.EsBox, boxid = sel.BoxId, rubroid = sel.RubroId, rubrogid = sel.RubroGId });
                ViewBag.AppItem = new AppItem { Nombre = "TI - Carga Carrito", VolverUrl = volver ?? "#" };

                return View(prod);
            }
            catch (Exception ex)
            {
                var sel = TIActual;
                TempData["error"]= ex.ToString();
                return RedirectToAction("TIentreSucCargaCarrito", "trint", new { area = "pocketppal", esrubro = sel.EsRubro, esbox = sel.EsBox, boxid = sel.BoxId, rubroid = sel.RubroId, rubrogid = sel.RubroGId });
            }
        }

        #endregion


     

      

        public IActionResult TIentreDep()
        {
            string? volver = Url.Action("index", "trint", new { area = "pocketppal" });
            ViewBag.AppItem = new AppItem { Nombre = "TR Interna entre Depositos", VolverUrl = volver ?? "#" };
            return View();
        }

        public IActionResult TIentreDepSinAuto()
        {
            string? volver = Url.Action("index", "trint", new { area = "pocketppal" });
            ViewBag.AppItem = new AppItem { Nombre = "TR Interna entre Depositos (SIN AUTORIZACION)", VolverUrl = volver ?? "#" };
            return View();
        }

        public IActionResult TIentreBox()
        {
            string? volver = Url.Action("index", "trint", new { area = "pocketppal" });
            ViewBag.AppItem = new AppItem { Nombre = "TR Interna entre Box", VolverUrl = volver ?? "#" };
            return View();
        }

        public IActionResult TIentreBoxSinAuto()
        {
            string? volver = Url.Action("index", "trint", new { area = "pocketppal" });
            ViewBag.AppItem = new AppItem { Nombre = "TR Interna entre Box (SIN AUTORIZACION)", VolverUrl = volver ?? "#" };
            return View();
        }
    }
}
