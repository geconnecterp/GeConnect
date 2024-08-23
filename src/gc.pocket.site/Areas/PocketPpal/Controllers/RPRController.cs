using gc.api.core.Constantes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using gc.pocket.site.Models.ViewModels;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class RPRController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<InfoProdController> _logger;
        private readonly IProductoServicio _productoServicio;

        public RPRController(IOptions<AppSettings> options, IHttpContextAccessor context, IOptions<MenuSettings> options1,
            ILogger<InfoProdController> logger, IProductoServicio productoServicio) : base(options, context)
        {
            _menuSettings = options1.Value;
            _logger = logger;
            _productoServicio = productoServicio;
        }

        public async Task<IActionResult> Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            List<RPRAutorizacionPendienteDto> pendientes;
            GridCore<RPRAutorizacionPendienteDto> grid;
            try
            {
                //se buscará todas las autorizaciones pendientes
                pendientes = await _productoServicio.RPRObtenerAutorizacionPendiente(AdministracionId, TokenCookie);
                //resguardo lista de autorizaciones pendientes 
                AutorizacionesPendientes = pendientes;
                grid = ObtenerAutorizacionPendienteGrid(pendientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Autorizaciones pendientes");
                TempData["error"] = "Hubo algun problema al intentar obtener las Autorizaciones Pendientes. Si el problema persiste informe al Administrador";
                grid = new();
            }
            return View(grid);

        }

        private GridCore<RPRAutorizacionPendienteDto> ObtenerAutorizacionPendienteGrid(List<RPRAutorizacionPendienteDto> pendientes)
        {

            var lista = new StaticPagedList<RPRAutorizacionPendienteDto>(pendientes, 1, 999, pendientes.Count);

            return new GridCore<RPRAutorizacionPendienteDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Cta_denominacion", SortDir = "ASC" };
        }

        [HttpGet]
        public IActionResult ResguardarAutorizacionProveedor(string rp)
        {
            RPRAutorizacionPendienteDto auto = new();
            try
            {
                auto = AutorizacionesPendientes.SingleOrDefault(x => x.Rp.Equals(rp));
                if (auto == null)
                {
                    TempData["Warn"] = $"No se pudo seleccionar la Autorización {rp}. Intente de nuevo.";
                    return RedirectToAction("Index");
                }

                if (RPRAutorizacionPendienteSeleccionada == null || !RPRAutorizacionPendienteSeleccionada.Rp.Equals(rp))
                {
                    //en el caso que haga para atras y vuelva a elegir la misma  Autorización Pendiente no se elimina nada. 
                    //si es distinto como en este caso, se inicializan las variables.
                    RPRAutorizacionPendienteSeleccionada = auto;
                    RPRProductoRegs = [];
                    RPRProductoTemp = new();
                }
                //este viewbag es para que aparezca en la segunda fila del encabezado la leyenda que se quiera.
                //en este caso presenta el numero de autorización pendiente y el proveedor al que le pertenece.
                ViewBag.AppItem = new AppItem { Nombre = $"Auto:{auto.Rp}-{auto.Cta_denominacion}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Autorizaciones pendientes");
                TempData["error"] = "Hubo algun problema al intentar obtener las Autorizaciones Pendientes. Si el problema persiste informe al Administrador";
                return RedirectToAction("Index");
            }

            return View("CargarProductos", RPRAutorizacionPendienteSeleccionada);
        }

        [HttpPost]
        public JsonResult ReguardarProductoEnLista(int up, string vto, int bulto, decimal unidad)
        {
            string msg;
            try
            {
                //armo producto a resguardar
                var item = new RPRProcuctoDto();
                item.Item = RPRProductoRegs.Count + 1;
                item.P_id = ProductoBase.P_id;
                item.P_desc = ProductoBase.P_desc;
                item.Up_id = ProductoBase.Up_id;
                item.Cta_id = ProductoBase.Cta_id;
                item.Usu_id = UserName;
                item.Bulto_up = up;
                item.Bulto = bulto;
                item.Uni_suelta = unidad;
                if (string.IsNullOrEmpty(vto) && string.IsNullOrWhiteSpace(vto))
                {
                    item.Vto = null;
                }
                else
                {
                    var f = vto.Split('-', StringSplitOptions.RemoveEmptyEntries);
                    item.Vto = new DateTime(f[0].ToInt(), f[1].ToInt(), f[2].ToInt());
                }
                item.Cantidad = ProductoBase.Up_id.Equals("07") ? (up * bulto) + unidad : bulto;
                item.Nro_tra = RPRAutorizacionPendienteSeleccionada.Rp;


                var res = RPRProductoRegs.Any(x => x.P_id.Equals(item.P_id));

                if (res)
                {
                    //ya se encuentra cargado el producto, se debe avisar.
                    RPRProductoTemp = item;
                    msg = $"El Producto {item.P_desc} ya se encuentra cargado. ¿Desea CANCELAR la operación, REMPLAZAR las cantidades existentes o ACUMULAR las cantidades?";
                    return Json(new { error = false, warn = true, msg, p_id = item.P_id });
                }
                else
                {
                    //no esta se carga directamente
                    var lista = RPRProductoRegs;
                    lista.Add(item);
                    RPRProductoRegs = lista;
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
            GridCore<RPRProcuctoDto> datosIP;

            datosIP = ObtenerRPRProdGrid(RPRProductoRegs);

            return PartialView("_rprProductosCargardos", datosIP);
        }


        private GridCore<RPRProcuctoDto> ObtenerRPRProdGrid(List<RPRProcuctoDto> listaRpr)
        {

            var lista = new StaticPagedList<RPRProcuctoDto>(listaRpr, 1, 999, listaRpr.Count);

            return new GridCore<RPRProcuctoDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
        }

        [HttpPost]
        public JsonResult EliminarProducto(string p_id)
        {
            //busco el producto.
            try
            {
                RPRProcuctoDto? item = EliminaProductoBase(p_id);
                return Json(new { error = false, msg = $"El producto {item.P_desc} fue removido de la lista." });
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

        private RPRProcuctoDto EliminaProductoBase(string p_id)
        {
            var item = RPRProductoRegs.SingleOrDefault(p => p.P_id.Equals(p_id));
            if (item == null)
            {
                throw new NegocioException("No se encontró el producto que intenta eliminar de la lista");
            }
            var lista = RPRProductoRegs.Where(p => !p.P_id.Equals(p_id)).ToList();
            RPRProductoRegs = lista;
            return item;
        }

        [HttpPost]
        public JsonResult AcumularProducto()
        {
            try
            {
                //Hubo problemas al intentar acumular el producto con la ya cargada en la lista de Productos seleccionados para el RPR."
                var item = RPRProductoRegs.SingleOrDefault(p => p.P_id.Equals(RPRProductoTemp.P_id));
                if (item == null)
                {
                    throw new Exception(ConstantesGC.MensajeError.RPR_PRODUCTO_NO_ENCONTRADO_ACUMULACION);
                }
                //acumulando
                //se verifica cual es el tipo de unidad que se utiliza
                if (RPRProductoTemp.Up_id.Equals("07"))
                {//son unidades enteras. 
                    if (!RPRProductoTemp.Bulto_up.Equals(item.Bulto_up))
                    {
                        throw new Exception(ConstantesGC.MensajeError.RPR_PRODUCTO_ACUMULACION_UNIDAD_BULTO_DISTINTO);
                    }

                    item.Bulto += RPRProductoTemp.Bulto;
                    item.Cantidad += RPRProductoTemp.Cantidad;
                    item.Uni_suelta += RPRProductoTemp.Uni_suelta;
                }
                else
                { //son unidades decimales. Directamente se suman.
                    item.Bulto_up += RPRProductoTemp.Bulto_up;
                    item.Bulto += RPRProductoTemp.Bulto;
                    item.Cantidad += RPRProductoTemp.Cantidad;
                }
                //Para agregar el acumulado primero debo sacar el producto de la lista
                _ = EliminaProductoBase(RPRProductoTemp.P_id);
                //traigo la lista, lo agrego y lo vuelvo a resguardar
                var lista = RPRProductoRegs;
                lista.Add(item);
                RPRProductoRegs = lista;

                return Json(new { error = false, msg = ConstantesGC.MensajesOK.RPR_PRODUCTO_ACUMULADO });
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
                var lista = RPRProductoRegs.Where(p => !p.P_id.Equals(RPRProductoTemp.P_id)).ToList();
                //Se agrega a lista el producto que esta en temp
                lista.Add(RPRProductoTemp);
                //resguardamos la lista
                RPRProductoRegs = lista;

                return Json(new { error = false, msg = ConstantesGC.MensajesOK.RPR_PRODUCTO_REMPLAZADO });
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

            return View(RPRAutorizacionPendienteSeleccionada);
        }
    }
}
