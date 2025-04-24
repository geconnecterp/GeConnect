using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Reflection;
using X.PagedList;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class DevPController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<RPRController> _logger;
        private readonly IProductoServicio _productoServicio;
        private readonly IProducto2Servicio _producto2Servicio;
        private readonly AppSettings _settings;

        public DevPController(IOptions<AppSettings> options, IHttpContextAccessor context, IOptions<MenuSettings> options1,
            ILogger<RPRController> logger, IProductoServicio productoServicio, IDepositoServicio depositoServicio, IProducto2Servicio producto2Servicio) : base(options, context, logger)
        {
            _menuSettings = options1.Value;
            _logger = logger;
            _productoServicio = productoServicio;
            _settings = options.Value;
            _producto2Servicio = producto2Servicio;
        }



        public async Task<IActionResult> Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            string volver = Url.Action("cprev", "almacen", new { area = "gestion" });
            ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Devolución a Proveedores", VolverUrl = volver ?? "#" };

            //inicializamos lista de productos a ajustar
            ProductoGenRegs = new List<ProductoGenDto>();
            BoxSeleccionado = string.Empty;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CargarProductos(string box)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }

                if (string.IsNullOrEmpty(box) || string.IsNullOrWhiteSpace(box))
                {
                    TempData["warn"] = "No se recepcionó el box";
                    return RedirectToAction("index", "rpr", new { area = "pocketppal", ul = AutorizacionPendienteSeleccionada.Rp, esUpdate = true });
                }

                BoxSeleccionado = box;


                string volver = Url.Action("index", "devp", new { area = "pocketppal" });

                ViewBag.AppItem = new AppItem { Nombre = $"CARGA DE PRODUCTOS A DEVOLVER A PROVEEDORES", VolverUrl = volver ?? "#", BotonEspecial = false };
                return View("CargarProductos", box);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod().Name}");
                TempData["error"] = "Hubo algun problema. Si el mismo persiste informe al Administrador";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<JsonResult> ReguardarProductoEnLista(int up, string vto, int bulto, decimal unidad, bool sig = true)
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

                //valido cantidad. Si el resultado es igual a 0 dar error

                if (ProductoBase.Up_id.Equals("07"))
                {
                    bulto *= -1;
                    unidad *= -1;
                }
                else
                {
                    unidad *= -1;
                }

                var cantidad = ProductoBase.Up_id.Equals("07") ? (up * bulto) + unidad : unidad;

                //armo producto a resguardar
                var item = new ProductoGenDto();
                item.depo_id = BoxSeleccionado.Substring(0, 2);
                item.box_id = BoxSeleccionado;
               
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
                    //antes de cargarlo se revisa que el stock actual + ajuste sea >= 0
                    var restk = await _productoServicio.InfoProductoStkBoxes(item.p_id, AdministracionId, item.depo_id, TokenCookie, item.box_id);

                    if (restk.Count > 0)
                    {
                        //hay algun producto en el box.
                        var prod = restk.Single();
                        if ((prod.Ps_stk + item.cantidad) < 0)
                        {
                            return Json(new { error = true, msg = $"Verifique el ajuste del producto {item.p_desc} ya que la devolución daria un Stock NEGATIVO." });
                        }
                    }

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
            GridCoreSmart<ProductoGenDto> datosIP;

            datosIP = ObtenerRPRProdGrid(ProductoGenRegs);

            return PartialView("_productosCargardos", datosIP);
        }


        private GridCoreSmart<ProductoGenDto> ObtenerRPRProdGrid(List<ProductoGenDto> listaRpr)
        {

            var lista = new StaticPagedList<ProductoGenDto>(listaRpr, 1, 999, listaRpr.Count);

            return new GridCoreSmart<ProductoGenDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
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
        public IActionResult PresentaAjustes()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            var box = BoxSeleccionado;


            string volver = Url.Action("CargarProductos", "devp", new { area = "pocketppal", box });

            ViewBag.AppItem = new AppItem { Nombre = $"CARGA DE PRODUCTOS A DEVOLVER AL PROVEEDOR", VolverUrl = volver ?? "#", BotonEspecial = false };
            return View("PresentaAjustes", box);
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarAjustes()
        {
            try
            {
                var box = BoxSeleccionado;
                var res = await _producto2Servicio.DV_CargaConteosPrevios(ProductoGenRegs, AdministracionId, box.Substring(0, 2), box, TokenCookie);
                if (res.Ok)
                {
                    return Json(new { error = false, msg = "Se realizo exitosamente la Carga de Conteos Previos por Devolución a Proveedores." });
                }
                else
                {
                    return Json(new { error = false, msg = res.Mensaje });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error al Confirmar Ajustes", ex);
                return Json(new { error = true, msg = "Hubo algun problema al intentar Confirmar los Conteos previos de Ajustes " });
            }
        }

    }
}
