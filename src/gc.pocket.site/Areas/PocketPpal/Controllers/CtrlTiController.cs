using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("pocketppal")]
    public class CtrlTiController : ControladorBase
    {
        private readonly AppSettings _settings;
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<CtrlTiController> _logger;

        public CtrlTiController(IOptions<AppSettings> options, IHttpContextAccessor context,IOptions<MenuSettings> options1,
            ILogger<CtrlTiController> logger) : base(options, context)
        {
            _menuSettings = options1.Value;
            _settings = options.Value;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            ListadoTIAutoPendientes = new();
            TIActual = new();
            TI_ModId = string.Empty;

            string? volver = Url.Action("index", "home", new { area = ""});
            ViewBag.AppItem = new AppItem { Nombre = "Ctrl Salida Transferencia", VolverUrl = volver ?? "#" };

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CtrlTI_AU(string titId)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }
                string? volver = Url.Action("index", "ctrlti", new { area = "pocketppal" });

                switch (titId)
                {
                    case "S":
                        ViewBag.AppItem = new AppItem { Nombre = "Ctrl Salida Transferencia entre Sucursales", VolverUrl = volver ?? "#" };
                        break;
                    case "D":
                        ViewBag.AppItem = new AppItem { Nombre = "Ctrl Salida Transferencia entre Depositos", VolverUrl = volver ?? "#" };
                        break;
                }
                TI_ModId = titId;
                var ti = TIActual;
                ti.TipoTI = TI_ModId;
                ti.SinAU = TI_CS;
                TIActual = ti;

                return View(TIActual);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error carga de Vista CTRLTI_AU");
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CtrlTI_Colector()
        {
            try
            {
                string? volver = Url.Action("index", "home", new { area = "" });
                ViewBag.AppItem = new AppItem { Nombre = "Ctrl Salida Transferencia", VolverUrl = volver ?? "#" };

                return View(TIActual);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error carga de vista CtrlTi_colector");
                return RedirectToAction("Index");
            }
           

        }


        #region Métodos POST
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
                var cantidad = ProductoBase.Up_id.Equals("07") ? (up * bulto) + unidad : bulto;

                if (cantidad <= 0)
                {
                    return Json(new { error = true, msg = "La cantidad dió como resultado 0 (cero). Verifique." });
                }

                //armo producto a resguardar
                var item = new ProcuctoGenDto();
                item.Ti = TIActual.Ti;

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
                //if (string.IsNullOrEmpty(vto) && string.IsNullOrWhiteSpace(vto))
                //{
                //    item.vto = null;
                //}
                //else
                //{
                //    var f = vto.Split('-', StringSplitOptions.RemoveEmptyEntries);
                //    item.vto = new DateTime(f[0].ToInt(), f[1].ToInt(), f[2].ToInt());
                //}
                item.cantidad = cantidad;

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
            GridCore<ProcuctoGenDto> datosIP;

            datosIP = ObtenerRPRProdGrid(RPRProductoRegs);

            return PartialView("_productosCargardos", datosIP);
        }


        private GridCore<ProcuctoGenDto> ObtenerRPRProdGrid(List<ProcuctoGenDto> listaRpr)
        {

            var lista = new StaticPagedList<ProcuctoGenDto>(listaRpr, 1, 999, listaRpr.Count);

            return new GridCore<ProcuctoGenDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
        }

        [HttpPost]
        public JsonResult EliminarProducto(string p_id)
        {
            //busco el producto.
            try
            {
                ProcuctoGenDto? item = EliminaProductoBase(p_id);
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

        private ProcuctoGenDto EliminaProductoBase(string p_id)
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

        #endregion
    }
}
