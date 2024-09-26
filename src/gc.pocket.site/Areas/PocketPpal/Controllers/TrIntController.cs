using gc.infraestructura.Constantes;
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
        #region VISTAS INICIALES

        public async Task<IActionResult> Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            ListadoTIAutoPendientes = new();
            TIActual = new();
            TI_ModId = string.Empty;
            TI_CS = false;
            //se verificará si el usuario tiene alguna TR PENDIENTE
            var resu = await _productoServicio.TIValidaPendiente(UserName, TokenCookie);

            if (!resu.Ok && resu.Entidad.resultado == "-1")
            {
                //tiene un tr pendiente por lo que se redirecciona a la carga de productos (carrito) previamente debo obtener los datos de la tr pendiente para cargar 
                //la variable de sesion TIActual 
                var tipo = resu.Entidad.Tit_id;
                var tiId = resu.Entidad.Ti;

                await ObtenerTIPendiente(tipo, tiId);

                //REDIRECCIONAR AL CARRITO                
                return RedirectToAction("TICargaCarrito", "trint", new { area = "pocketppal", esrubro = false, esbox = false, boxid = "", rubroid = "", rubrogid = "", tiId = tipo});

            }

            //este viewbag es para que aparezca en la segunda fila del encabezado la leyenda que se quiera.
            //en este caso presenta el numero de autorización pendiente y el proveedor al que le pertenece.
            var sigla = "ti";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            ViewBag.AppItem = modulo;
            
            return View();
        }

        /// <summary>
        /// Este metodo se encarga de ya cargar en las variables de session los valores de la TIActual
        /// y otras variables de session utilizadas
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="tiId"></param>
        /// <returns></returns>
        /// <exception cref="NegocioException"></exception>
        private async Task ObtenerTIPendiente(string tipo, string tiId)
        {
            TI_ModId = tipo;
            TI_CS = false;
            var ti = TIActual;
            ti.Ti = tiId;
            ti.TipoTI = tipo;
            ti.SinAU = true;

            TIActual = ti;
        }


        #endregion

        #region VISTA 01

        #region VISTA CONFIGURACION INICIAL

        /// <summary>
        /// Primer vista para presentar las autorizaciones pendientes para TI
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> TI_AU(string tiId, bool cs)
        {
            // GENERAR VARIABLE QUE ME INDIQUE QUE TIPO DE TR ESTOY HACIENDO PARA AUTOMATIZAR MENSAJES DE CABECERA


            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            string? volver = Url.Action("index", "trint", new { area = "pocketppal", tiId, cs });
            switch (tiId)
            {
                case "S":
                    ViewBag.AppItem = new AppItem { Nombre = "TR Interna entre Sucursales", VolverUrl = volver ?? "#" };
                    break;
                case "D":
                    ViewBag.AppItem = new AppItem { Nombre = "TR Interna entre Depositos", VolverUrl = volver ?? "#" };
                    break;
                case "E":
                    ViewBag.AppItem = new AppItem { Nombre = "TR Interna entre Depositos (SIN AUTORIZACION)", VolverUrl = volver ?? "#" };
                    break;
                case "B":
                    ViewBag.AppItem = new AppItem { Nombre = "TR Interna entre Box", VolverUrl = volver ?? "#" };
                    break;
                case "O":
                    ViewBag.AppItem = new AppItem { Nombre = "TR Interna entre Box (SIN AUTORIZACION)", VolverUrl = volver ?? "#" };
                    break;
            }
            TI_ModId = tiId;
            TI_CS = cs;
            var ti = TIActual;
            ti.TipoTI = TI_ModId;
            ti.SinAU = TI_CS;
            TIActual = ti;

            return View("TI_AU", TIActual);
        }

        #endregion

        #region Acciones POST

        [HttpPost]
        public async Task<IActionResult> ObtenerAutorizacionesPendientes()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            GridCore<AutorizacionTIDto> grid;
            try
            {
                var autos = await _productoServicio.TRObtenerAutorizacionesPendientes(AdministracionId, UserName, TI_ModId, TokenCookie);

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
                    ti.TipoTI = TI_ModId;
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
        #endregion

        #endregion  //VISTA 01

        #region VISTA 02

        /// <summary>
        /// SELECCIONADA LA AUTORIZACION PENDIENTE SE PRESENTARAN EN PANTALLA LOS PRODUCTOS A INCLUIR BUSCANDOLOS POR BOX O RUBRO
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> TiProdBoxRubro()
        {


            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            string? volver;
            var ti = TIActual;
            List<string> list = new List<string>() { "S", "D", "B" };
            if (list.Contains(ti.TipoTI))
            {
                volver = Url.Action("TI_AU", "trint", new { area = "pocketppal", tiId = TI_ModId, cs = TI_CS });
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
        public async Task<IActionResult> TICargaCarrito(bool esrubro, bool esbox, string? boxid, string? rubroid, string? rubrogid, string tiId = "")
        {
            string? volver;
            AutorizacionTIDto ti;
            try
            {

                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }
                ti = TIActual;
                //tengo que verificar si la acción es llamada desde una autorización o viene directamente desde un proceso sin AU
                if ((string.IsNullOrEmpty(tiId) || string.IsNullOrWhiteSpace(tiId)) && string.IsNullOrEmpty(boxid) && string.IsNullOrEmpty(rubroid))
                {
                    throw new NegocioException(Constantes.MensajeError.TI_PROC_SIN_ID);
                }

                if ((tiId.Equals(Constantes.ValoresDefault.TI_Dep_SAU) || tiId.Equals(Constantes.ValoresDefault.TI_Box_SAU))&& string.IsNullOrEmpty(ti.Ti))
                {
                    #region Se llama al generador de nuevo TR para operación sin AU
                    var resu = await _productoServicio.TINueva_SinAu(tiId, AdministracionId, UserName, TokenCookie);
                    if (resu.Ok)
                    {
                        ti = new();
                        ti.Ti = resu.Entidad.Ti; //este es el identificador de la autorizacion pendiente temporal                        
                    }
                    else
                    {
                        throw new NegocioException("No se pudo generar el Nuevo TI. Intente de nuevo más tarde.");
                    }
                    #endregion

                    TI_ModId = tiId; //resguardamos que es valor E u O
                    TI_CS = false;

                    ti.TipoTI = TI_ModId;
                    ti.SinAU = true;
                    TIActual = ti;
                    ListadoTIAutoPendientes = new();
                }

                if (!esrubro && !esbox)
                {
                    //verificamos que luego de cargar producto no conocemos cual es la lista que se debe presentar. Por lo qeu se deberá tomar los valores resguardados en TIActual
                    //ti = TIActual;
                    esrubro = ti.EsRubro;
                    rubrogid = ti.RubroGId;
                    rubroid = ti.RubroId;
                    esbox = ti.EsBox;
                    boxid = ti.BoxId;
                }
                else
                {
                    //resguardamos la seleccion realizada ya que si se conoce que se pide
                    //ti = TIActual;
                    ti.EsRubro = esrubro;
                    ti.RubroGId = rubrogid;
                    ti.RubroId = rubroid ?? "%";
                    ti.EsBox = esbox;
                    ti.BoxId = boxid ?? "%";

                    TIActual = ti;
                }

                if (tiId.Equals(Constantes.ValoresDefault.TI_Dep_SAU) || tiId.Equals(Constantes.ValoresDefault.TI_Box_SAU))
                {
                    volver = Url.Action("index", "trint", new { area = "pocketppal" });
                }
                else
                {
                    volver = Url.Action("TiProdBoxRubro", "trint", new { area = "pocketppal" });
                }

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
                List<TiListaProductoDto> regs = await _productoServicio.BuscaTIListaProductos(tr: TIActual.Ti, admId: AdministracionId, usuId: UserName, boxid: selec.BoxId??"%", rubId: selec.RubroId??"%", token: TokenCookie);
                ListaProductosActual = regs;
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
            AutorizacionTIDto sel;
            TiListaProductoDto prod;
            try
            {

                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }

                sel = TIActual;

                //se verifica que si el pid no trae productos, pero es proceso B o D, deberia continuar sin buscar producto
                //en el if siguiente niego toda la premisa... si no se cumple la premisa, DEBE BUSCAR.
                if (!((string.IsNullOrWhiteSpace(pId) || string.IsNullOrEmpty(pId)) && (TI_ModId.Equals("E") || TI_ModId.Equals("O"))))
                {

                    prod = ListaProductosActual.FirstOrDefault(x => x.P_id == pId);
                    if (prod == null)
                    {
                        throw new Exception("El producto buscado no puede ser encontrado. Intente de nuevo, seleccione la lista y seleccione el producto a cargar.");
                    }
                    sel.PId = pId; //le asigno el pId para tenerlo resguardado. 
                    sel.PBoxId = prod.Box_id;
                    sel.PUnidPres = prod.Unidad_pres;
                    sel.PPedido = prod.Pedido;
                }



                TIActual = sel;

                volver = Url.Action("TICargaCarrito", "trint", new { area = "pocketppal", esrubro = sel.EsRubro, esbox = sel.EsBox, boxid = sel.BoxId, rubroid = sel.RubroId, rubrogid = sel.RubroGId, tiId = TI_ModId });
                ViewBag.AppItem = new AppItem { Nombre = "TI - Carga Carrito", VolverUrl = volver ?? "#" };

                ////para validar la fecha de vencimiento
                //ViewBag.FechaCotaJS = _settings.FechaVtoCota;


                return View(sel);
            }

            catch (Exception ex)
            {
                sel = TIActual;
                TempData["error"] = ex.ToString();
                return RedirectToAction("TICargaCarrito", "trint", new { area = "pocketppal", esrubro = sel.EsRubro, esbox = sel.EsBox, boxid = sel.BoxId, rubroid = sel.RubroId, rubrogid = sel.RubroGId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LimpiaProductoCarrito(string p_id)
        {
            try
            {
                var ti = TIActual;

                TiProductoCarritoDto request = new TiProductoCarritoDto();
                request.Ti = ti.Ti;
                request.AdmId = AdministracionId;
                request.UsuId = UserName;
                request.BoxId = ti.PBoxId;
                request.Desarma = true;
                request.Pid = p_id;
                request.Unidad_pres = ti.PUnidPres;
                request.Bulto = 0;
                request.Us = 0;
                request.Cantidad = 0;
                request.Fvto = DateTime.MinValue.ToStringYYYYMMDD();

                RespuestaGenerica<RespuestaDto> resp = await _productoServicio.ResguardarProductoCarrito(request, TokenCookie);

                if (resp.Ok)
                {
                    return Json(new { error = false, warn = false, msg = $"Producto {ProductoBase.P_desc} fue limpiado exitosamente" });
                }
                else { return Json(new { error = false, warn = true, msg = resp.Mensaje }); }
            }
            catch (NegocioException ex)
            {
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name}params: {p_id}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} params: {p_id} ");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} params: {p_id} ");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ResguardarProductoCarrito(string p_id, int up, int bulto, decimal unid, decimal cantidad, string? fv, bool desarma = true)
        {
            try
            {
                var ti = TIActual;
                if (cantidad < 1)
                {
                    return Json(new { error = false, warn = true, msg = $"La cantidades de los productos a cargar siempre tienen que ser positivas, mayores a 0 (cero)." });
                }
                if (ti.PPedido < cantidad && (!TIActual.SinAU || !desarma)) //verificamos las cantidades siempre y cuando haya una autorización o en el caso de transferencia de box completo con desarma = false
                {
                    return Json(new { error = false, warn = true, msg = $"No se puede cargar más unidades o cantidades ({cantidad}) que las pedidas ({ti.PPedido})" });
                }

                TiProductoCarritoDto request = new TiProductoCarritoDto();

                request.Ti = ti.Ti;

                request.AdmId = AdministracionId;
                request.UsuId = UserName;
                request.BoxId = ti.PBoxId;
                request.Desarma = desarma;
                request.Pid = p_id;
                request.Unidad_pres = up;
                request.Bulto = bulto;
                request.Us = unid;
                request.Cantidad = cantidad;
                request.Fvto = fv ?? DateTime.Today.AddDays(_settings.FechaVtoCota).ToStringYYYYMMDD();   ///debo traer fecha de vencimiento del producto a mostrar


                RespuestaGenerica<RespuestaDto> resp = await _productoServicio.ResguardarProductoCarrito(request, TokenCookie);

                if (resp.Ok)
                {
                    return Json(new { error = false, warn = false, msg = $"Producto {ProductoBase.P_desc} fue cargado exitosamente" });
                }
                else { return Json(new { error = false, warn = true, msg = resp.Mensaje }); }
            }
            catch (NegocioException ex)
            {
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name}params: {p_id} {up} {bulto} {unid} {cantidad} {fv}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} params: {p_id} {up} {bulto} {unid} {cantidad} {fv}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} params: {p_id} {up} {bulto} {unid} {cantidad} {fv}");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult ValidarBoxIngresado(string boxId)
        {
            AutorizacionTIDto sel;
            try
            {

                sel = TIActual;
                if ((sel.TipoTI.Equals("E") || sel.TipoTI.Equals("O") && sel.SinAU))
                {
                    sel.PBoxId = boxId;
                    TIActual = sel;
                    return Json(new { error = false, warn = false, msg = "Box Correcto" });
                }
                if (sel.PBoxId.Equals(boxId))
                {
                    return Json(new { error = false, warn = false, msg = "Box Correcto" });
                }
                else
                {
                    throw new NegocioException("El BOX ingresado no corresponde al Box esperado");
                }

            }
            catch (NegocioException ex)
            {
                sel = TIActual;
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} boxId ingresado:{boxId} - Box Esperado {sel.BoxId}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                sel = TIActual;
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} boxId ingresado:{boxId} - Box Esperado {sel.BoxId}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                sel = TIActual;
                _logger.LogError($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} boxId ingresado:{boxId} - Box Esperado {sel.BoxId}");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult ValidarProductoIngresado(string pId)
        {
            AutorizacionTIDto sel;
            try
            {
                sel = TIActual;
                if ((sel.TipoTI.Equals("E") || sel.TipoTI.Equals("O") && sel.SinAU))
                {
                    return Json(new { error = false, warn = false, msg = "Producto es Correcto" });
                }
                if (sel.PId.Equals(pId))
                {
                    return Json(new { error = false, warn = false, msg = "Producto es Correcto" });
                }
                else
                {
                    throw new NegocioException("El Producto ingresado no corresponde al Producto esperado");
                }

            }
            catch (NegocioException ex)
            {
                sel = TIActual;
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} Producto ingresado:{pId} - Producto Esperado {sel.PId}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                sel = TIActual;
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} Producto ingresado:{pId} - Producto Esperado {sel.PId}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                sel = TIActual;
                _logger.LogError($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} Producto ingresado:{pId} - Producto Esperado {sel.PId}");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult ObtenerAutorizacionActual()
        {
            AutorizacionTIDto sel;
            try
            {
                sel = TIActual;

                return Json(new { error = false, auto = sel });
            }
            catch (UnauthorizedException ex)
            {
                sel = TIActual;
                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} No se pudo Cargar la Autorización Actual en la vista");
                return Json(new { error = true, msg = "Las credenciales se han vencido. Debera autenticarse nuevamente." });
            }
            catch (Exception ex)
            {
                sel = TIActual;
                _logger.LogError($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} No se pudo Cargar la Autorización Actual en la vista");
                return Json(new { error = true, msg = "No se pudo Cargar la Autorización Actual en la vista" });
            }
        }


        #endregion

        #region PASO 4
        [HttpPost]
        public async Task<JsonResult> ControlSalidaTI(string ti)
        {
            AutorizacionTIDto sel;
            try
            {
                sel = TIActual;
                if (!sel.Ti.Equals(ti))
                {
                    throw new NegocioException("No se Reconoce el Identificador de la Transferencia. Intente nuevamente, sino salga e ingrese de nuevo.");
                }


                #region Se realiza el control de Salida
                var res = await _productoServicio.ControlSalidaTI(sel.Ti, AdministracionId, UserName, TokenCookie);

                if (res.Ok)
                {
                    return Json(new { error = false, warn = false, msg = "Control de Salida superado." });
                }
                else
                {
                    return Json(new { error = true, warn = false, msg = res.Entidad.resultado_msj });
                }

                #endregion

            }
            catch (NegocioException ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {

                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} No se pudo Cargar la Autorización Actual en la vista");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} Error durante el proceso");
                return Json(new { error = true, warn = false, msg = "Algo no fue bien en el procesamiento. Ingrese de nuevo si fuera necesario. " });
            }
        }

        [HttpGet]
        public IActionResult ConfirmacionTI()
        {
            string? volver;
            AutorizacionTIDto sel;
            TiListaProductoDto prod;
            try
            {

                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }

                sel = TIActual;

                volver = Url.Action("TICargaCarrito", "trint", new { area = "pocketppal", esrubro = sel.EsRubro, esbox = sel.EsBox, boxid = sel.BoxId, rubroid = sel.RubroId, rubrogid = sel.RubroGId, tiId = TI_ModId });
                ViewBag.AppItem = new AppItem { Nombre = "TI - Confirmación de TR", VolverUrl = volver ?? "#" };

                ////para validar la fecha de vencimiento
                //ViewBag.FechaCotaJS = _settings.FechaVtoCota;


                return View(sel);
            }

            catch (Exception ex)
            {
                sel = TIActual;
                TempData["error"] = ex.ToString();
                return RedirectToAction("TICargaCarrito", "trint", new { area = "pocketppal", esrubro = sel.EsRubro, esbox = sel.EsBox, boxid = sel.BoxId, rubroid = sel.RubroId, rubrogid = sel.RubroGId });
            }
        }

        [HttpPost]
        //el valor "" de boxDest se refiere a que a TR entre Sucursales no se puede especificar.
        public async Task<JsonResult> ConfirmacionFinalTI(string ti, string boxDest = "")
        {
            AutorizacionTIDto sel;
            try
            {
                sel = TIActual;
                if (!sel.Ti.Equals(ti))
                {
                    throw new NegocioException("No se Reconoce el Identificador de la Transferencia. Intente nuevamente, sino salga e ingrese de nuevo.");
                }

                if ((string.IsNullOrEmpty(boxDest) || string.IsNullOrWhiteSpace(boxDest)) && !sel.TipoTI.Equals("S"))
                {
                    throw new NegocioException("NO SE HA ESPECIFICADO EL BOX FINAL. VERIFIQUE");
                }


                #region Se realiza el control de Salida
                var request = new TIRequestConfirmaDto { Ti = ti, AdmId = AdministracionId, Usu = UserName, BoxDest = boxDest };
                var res = await _productoServicio.TIConfirma(request, TokenCookie);

                if (res.Ok)
                {
                    return Json(new { error = false, warn = false, msg = "Control de Salida superado." });
                }
                else
                {
                    return Json(new { error = true, warn = false, msg = res.Entidad.resultado_msj });
                }

                #endregion

            }
            catch (NegocioException ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {

                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} No se pudo Cargar la Autorización Actual en la vista");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} Error durante el proceso");
                return Json(new { error = true, warn = false, msg = "Algo no fue bien en el procesamiento. Ingrese de nuevo si fuera necesario. " });
            }
        }

        [HttpPost]
        public async Task<JsonResult> GeneraNuevaTISinAuto(string tipo)
        {
            AutorizacionTIDto sel;
            try
            {
                sel = TIActual;
                var ls = new List<string> { "E", "O" };
                if (!ls.Contains(tipo))
                {
                    throw new NegocioException("No se Puede generar una Nueva TI sin Autorización si no corresponde a TI entre Depositos o entre Box.");
                }

                #region Se realiza el control de Salida
                var res = await _productoServicio.TINueva_SinAu(tipo, AdministracionId, UserName, TokenCookie);

                if (res.Ok)
                {
                    //se generó la TI NUEVA. Por lo que será necesario traerla y resguardarla.
                    await ObtenerTIPendiente(tipo, res.Entidad.Tit_id);

                    return Json(new { error = false, warn = false, msg = "Control de Salida superado." });
                }
                else
                {
                    return Json(new { error = true, warn = false, msg = res.Entidad.resultado_msj });
                }

                #endregion

            }
            catch (NegocioException ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {

                _logger.LogWarning($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} No se pudo Cargar la Autorización Actual en la vista");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} -{this.GetType().Name} {MethodBase.GetCurrentMethod().Name} Error durante el proceso");
                return Json(new { error = true, warn = false, msg = "Algo no fue bien en el procesamiento. Ingrese de nuevo si fuera necesario. " });
            }
        }


        #endregion
    }
}
