using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Seguridad;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

namespace gc.pocket.site.Areas.Gestion.Controllers
{
    [Area("Gestion")]
    public class ProductoController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<ProductoController> _logger;
        private readonly ICuentaServicio _ctaSv;
        private readonly IRubroServicio _rubSv;
        private readonly IProductoServicio _productoServicio;
        private readonly BusquedaProducto _busqueda;

        public ProductoController(ILogger<ProductoController> logger, IOptions<MenuSettings> options, IOptions<AppSettings> options1, IOptions<BusquedaProducto> busqueda,
            ICuentaServicio cuentaServicio, IHttpContextAccessor context, IRubroServicio rubSv, IProductoServicio productoServicio) : base(options1, options, context)
        {
            _logger = logger;
            _menuSettings = options.Value;
            _ctaSv = cuentaServicio;
            _rubSv = rubSv;
            _busqueda = busqueda.Value;
            _productoServicio = productoServicio;
        }
        public IActionResult Index(bool actualizar = false)
        {
            try
            {
                if (!EstaAutenticado.Item1)
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
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> BusquedaBase(string busqueda,bool validarEstado=false, bool acumularProductos = false)
        {
            try
            {
                ProductoBusquedaDto producto = new ProductoBusquedaDto { P_id = "0000-0000" };
                if (string.IsNullOrEmpty(busqueda))
                {
                    return Json(new { error = false, producto });
                }

                InicializaVariablesBusquedaBase();

                BusquedaBase buscar = new BusquedaBase
                {
                    Administracion = AdministracionId,
                    Busqueda = busqueda,
                    DescuentoCli = _busqueda.DescuentoCli,
                    ListaPrecio = _busqueda.ListaPrecio,
                    TipoOperacion = _busqueda.TipoOperacion
                };

                producto = await _productoServicio.BusquedaBaseProductos(buscar, TokenCookie);

                if (producto != null && !string.IsNullOrEmpty(producto.P_id))
                {
                    bool warn = false;
                    string msg = string.Empty;
                    //validación de Estado
                    if (!producto.P_activo.Equals("S") && validarEstado)
                    {
                        //se valida que no esta activo. Valores Noactivo Discontinuo
                        return Json(new { error = true, msg = $"El producto {producto.P_desc} se encuentra {producto.Msj}" });
                    }
                    //Validación si pertenece o no al proveedor

                    if (RPRAutorizacionPendienteSeleccionada != null &&
                        !RPRAutorizacionPendienteSeleccionada.Cta_id.Equals(producto.Cta_id) && validarEstado)
                    {
                        warn = true;
                        msg = $"El Producto NO pertenece al actual proveedor. Pertenece al Proveedor {producto.Cta_denominacion}.";
                    }

                    //se resguarda el producto recien buscado.
                    ProductoBase = producto;
                    if (acumularProductos)
                    {
                        var productos = ProductosSeleccionados;
                        productos.Add(producto);
                        ProductosSeleccionados = productos;
                    }
                    return Json(new { error = false, producto, warn, msg, });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = "El producto no ha sido identificado." ,producto=new ProductoBusquedaDto() { P_id="NO" } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hubo un error en la busqueda avanzada");
                return Json(new { error = true, msg = "Algo no salió bien. Vuelva a intentarlo." });
            }
        }

        private void InicializaVariablesBusquedaBase()
        {
            #region Variables de InfoProd
            InfoProdStkDId = "";
            InfoProdStkDRegs = [];
            InfoProdStkBoxesIds = ("", "");
            InfoProdStkBoxesRegs = [];
            InfoProdStkAId = "";
            InfoProdStkARegs = [];
            InfoProdMovStkIds = "";
            InfoProdMovStkRegs = [];
            InfoProdLPId = "";
            InfoProdLPRegs = [];

            #endregion
        }

        [HttpPost]
        public async Task<JsonResult> BusquedaAvanzada(BusquedaProducto search)
        {
            try
            {
                search.SinStock = !search.ConStock;

                List<ProductoListaDto> productos = await _productoServicio.BusquedaListaProductos(search, TokenCookie);

                return Json(new { error = false, lista = productos });
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda Avanzada";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda Avanzada");

                return Json(new { error = true, msg });
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


    }
}
