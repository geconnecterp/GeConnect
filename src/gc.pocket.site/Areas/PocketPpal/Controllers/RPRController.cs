using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen.Rpr;
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
    public class RPRController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<RPRController> _logger;
        private readonly IProductoServicio _productoServicio;
        private readonly AppSettings _settings;

        public RPRController(IOptions<AppSettings> options, IHttpContextAccessor context, IOptions<MenuSettings> options1,
            ILogger<RPRController> logger, IProductoServicio productoServicio, IDepositoServicio depositoServicio) : base(options, context)
        {
            _menuSettings = options1.Value;
            _logger = logger;
            _productoServicio = productoServicio;
            _settings = options.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return await MetodoGenericoInicial();

        }

        [HttpGet]
        public async Task<IActionResult> ModificaDetalleUL()
        {

            return await MetodoGenericoInicial(true);

        }

        private async Task<IActionResult> MetodoGenericoInicial(bool modifica = false)
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            List<AutorizacionPendienteDto> pendientes;
            GridCore<AutorizacionPendienteDto> grid;

            try
            {
                //se buscará todas las autorizaciones pendientes
                pendientes = await _productoServicio.RPRObtenerAutorizacionPendiente(AdministracionId, TokenCookie);
                //resguardo lista de autorizaciones pendientes 
                AutorizacionesPendientes = pendientes;
                grid = ObtenerAutorizacionPendienteGrid(pendientes);

                string volver = Url.Action("rpr", "almacen", new { area = "gestion" });
                if (modifica)
                {
                    ViewBag.AppItem = new AppItem { Nombre = "Detalle de autorizaciones pendientes - MODIFICA UL", VolverUrl = volver ?? "#" };
                }
                else
                {
                    ViewBag.AppItem = new AppItem { Nombre = "Detalle de autorizaciones pendientes", VolverUrl = volver ?? "#" };
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Autorizaciones pendientes");
                TempData["error"] = "Hubo algun problema al intentar obtener las Autorizaciones Pendientes. Si el problema persiste informe al Administrador";
                grid = new();
            }
            return View(grid);
        }

        private GridCore<AutorizacionPendienteDto> ObtenerAutorizacionPendienteGrid(List<AutorizacionPendienteDto> pendientes)
        {

            var lista = new StaticPagedList<AutorizacionPendienteDto>(pendientes, 1, 999, pendientes.Count);

            return new GridCore<AutorizacionPendienteDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Cta_denominacion", SortDir = "ASC" };
        }

        [HttpGet]
        public IActionResult ResguardarAutorizacionProveedor(string rp, bool esUpdate = false)
        {
            AutorizacionPendienteDto auto = new();
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }


                auto = AutorizacionesPendientes.SingleOrDefault(x => x.Rp.Equals(rp));
                if (auto == null)
                {
                    TempData["warn"] = $"No se pudo seleccionar la Autorización {rp}. Intente de nuevo.";
                    return RedirectToAction("Index");
                }

                if (AutorizacionPendienteSeleccionada == null || !AutorizacionPendienteSeleccionada.Rp.Equals(rp) || AutorizacionPendienteSeleccionada.EsModificacion)
                {
                    //en el caso que haga para atras y vuelva a elegir la misma  Autorización Pendiente no se elimina nada. 
                    //si es distinto como en este caso, se inicializan las variables.
                    InicializaVariablesSessionRPR();

                    AutorizacionPendienteSeleccionada = auto;

                }
                string? volver;
                if (esUpdate)
                {
                    //si es modificación de UL voy a marcar la rp
                    auto.EsModificacion = true;
                    AutorizacionPendienteSeleccionada = auto;
                
                    volver = Url.Action("ModificaDetalleUL", "rpr", new { area = "pocketppal" });

                    ViewBag.AppItem = new AppItem { Nombre = $"Detalle de ULs de {auto.Rp}", VolverUrl = volver ?? "#", BotonEspecial = true };
                }
                else
                {
                    volver = Url.Action("index", "rpr", new { area = "pocketppal" });

                    ViewBag.AppItem = new AppItem { Nombre = "Carga de Productos para la Aut.Pendiente", VolverUrl = volver ?? "#", BotonEspecial = true };
                }


                if (AutorizacionPendienteSeleccionada.EsModificacion)
                {
                    //es modificación, redirecciono a la vista que presenta las distintas UL de la solicitud
                    return View("DetalleRpUls", AutorizacionPendienteSeleccionada);
                }


                ViewBag.FechaCotaJS = _settings.FechaVtoCota;
                return View("CargarProductos", AutorizacionPendienteSeleccionada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Autorizaciones pendientes");
                TempData["error"] = "Hubo algun problema al intentar obtener las Autorizaciones Pendientes. Si el problema persiste informe al Administrador";
                return RedirectToAction("Index");
            }
        }

        private void InicializaVariablesSessionRPR()
        {
            AutorizacionPendienteSeleccionada = new();
            ProductoGenRegs = [];
            ProductoTemp = new();
        }

        [HttpPost]
        public async Task<ActionResult> ObtenerDetalleUls()
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<RPRxULDto> grillaDatos;

            try
            {
                var regs = await _productoServicio.RPRxUL(AutorizacionPendienteSeleccionada.Rp, TokenCookie);
                grillaDatos = GenerarGrilla(regs, "p_id");
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
            return PartialView("_detalleULsGrid", grillaDatos);
        }

        [HttpGet]
        ///ESTE METODO ES EXCLUSIVO DE MODIFICACIÓN. NO OBSTANTE SU VISTA SE USABA ORIGINALMENTE PARA PRESENTAR 
        ///LOS PRODUCTOS DESDE OTRA VISTA ResguardarAutorizacionProveedor
        public async Task<IActionResult> CargarProductos(string ul)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }

                if (string.IsNullOrEmpty(ul) || string.IsNullOrWhiteSpace(ul))
                {
                    TempData["warn"] = "No se recepcionó la UL seleccionada";
                    return RedirectToAction("ResguardarAutorizacionProveedor", "rpr", new { area = "pocketppal", ul = AutorizacionPendienteSeleccionada.Rp, esUpdate = true });
                }
                //debo resguardar la UL en la AutorizacionPendienteSeleccionada
                var auto = AutorizacionPendienteSeleccionada;
                auto.Ul = ul;
                AutorizacionPendienteSeleccionada = auto;

                var prods = await _productoServicio.RPRxULDetalle(ul, TokenCookie);
                var lista = prods.Select(x => new ProductoGenDto
                {
                    bulto = x.bulto,
                    cantidad = x.cantidad,
                    cantidad_total = x.up_id == "07" ? (x.unidad_pres * x.bulto) + x.us : x.us,
                    item = x.item,
                    p_con_vto = x.p_con_vto,
                    p_con_vto_ctl = x.p_con_vto,
                    p_con_vto_min = x.p_con_vto_min,
                    p_desc = x.p_desc,
                    p_id = x.p_id,
                    p_id_barrado = x.p_id_barrado,
                    ul_id = x.ul_id,
                    p_id_prov = x.p_id_prov,
                    unidad_pres = x.unidad_pres,
                    up_id = x.up_id,
                    us = x.us,
                    usu_id = x.usu_id,
                    vto = x.vto.ToDateTimeOrNull()
                }).ToList();
                ProductoGenRegs = lista;

                string volver = Url.Action("ResguardarAutorizacionProveedor", "rpr", new { area = "pocketppal", rp = AutorizacionPendienteSeleccionada.Rp, esUpdate = true });

                ViewBag.AppItem = new AppItem { Nombre = $"Detalle de ULs de {AutorizacionPendienteSeleccionada.Rp}", VolverUrl = volver ?? "#", BotonEspecial = false };
                return View(AutorizacionPendienteSeleccionada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod().Name}");
                TempData["error"] = "Hubo algun problema. Si el mismo persiste informe al Administrador";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public JsonResult ReguardarProductoEnLista(int up, string vto, int bulto, decimal unidad)
        {
            string msg;
            try
            {
                //validaciones de parametros
                if (up < 1)
                {
                    return Json(new { error = true, msg = "Las unidades del bulto no puede ser 0 (cero) o tener valores negativos. Verifique, por favor." });
                }
                if (bulto < 0)
                {
                    return Json(new { error = true, msg = "Los bultos no puede tener valores negativos. Verifique, por favor." });
                }

                if (unidad < 0)
                {
                    return Json(new { error = true, msg = "Las unidades sueltas no puede tener valores negativos. Verifique, por favor." });
                }

                if (!ProductoBase.Up_id.Equals("07") && up != 1)
                {
                    return Json(new { error = true, msg = "EL PRODUCTO NO ES POR UNIDADES. LA UNIDAD DE PRESENTACIÓN TIENE QUE SER IGUAL A 1 SIEMPRE." });
                }

                if (!string.IsNullOrEmpty(vto) && !string.IsNullOrWhiteSpace(vto))
                {
                    var fecha = vto.ToDateTimeOrNull();
                    var tope = DateTime.Today.AddDays(_settings.FechaVtoCota);

                    if (fecha == null)
                    {
                        return Json(new { error = true, msg = "La fecha recepcionada no es válida. Verifique." });
                    }
                    else if (fecha < tope)
                    {
                        return Json(new { error = true, msg = $"La fecha recepcionada no puede ser menor a {tope}. Verifique, por favor." });
                    }
                }

                //valido cantidad. Si el resultado es igual a 0 dar error
                var cantidad = ProductoBase.Up_id.Equals("07") ? (up * bulto) + unidad : unidad;

                if (cantidad <= 0)
                {
                    return Json(new { error = true, msg = "La cantidad dió como resultado 0 (cero). Verifique." });
                }

                //armo producto a resguardar
                var item = new ProductoGenDto();
                item.rp = AutorizacionPendienteSeleccionada.Rp;
                item.item = ProductoGenRegs.Count + 1;
                item.p_id = ProductoBase.P_id;
                item.p_desc = ProductoBase.P_desc;
                item.up_id = ProductoBase.Up_id;
                //item.Cta_id = ProductoBase.Cta_id;
                item.p_id_prov = ProductoBase.P_id_prov;
                item.p_id_barrado = ProductoBase.P_id_barrado;
                item.usu_id = UserName;
                item.unidad_pres = up;
                item.bulto = bulto;
                item.us = unidad;
                if (string.IsNullOrEmpty(vto) && string.IsNullOrWhiteSpace(vto))
                {
                    item.vto = null;
                }
                else
                {
                    var f = vto.Split('-', StringSplitOptions.RemoveEmptyEntries);
                    item.vto = new DateTime(f[0].ToInt(), f[1].ToInt(), f[2].ToInt());
                }
                item.cantidad = cantidad;

                var res = ProductoGenRegs.Any(x => x.p_id.Equals(item.p_id));

                if (res)
                {
                    //ya se encuentra cargado el producto, se debe avisar.
                    ProductoTemp = item;
                    msg = $"El Producto {item.p_desc} ya se encuentra cargado. ¿Desea CANCELAR la operación, REMPLAZAR las cantidades existentes o ACUMULAR las cantidades?";
                    return Json(new { error = false, warn = true, msg, p_id = item.p_id });
                }
                else
                {
                    //no esta se carga directamente
                    var lista = ProductoGenRegs;
                    lista.Add(item);
                    ProductoGenRegs = lista;
                    return Json(new { error = false, warn = false });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = true, msg = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult PresentarProductosSeleccionados()
        {
            GridCore<ProductoGenDto> datosIP;

            datosIP = ObtenerRPRProdGrid(ProductoGenRegs);

            return PartialView("_rprProductosCargardos", datosIP);
        }


        private GridCore<ProductoGenDto> ObtenerRPRProdGrid(List<ProductoGenDto> listaRpr)
        {

            var lista = new StaticPagedList<ProductoGenDto>(listaRpr, 1, 999, listaRpr.Count);

            return new GridCore<ProductoGenDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
        }

        [HttpPost]
        public JsonResult EliminarProducto(string p_id)
        {
            //busco el producto.
            try
            {
                ProductoGenDto? item = EliminaProductoBase(p_id);
                return Json(new { error = false, msg = $"El producto {item.p_desc} fue removido de la lista." });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                var msg = "Hubo problemas al intentar eliminar el producto de la lista de Productos seleccionados para el RPR.";
                _logger.LogError(ex, msg);
                return Json(new { error = true, msg });
            }

        }

        private ProductoGenDto EliminaProductoBase(string p_id)
        {
            var item = ProductoGenRegs.SingleOrDefault(p => p.p_id.Equals(p_id));
            if (item == null)
            {
                throw new NegocioException("No se encontró el producto que intenta eliminar de la lista");
            }
            var lista = ProductoGenRegs.Where(p => !p.p_id.Equals(p_id)).ToList();
            ProductoGenRegs = lista;
            return item;
        }

        [HttpPost]
        public JsonResult AcumularProducto()
        {
            try
            {
                //Hubo problemas al intentar acumular el producto con la ya cargada en la lista de Productos seleccionados para el RPR."
                var item = ProductoGenRegs.SingleOrDefault(p => p.p_id.Equals(ProductoTemp.p_id));
                if (item == null)
                {
                    throw new Exception(infraestructura.Constantes.Constantes.MensajeError.RPR_PRODUCTO_NO_ENCONTRADO_ACUMULACION);
                }
                //acumulando
                //se verifica cual es el tipo de unidad que se utiliza
                if (ProductoTemp.up_id.Equals("07"))
                {//son unidades enteras. 
                    if (!ProductoTemp.unidad_pres.Equals(item.unidad_pres))
                    {
                        throw new Exception(infraestructura.Constantes.Constantes.MensajeError.RPR_PRODUCTO_ACUMULACION_UNIDAD_BULTO_DISTINTO);
                    }

                    item.bulto += ProductoTemp.bulto;
                    item.cantidad += ProductoTemp.cantidad;
                    item.us += ProductoTemp.us;
                }
                else
                { //son unidades decimales. Directamente se suman.
                    item.unidad_pres += ProductoTemp.unidad_pres;
                    item.bulto += ProductoTemp.bulto;
                    item.cantidad += ProductoTemp.cantidad;
                }
                //Para agregar el acumulado primero debo sacar el producto de la lista
                _ = EliminaProductoBase(ProductoTemp.p_id);
                //traigo la lista, lo agrego y lo vuelvo a resguardar
                var lista = ProductoGenRegs;
                lista.Add(item);
                ProductoGenRegs = lista;

                return Json(new { error = false, msg = infraestructura.Constantes.Constantes.MensajesOK.RPR_PRODUCTO_ACUMULADO });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Acumular Producto");
                return Json(new { error = true, msg = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult RemplazarProducto()
        {
            try
            {
                //se toma la lista y se extrae el producto a ser reemplazado.
                var lista = ProductoGenRegs.Where(p => !p.p_id.Equals(ProductoTemp.p_id)).ToList();
                //Se agrega a lista el producto que esta en temp
                lista.Add(ProductoTemp);
                //resguardamos la lista
                ProductoGenRegs = lista;

                return Json(new { error = false, msg = infraestructura.Constantes.Constantes.MensajesOK.RPR_PRODUCTO_REMPLAZADO });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Remplazar Producto");
                return Json(new { error = true, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarRPR(string ul)
        {
            string msg = "";
            try
            {
                if (string.IsNullOrEmpty(ul) || string.IsNullOrWhiteSpace(ul))
                {
                    throw new NegocioException("No ha ingresado el numero de la Unidad de Lectura.");
                }
                var auto = AutorizacionPendienteSeleccionada;
                if (!auto.EsModificacion)
                {
                    var analisis = ul.Trim().Split('-', StringSplitOptions.RemoveEmptyEntries);
                    if (analisis.Length != 2)
                    {
                        throw new NegocioException("Verifique la Unidad de Lectura. Algo no esta bien.");
                    }
                    if (analisis[0].Length != 5)
                    {
                        throw new NegocioException("Verifique la Unidad de Lectura. Algo no esta bien.");
                    }
                    if (analisis[1].Length != 10)
                    {
                        throw new NegocioException("Verifique la Unidad de Lectura. Algo no esta bien.");
                    }
                }
                var res = await _productoServicio.RPRRegistrarProductos(ProductoGenRegs, AdministracionId, ul,auto.EsModificacion, TokenCookie);

                if (res.Resultado == 0)
                {
                    //inicializa variables de sesion
                    InicializaVariablesSessionRPR();
                    return Json(new { error = false, warn = false, msg = $"La Carga del {ul} fue satisfactoria" });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.Resultado_msj });
                }
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod().Name} Error al intentar confirmar");
                msg = $"Hubo algún inconveniente al intentar Confirmar la Autorización.";
            }

            return Json(new { error = true, msg });
        }

        [HttpGet]
        public IActionResult CargaUL()
        {
            //ComboDepositos();
            ViewBag.AppItem = new AppItem { Nombre = $"Auto:{AutorizacionPendienteSeleccionada.Rp}-{AutorizacionPendienteSeleccionada.Cta_denominacion}" };
            string? volver;
            var auto = AutorizacionPendienteSeleccionada;
            if (auto.EsModificacion)
            {
                volver = Url.Action("CargarProductos", "rpr", new { area = "pocketppal", ul = auto.Ul });
                ViewBag.AppItem = new AppItem { Nombre = "Carga de Productos en UL ACTUAL", VolverUrl = volver ?? "#", BotonEspecial = false };
            }
            else
            {
                volver = Url.Action("ResguardarAutorizacionProveedor", "rpr", new { area = "pocketppal", rp = auto.Rp });
                ViewBag.AppItem = new AppItem { Nombre = "Carga de Productos en UL", VolverUrl = volver ?? "#", BotonEspecial = false };
            }
            //ViewBag.AppItem = new AppItem { Nombre = $"Auto:{auto.Rp}-{auto.Cta_denominacion}" };

            return View(AutorizacionPendienteSeleccionada);
        }



        [HttpGet]
        public IActionResult AlmacenajeBox()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            string? volver = Url.Action("rpr", "almacen", new { area = "gestion" });
            //ViewBag.AppItem = new AppItem { Nombre = $"Auto:{auto.Rp}-{auto.Cta_denominacion}" };
            ViewBag.AppItem = new AppItem { Nombre = "Almacenamiento en BOX", VolverUrl = volver ?? "#", BotonEspecial = false };

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ValidarUl(string ul)
        {
            try
            {
                if (string.IsNullOrEmpty(ul) || string.IsNullOrWhiteSpace(ul))
                {
                    throw new NegocioException("No se recepcionó la UL. Verifique");
                }
                var res = await _productoServicio.ValidarUL(ul.ToUpper(), AdministracionId, "RC", TokenCookie);  //para RPR es RC, para RTR es RI
                if (res.resultado == 0)
                {
                    return Json(new { error = false, warn = false, msg = "La validación del UL fue exitosa." });
                }
                else
                {
                    throw new NegocioException(res.resultado_msj);
                }
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al validar la UL {ul}");
                return Json(new { error = true, msg = "Algo no fué bien en la Validación de la UL. Intente nuevamente." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ValidarBox(string box)
        {
            try
            {
                if (string.IsNullOrEmpty(box) || string.IsNullOrWhiteSpace(box))
                {
                    throw new NegocioException("No se recepcionó el Box. Verifique");
                }
                var res = await _productoServicio.ValidarBox(box, AdministracionId, TokenCookie);
                if (res.Resultado == 0)
                {
                    return Json(new { error = false, warn = false, msg = "La validación del Box fue exitosa.", box = res.Box_id_sugerido });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al validar el Box {box}");
                return Json(new { error = true, msg = "Algo no fué bien en la Validación de la UL. Intente nuevamente." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmaBoxUl(string box, string ul)
        {
            try
            {
                if (string.IsNullOrEmpty(ul) || string.IsNullOrWhiteSpace(ul))
                {
                    throw new NegocioException("No se recepcionó la UL. Verifique");
                }
                if (string.IsNullOrEmpty(box) || string.IsNullOrWhiteSpace(box))
                {
                    throw new NegocioException("No se recepcionó el Box. Verifique");
                }
                var res = await _productoServicio.ConfirmaBoxUl(box, ul.ToUpper(), AdministracionId, sm: "RC", TokenCookie);
                if (res.Resultado == 0)
                {
                    return Json(new { error = false, warn = false, msg = "Se realizó exitosamente el ingreso de Stock de la Unidad de Lectura." });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al Confirmar el Box {box} Ul {ul}");
                return Json(new { error = true, msg = "Algo no fué bien en la Validación de la UL. Intente nuevamente." });
            }
        }
    }
}
