using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Implementacion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using System.Reflection;
using X.PagedList;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class RTIController : ControladorBase
    {
        private readonly AppSettings _settings;
        private readonly ILogger<RTIController> _logger;
        private readonly IRemitoServicio _remitoServicio;
        private readonly IProductoServicio _productoServicio;

        public RTIController(IOptions<AppSettings> option, IHttpContextAccessor context,
            ILogger<RTIController> logger, IRemitoServicio remitoServicio, IProductoServicio productoServicio) : base(option, context)
        {
            _settings = option.Value;
            _logger = logger;
            _remitoServicio = remitoServicio;
            _productoServicio = productoServicio;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            string? volver = Url.Action("index", "home", new { area = "" });
            ViewBag.AppItem = new AppItem { Nombre = "Recepción de Transferencia de otra Sucursal", VolverUrl = volver ?? "#" };
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerRemitos()
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<RemitoGenDto> datosIP;
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return Json(new { error = false, warn = true, auth = true, msg = "Debe volverse a autenticar" });
            }
            try
            {
                List<RemitoGenDto> remitos = await _remitoServicio.ObtenerRemitosTransferidos(admId: AdministracionId, token: TokenCookie);
                RemitosPendientes = remitos;
                datosIP = ObtenerRPRProdGrid(RemitosPendientes);

                return PartialView("_rtiRemitosPendientes", datosIP);
            }
            catch (NegocioException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Autorizaciones pendientes");
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        private GridCore<RemitoGenDto> ObtenerRPRProdGrid(List<RemitoGenDto> listaRpr)
        {

            var lista = new StaticPagedList<RemitoGenDto>(listaRpr, 1, 999, listaRpr.Count);

            return new GridCore<RemitoGenDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
        }

        [HttpGet]
        public IActionResult CargaProductos(string rm)
        {
            RemitoGenDto remito = new();
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    TempData["warn"] = $"Su sesión ha expirado. Por favor, autentiquese nuevamente.";

                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }

                remito = RemitosPendientes.SingleOrDefault(x => x.re_compte.Equals(rm));
                if (remito == null)
                {
                    TempData["warn"] = $"No se pudo seleccionar el Remito N° {rm}. Intente de nuevo";
                }

                if (RemitoActual == null || !RemitoActual.re_compte.Equals(rm))
                {
                    //en el caso que haga para atras y vuelva a elegir la misma  Autorización Pendiente no se elimina nada. 
                    //si es distinto como en este caso, se inicializan las variables.
                    InicializaVariablesSessionRTI();
                    RemitoActual = remito;


                }

                string? volver = Url.Action("ObtenerRemitos", "rti", new { area = "pocketppal" });
                ViewBag.AppItem = new AppItem { Nombre = "Carga de Productos - Remito Actual", VolverUrl = volver ?? "#", BotonEspecial = true };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Autorizaciones pendientes");
                TempData["error"] = "Hubo algun problema al intentar obtener las Autorizaciones Pendientes. Si el problema persiste informe al Administrador";
                return RedirectToAction("Index");
            }

            return View(RemitoActual);
        }

        private void InicializaVariablesSessionRTI()
        {
            RemitoActual = new();
            ProductoGenRegs = [];
            ProductoTemp = new();
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

                if (ProductoBase.P_con_vto.Equals("S"))
                {
                    var fecha = vto.ToDateTimeOrNull();
                    var tope = ProductoBase.p_con_vto_ctl;
                    if (fecha == null)
                    {
                        return Json(new { error = true, msg = "La fecha recepcionada no es válida. Verifique." });
                    }
                    else if (fecha < tope)
                    {
                        return Json(new { error = true, msg = $"La fecha recepcionada no puede ser menor a {tope}. Verifique, por favor." });
                    }
                }

                //if (!string.IsNullOrEmpty(vto) && !string.IsNullOrWhiteSpace(vto))
                //{
                //    var fecha = vto.ToDateTimeOrNull();
                //    var tope = DateTime.Today.AddDays(_settings.FechaVtoCota);

                //    if (fecha == null)
                //    {
                //        return Json(new { error = true, msg = "La fecha recepcionada no es válida. Verifique." });
                //    }
                //    else if (fecha < tope)
                //    {
                //        return Json(new { error = true, msg = $"La fecha recepcionada no puede ser menor a {tope}. Verifique, por favor." });
                //    }
                //}

                //valido cantidad. Si el resultado es igual a 0 dar error
                var cantidad = ProductoBase.Up_id.Equals("07") ? (up * bulto) + unidad : bulto;

                if (cantidad <= 0)
                {
                    return Json(new { error = true, msg = "La cantidad dió como resultado 0 (cero). Verifique." });
                }

                //armo producto a resguardar
                var item = new ProductoGenDto();
                item.re = RemitoActual.re_compte;
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

            return PartialView("_rtiProductosCargardos", datosIP);
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


        [HttpGet]
        public IActionResult CargaUL()
        {
            string? volver = Url.Action("CargaProductos", "rti", new { area = "pocketppal", rm = RemitoActual.re_compte });
            //ViewBag.AppItem = new AppItem { Nombre = $"Auto:{auto.Rp}-{auto.Cta_denominacion}" };
            ViewBag.AppItem = new AppItem { Nombre = "Carga de Productos en UL", VolverUrl = volver ?? "#", BotonEspecial = false };

            return View(RemitoActual);
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarRTI(string ul)
        {
            string msg = "";
            try
            {
                if (string.IsNullOrEmpty(ul) || string.IsNullOrWhiteSpace(ul))
                {
                    throw new NegocioException("No ha ingresado el numero de la Unidad de Lectura.");
                }
                var analisis = ul.Trim().Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (analisis.Length != 4)
                {
                    throw new NegocioException("Verifique la Unidad de Lectura. Algo no esta bien.");
                }
                if (analisis[0].Length != 3)
                {
                    throw new NegocioException("Verifique la Unidad de Lectura. Algo no esta bien.");
                }
                if (analisis[1].Length != 2)
                {
                    throw new NegocioException("Verifique la Unidad de Lectura. Algo no esta bien.");
                }
                if (analisis[2].Length != 8)
                {
                    throw new NegocioException("Verifique la Unidad de Lectura. Algo no esta bien.");
                }
                if (analisis[3].Length != 2)
                {
                    throw new NegocioException("Verifique la Unidad de Lectura. Algo no esta bien.");
                }
                var lista = ProductoGenRegs;
                foreach (var item in lista)
                {
                    item.ul_id = ul;
                    item.re = RemitoActual.re_compte;

                }
                //lista.ForEach(x=> x.ul_id = ul).ForeEach(s=> s.re_compte = RemitoActual.re_compte);
                var res = await _remitoServicio.RTRCargarConteos(lista, TokenCookie);

                if (res.resultado == 0)
                {
                    InicializaVariablesSessionRTI();
                    return Json(new { error = false, warn = false, msg = $"La Carga del {ul} fue satisfactoria" });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.resultado_msj });
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
        public IActionResult AlmacenajeBox()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            string? volver = Url.Action("rti", "almacen", new { area = "gestion" });
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
                var res = await _productoServicio.ValidarUL(ul.ToUpper(), AdministracionId, "RI", TokenCookie);  //para RPR es RC, para RTR es RI
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
                var res = await _productoServicio.ConfirmaBoxUl(box, ul, AdministracionId, sm: "RI", TokenCookie);
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
