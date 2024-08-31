using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.pocket.site.Controllers;
using gc.pocket.site.Models.ViewModels;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;
using X.PagedList;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class RPRController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<InfoProdController> _logger;
        private readonly IProductoServicio _productoServicio;
        private readonly IDepositoServicio _depositoServicio;
        private readonly AppSettings _settings;

        public RPRController(IOptions<AppSettings> options, IHttpContextAccessor context, IOptions<MenuSettings> options1,
            ILogger<InfoProdController> logger, IProductoServicio productoServicio, IDepositoServicio depositoServicio) : base(options, context)
        {
            _menuSettings = options1.Value;
            _logger = logger;
            _productoServicio = productoServicio;
            _depositoServicio = depositoServicio;
            _settings = options.Value;
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
                //validaciones de parametros
                if (up < 1) { 
                return Json(new { error = true, msg = "Las unidades del bulto no puede ser 0 (cero) o tener valores negativos. Verifique, por favor." });
                }
                if (bulto < 1)
                {
                    return Json(new { error = true, msg = "Los bultos no puede ser 0 (cero) o tener valores negativos. Verifique, por favor." });
                }

                if (unidad < 0)
                {
                    return Json(new { error = true, msg = "Las unidades sueltas no puede tener valores negativos. Verifique, por favor." });
                }
                if (!string.IsNullOrEmpty(vto) && !string.IsNullOrWhiteSpace(vto))
                {
                    var fecha = vto.ToDateTimeOrNull();
                    var tope = DateTime.Today.AddDays(_settings.FechaVtoCota);

                    if ( fecha == null)
                    {
                        return Json(new { error = true, msg = "La fecha recepcionada no es válida. Verifique." });
                    }
                    else if(fecha<tope)
                    {
                        return Json(new { error = true, msg = $"La fecha recepcionada no puede ser menor a {tope}. Verifique, por favor." });
                    }
                }


                //armo producto a resguardar
                var item = new RPRProcuctoDto();
                item.rp = RPRAutorizacionPendienteSeleccionada.Rp;
                item.item = RPRProductoRegs.Count + 1;
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
                item.cantidad = ProductoBase.Up_id.Equals("07") ? (up * bulto) + unidad : bulto;

                var res = RPRProductoRegs.Any(x => x.p_id.Equals(item.p_id));

                if (res)
                {
                    //ya se encuentra cargado el producto, se debe avisar.
                    RPRProductoTemp = item;
                    msg = $"El Producto {item.p_desc} ya se encuentra cargado. ¿Desea CANCELAR la operación, REMPLAZAR las cantidades existentes o ACUMULAR las cantidades?";
                    return Json(new { error = false, warn = true, msg, p_id = item.p_id });
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

        private RPRProcuctoDto EliminaProductoBase(string p_id)
        {
            var item = RPRProductoRegs.SingleOrDefault(p => p.p_id.Equals(p_id));
            if (item == null)
            {
                throw new NegocioException("No se encontró el producto que intenta eliminar de la lista");
            }
            var lista = RPRProductoRegs.Where(p => !p.p_id.Equals(p_id)).ToList();
            RPRProductoRegs = lista;
            return item;
        }

        [HttpPost]
        public JsonResult AcumularProducto()
        {
            try
            {
                //Hubo problemas al intentar acumular el producto con la ya cargada en la lista de Productos seleccionados para el RPR."
                var item = RPRProductoRegs.SingleOrDefault(p => p.p_id.Equals(RPRProductoTemp.p_id));
                if (item == null)
                {
                    throw new Exception(infraestructura.Constantes.Constantes.MensajeError.RPR_PRODUCTO_NO_ENCONTRADO_ACUMULACION);
                }
                //acumulando
                //se verifica cual es el tipo de unidad que se utiliza
                if (RPRProductoTemp.up_id.Equals("07"))
                {//son unidades enteras. 
                    if (!RPRProductoTemp.unidad_pres.Equals(item.unidad_pres))
                    {
                        throw new Exception(infraestructura.Constantes.Constantes.MensajeError.RPR_PRODUCTO_ACUMULACION_UNIDAD_BULTO_DISTINTO);
                    }

                    item.bulto += RPRProductoTemp.bulto;
                    item.cantidad += RPRProductoTemp.cantidad;
                    item.us += RPRProductoTemp.us;
                }
                else
                { //son unidades decimales. Directamente se suman.
                    item.unidad_pres += RPRProductoTemp.unidad_pres;
                    item.bulto += RPRProductoTemp.bulto;
                    item.cantidad += RPRProductoTemp.cantidad;
                }
                //Para agregar el acumulado primero debo sacar el producto de la lista
                _ = EliminaProductoBase(RPRProductoTemp.p_id);
                //traigo la lista, lo agrego y lo vuelvo a resguardar
                var lista = RPRProductoRegs;
                lista.Add(item);
                RPRProductoRegs = lista;

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
                var lista = RPRProductoRegs.Where(p => !p.p_id.Equals(RPRProductoTemp.p_id)).ToList();
                //Se agrega a lista el producto que esta en temp
                lista.Add(RPRProductoTemp);
                //resguardamos la lista
                RPRProductoRegs = lista;

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
                var analisis = ul.Trim().Split('-', StringSplitOptions.RemoveEmptyEntries);
                if(analisis.Length != 2)
                {
                    throw new NegocioException("Verifique la Unidad de Lectura. Algo no esta bien.");
                }
                if (analisis[0].Length!= 5)
                {
                    throw new NegocioException("Verifique la Unidad de Lectura. Algo no esta bien.");
                }
                if (analisis[1].Length != 10)
                {
                    throw new NegocioException("Verifique la Unidad de Lectura. Algo no esta bien.");
                }

                var res = await _productoServicio.RPRRegistrarProductos(RPRProductoRegs, AdministracionId, ul, TokenCookie);

                if (res.Resultado==0)
                {
                    return Json(new { error = false, warn = false, msg = $"La Carga del {ul} fue satisfactoria" });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.Resultado_msj });
                }
            }
            catch(NegocioException ex)
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
            ViewBag.AppItem = new AppItem { Nombre = $"Auto:{RPRAutorizacionPendienteSeleccionada.Rp}-{RPRAutorizacionPendienteSeleccionada.Cta_denominacion}" };
            return View(RPRAutorizacionPendienteSeleccionada);
        }

        private void ComboDepositos()
        {
            var adms = _depositoServicio.ObtenerDepositosDeAdministracion(AdministracionId, TokenCookie);
            var lista = adms.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
            ViewBag.DepoId = HelperMvc<ComboGenDto>.ListaGenerica(lista);
        }

        [HttpGet]
        public IActionResult AlmacenajeBox()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }



            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ValidarUl(string ul)
        {
            try
            {
                if(string.IsNullOrEmpty(ul) || string.IsNullOrWhiteSpace(ul))
                {
                    throw new NegocioException("No se recepcionó la UL. Verifique");
                }
                var res = await _productoServicio.ValidarUL(ul,AdministracionId,TokenCookie);
                if (res.Resultado == 0)
                {
                    return Json(new { error = false, warn = false, msg = "La validación del UL fue exitosa." });
                }
                else
                {
                    throw new NegocioException(res.Resultado_msj);
                }                
            }
            catch (NegocioException ex)
            {
                return Json(new {error=false,warn=true,msg =ex.Message});
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
                    return Json(new { error = false, warn = false, msg = "La validación del Box fue exitosa." });
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
        public async Task<JsonResult> ConfirmaBoxUl(string box,string ul)
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
                var res = await _productoServicio.ConfirmaBoxUl(box,ul, AdministracionId, TokenCookie);
                if (res.Resultado == 0)
                {
                    return Json(new { error = false, warn = false, msg = "La validación del Box fue exitosa." });
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
