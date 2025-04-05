using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Seguridad;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.ABMs.Controllers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.ControlComun.Controllers
{
    [Area("ControlComun")]
    public class ProductoController : ProductoControladorBase
	{
        private readonly ILogger<ProductoController> _logger;
        private readonly ICuentaServicio _ctaSv;
        private readonly IRubroServicio _rubSv;
        private readonly IProductoServicio _productoServicio;
        private readonly BusquedaProducto _busqueda;

        public ProductoController(ILogger<ProductoController> logger, IOptions<MenuSettings> options, IOptions<AppSettings> options1, IOptions<BusquedaProducto> busqueda,
            ICuentaServicio cuentaServicio, IHttpContextAccessor context, IRubroServicio rubSv, IProductoServicio productoServicio) : base(options1, context, logger)
        {
            _logger = logger;
            _ctaSv = cuentaServicio;
            _rubSv = rubSv;
            _busqueda = busqueda.Value;
            _productoServicio = productoServicio;

			if (ProveedoresLista.Count == 0)
			{
				ObtenerProveedores(_ctaSv, "BI");
			}

			if (RubroLista.Count == 0)
			{
				ObtenerRubros(_rubSv);
			}
		}
        public IActionResult Index(bool actualizar = false)
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
					ObtenerProveedores(_ctaSv, "BI");
				}

				if (RubroLista.Count == 0 || actualizar)
				{
					ObtenerRubros(_rubSv);
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
        public async Task<JsonResult> BusquedaBase(string busqueda, bool validarEstado = false, bool acumularProductos = false, bool validarPertenenciaDeProveedor = true)
        {
            try
            {
                ProductoBusquedaDto producto = new ProductoBusquedaDto { P_id = "0000-0000" };
                if (string.IsNullOrEmpty(busqueda))
                {
                    producto.P_id = "NO";
					return Json(new { error = false, warn = true, producto });
                }

                if (busqueda.Trim().Length < 6)
                {
                    busqueda = busqueda.Trim().PadLeft(6, '0');
                }
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

                    if (validarPertenenciaDeProveedor)
                    {
						if (CuentaComercialSeleccionada != null &&
						!CuentaComercialSeleccionada.Cta_Id.Equals(producto.Cta_id) && validarEstado)
						{
							warn = true;
							msg = $"El Producto NO pertenece al actual proveedor. Pertenece al Proveedor {producto.Cta_denominacion}.";
						}
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
                    return Json(new { error = false, warn = true, msg = "El producto no ha sido identificado.", producto = new ProductoBusquedaDto() { P_id = "NO" } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hubo un error en la busqueda avanzada");
                return Json(new { error = true, msg = "Algo no salió bien. Vuelva a intentarlo." });
            }
        }

		//private void ObtenerRubros()
		//{
		//    RubroLista = _rubSv.ObtenerListaRubros(TokenCookie);
		//}

		//private void ObtenerProveedores()
		//{
		//    //se guardan los proveedores en session. Para ser utilizados posteriormente

		//    ProveedoresLista = _ctaSv.ObtenerListaProveedores(TokenCookie);
		//}
		[HttpPost]
		public async Task<IActionResult> BusquedaAvanzada(string ri01, string ri02, bool act, bool dis, bool ina, bool cstk, bool sstk, string buscar, bool buscaNew, string sort = "p_id", string sortDir = "asc", int pag = 1)
        {
            return await BusquedaAvanzada(ri01, ri02, act, dis, ina, cstk, sstk, buscar, buscaNew, _productoServicio, sort, sortDir, pag);
        }

    }
}
