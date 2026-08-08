using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Seguridad;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.pocket.site.Areas.ABMs.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.pocket.site.Areas.Gestion.Controllers
{
    [Area("Gestion")]
    public class ProductoController : ProductoControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<ProductoController> _logger;
        private readonly ICuentaServicio _ctaSv;
        private readonly IRubroServicio _rubSv;
        private readonly IProductoServicio _productoServicio;
        private readonly IRemitoServicio _remitoSv;
        private readonly BusquedaProducto _busqueda;

        public ProductoController(ILogger<ProductoController> logger, IOptions<MenuSettings> options, IOptions<AppSettings> options1, IOptions<BusquedaProducto> busqueda,
            ICuentaServicio cuentaServicio, IHttpContextAccessor context, IRubroServicio rubSv, IProductoServicio productoServicio, IRemitoServicio remitoServicio) : base(options1, context, logger)
        {
            _logger = logger;
            _menuSettings = options.Value;
            _ctaSv = cuentaServicio;
            _rubSv = rubSv;
            _busqueda = busqueda.Value;
            _productoServicio = productoServicio;
            _remitoSv = remitoServicio;
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
                    ObtenerProveedores(_ctaSv);
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
        public async Task<JsonResult> BusquedaBase(string busqueda, bool validarEstado = false, bool acumularProductos = false, string modulo = "GENERAL")
        {
            try
            {                
                ProductoBusquedaDto producto = new ProductoBusquedaDto { P_id = "0000-0000" };
                if (string.IsNullOrEmpty(busqueda))
                {
                    return Json(new { error = false, producto });
                }

                InicializaVariablesBusquedaBase();
                var moduloNormalizado = string.IsNullOrWhiteSpace(modulo)
                    ? "GENERAL"
                    : modulo.Trim().ToUpperInvariant();

                _logger.LogInformation("Búsqueda base de producto. Búsqueda: {Busqueda}; módulo: {Modulo}; validar estado: {ValidarEstado}",
                    busqueda, moduloNormalizado, validarEstado);

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
                    _logger.LogInformation("Producto encontrado en búsqueda base. Producto: {Producto}; módulo: {Modulo}; estado: {Estado}",
                        producto.P_id, moduloNormalizado, producto.P_activo);

                    bool warn = false;
                    string msg = string.Empty;
                    //validación de Estado
                    if (!producto.P_activo.Equals("S") && validarEstado)
                    {
                        //se valida que no esta activo. Valores Noactivo Discontinuo
                        return Json(new { error = true, msg = $"El producto {producto.P_desc} se encuentra {producto.Msj}" });
                    }
                    // Las reglas de pertenencia dependen del módulo. No deben inferirse
                    // a partir de validarEstado porque son controles de negocio distintos.
                    if (moduloNormalizado.Equals("RTI"))
                    {
                        _logger.LogInformation("Validando pertenencia del producto {Producto} al remito RTI {Remito}",
                            producto.P_id, RemitoActual.re_compte);

                        var resp = await _remitoSv.VerificaProductoEnRemito(rm: RemitoActual.re_compte, pId: producto.P_id, TokenCookie);
                        if (resp.resultado != 0)
                        {
                            _logger.LogWarning("Producto rechazado por validación RTI. Producto: {Producto}; remito: {Remito}; mensaje: {Mensaje}",
                                producto.P_id, RemitoActual.re_compte, resp.resultado_msj);
                            return Json(new { error = true, msg = resp.resultado_msj });
                        }
                    }
                    else if (moduloNormalizado.Equals("RPR"))
                    {
                        var autorizacionRpr = AutorizacionPendienteSeleccionada;
                        if (string.IsNullOrWhiteSpace(autorizacionRpr.Cta_id))
                        {
                            _logger.LogWarning("No se pudo validar proveedor RPR porque no existe una autorización seleccionada. Producto: {Producto}",
                                producto.P_id);
                            return Json(new { error = true, msg = "No se pudo determinar la autorización RPR actual. Reingrese al módulo." });
                        }

                        if (!string.Equals(autorizacionRpr.Cta_id, producto.Cta_id, StringComparison.OrdinalIgnoreCase))
                        {
                            warn = true;
                            msg = $"El Producto NO pertenece al actual proveedor. Pertenece al Proveedor {producto.Cta_denominacion}.";
                            _logger.LogWarning("Producto de otro proveedor en RPR. Producto: {Producto}; proveedor RPR: {ProveedorRpr}; proveedor producto: {ProveedorProducto}",
                                producto.P_id, autorizacionRpr.Cta_id, producto.Cta_id);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("El módulo {Modulo} no requiere validación de proveedor o remito para el producto {Producto}",
                            moduloNormalizado, producto.P_id);
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
