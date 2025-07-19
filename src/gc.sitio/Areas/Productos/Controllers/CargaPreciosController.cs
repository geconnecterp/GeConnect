using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class CargaPreciosController : ControladorProductoBase
    {
        private readonly AppSettings _appSettings;
        private readonly IAdministracionServicio _administracionServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IRubroServicio _rubroServicio;
        private readonly IProducto2Servicio _productoServicio;


        public CargaPreciosController(
            ICuentaServicio cuentaServicio,
            IRubroServicio rubroServicio,
            IProducto2Servicio productoServicio,
            IAdministracionServicio administracionServicio,
            ILogger<CompraController> logger,
            IOptions<AppSettings> options,
            IHttpContextAccessor context) : base(options, context, logger)
        {
            _administracionServicio = administracionServicio;
            _cuentaServicio = cuentaServicio;
            _rubroServicio = rubroServicio;
            _productoServicio = productoServicio;
            _appSettings = options.Value;
        }

        public IActionResult Index()
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                var listR01 = new List<ComboGenDto>();
                ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

                var listR02 = new List<ComboGenDto>();
                ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

                var listR03 = new List<ComboGenDto>();
                ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);
                ViewBag.Rel03 = HelperMvc<ComboGenDto>.ListaGenerica(listR03);

                string titulo = "Productos - Carga de Precios";
                ViewData["Titulo"] = titulo;
                CargarDatosIniciales(true);

                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cargar la vista de BSS");
                TempData["error"] = "Hubo un problema al cargar la vista del BSS. Si el problema persiste, contacte al administrador.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerProductosDetalle(string buscar,
            string id, string id2,
            string ctaId, List<ComboGenDto> familias,
            List<string> rubros,
            bool disc = true, bool file = false)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Validar que el parámetro eje_nro no sea nulo o vacío
                if (string.IsNullOrEmpty(ctaId))
                {
                    return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                    {
                        Ok = false,
                        Mensaje = "Debe indicar el proveedor."
                    });
                }

                // Llamar al servicio para obtener los asientos de ajuste
                var filtro = new QueryFilters
                {
                    Buscar = buscar,
                    Id = id,
                    Id2 = id2,
                    Rel01 = new List<string> { ctaId },
                    Rel02 = rubros,
                    Rel03 = familias,
                    Opt1 = disc,
                    Opt2 = file
                };
                var respuesta = await _productoServicio.Obtener_ProductoDetalle(filtro, TokenCookie);

                if (!respuesta.Ok || respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    response.Mensaje = "No se encontraron productos para la configuración del filtro que especificó.";
                    response.Ok = false;
                    response.EsWarn = true;
                    response.EsError = false;
                    return PartialView("_gridMensaje", response);
                }
                var lista = respuesta.ListaEntidad.OrderBy(x => x.pg_id).ThenBy(x => x.p_id).ToList();
                // Guardar datos en variable de sesión para uso posterior
                ProductosDetalle = lista;


                // Crear el grid para la vista (sin paginación)
                var grid = GenerarGrillaSmart(
                    lista,
                    "pg_id",  // Ordenamiento por defecto
                    respuesta.ListaEntidad.Count,  // Todos los registros en una página
                    1,  // Página única
                    respuesta.ListaEntidad.Count,  // Total de registros
                    1,  // Total de páginas (una sola)
                    "ASC"  // Dirección de ordenamiento por defecto
                );

                // Devolver la vista parcial con el grid
                return PartialView("_gridProdDet", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener el detalle de productos");

                response.Mensaje = "Error al obtener el detalle de productos.";
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerProductosDetalleLista(string buscar,
            string id, string id2,
            string ctaId, List<ComboGenDto> familias,
            List<string> rubros,
            bool disc = true, bool file = false)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Validar que el parámetro eje_nro no sea nulo o vacío
                if (string.IsNullOrEmpty(ctaId))
                {
                    return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                    {
                        Ok = false,
                        Mensaje = "Debe indicar el proveedor."
                    });
                }

                // Llamar al servicio para obtener los asientos de ajuste
                var filtro = new QueryFilters
                {
                    Buscar = buscar,
                    Id = id,
                    Id2 = id2,
                    Rel01 = new List<string> { ctaId },
                    Rel02 = rubros,
                    Rel03 = familias,
                    Opt1 = disc,
                    Opt2 = file
                };
                var respuesta = await _productoServicio.Obtener_ProductoDetalleListas(filtro, TokenCookie);

                if (!respuesta.Ok || respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    response.Mensaje = "No se encontraron las listas del producto especificado.";
                    response.Ok = false;
                    response.EsWarn = true;
                    response.EsError = false;
                    return PartialView("_gridMensaje", response);
                }
                var lista = respuesta.ListaEntidad.OrderBy(x => x.pg_id).ThenBy(x => x.p_id).ToList();
                // Guardar datos en variable de sesión para uso posterior
                ProductosDetalle = lista;


                // Crear el grid para la vista (sin paginación)
                var grid = GenerarGrillaSmart(
                    lista,
                    "pg_id",  // Ordenamiento por defecto
                    respuesta.ListaEntidad.Count,  // Todos los registros en una página
                    1,  // Página única
                    respuesta.ListaEntidad.Count,  // Total de registros
                    1,  // Total de páginas (una sola)
                    "ASC"  // Dirección de ordenamiento por defecto
                );

                // Devolver la vista parcial con el grid
                return PartialView("_gridProdLista", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener el detalle de listas");

                response.Mensaje = "Error al obtener el detalle de listas.";
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public JsonResult CalcularCosto(decimal tp_plista, decimal tp_dto1, decimal tp_dto2,
            decimal tp_dto3, decimal tp_dto4, decimal tp_dto_pa, decimal tp_porc_flete, string tp_boni)
        {
            try
            {
                // Verificar autenticación
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                if (tp_plista <= 0)
                {
                    throw new NegocioException("El valor del Producto es incorrecto. Por favor verifique.");
                }

                var costo = HelperContable.CalcularPCosto(tp_plista, tp_dto1, tp_dto2,
                    tp_dto3, tp_dto4, tp_dto_pa, tp_boni, tp_porc_flete);

                return Json(new { error = false, warn = false, costo });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        //Invocar cuando se haya seleccionado solo un proveedor desde el filtro base.
        [HttpPost]
        public JsonResult BuscarFamiliaDesdeProveedorSeleccionado(string ctaId)
        {
            try
            {
                CargarProveedoresFamiliaLista(ctaId, _cuentaServicio);
                var familias = ProveedorFamiliaLista;
                var combo = familias.Select(x => new ComboGenDto { Id = x.pg_id, Descripcion = x.pg_desc })
                    .ToList();
                return Json(new { error = false, warn = false, lista = combo });
            }
            catch (Exception)
            {
                return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar obtener los datos de la familia de productos del proveedor: {ctaId}" });
            }

        }

        protected void CargarProveedoresFamiliaLista(string ctaId, ICuentaServicio _cuentaServicio, string? fam = null)
        {
            var adms = _cuentaServicio.ObtenerListaProveedoresFamilia(ctaId, TokenCookie);
            ProveedorFamiliaLista = adms;
        }

        private SelectList ComboProveedores()
        {
            var adms = _cuentaServicio.ObtenerListaProveedores("BI", TokenCookie);
            var lista = adms.Select(x => new ComboGenDto { Id = x.Cta_Id, Descripcion = x.Cta_Denominacion });
            return HelperMvc<ComboGenDto>.ListaGenerica(lista);
        }

        private SelectList ComboRubros()
        {
            var adms = _rubroServicio.ObtenerListaRubros("", TokenCookie);
            var lista = adms.Select(x => new ComboGenDto { Id = x.Rub_Id, Descripcion = x.Rub_Desc });
            return HelperMvc<ComboGenDto>.ListaGenerica(lista);
        }

        private SelectList ComboSucursales()
        {
            var adms = _administracionServicio.GetAdministracionLogin();
            var lista = adms.Select(x => new ComboGenDto { Id = x.Id, Descripcion = x.Descripcion });
            return HelperMvc<ComboGenDto>.ListaGenerica(lista);
        }
        private void CargarDatosIniciales(bool actualizar)
        {
            if (ProveedoresLista.Count == 0 || actualizar)
            {
                ObtenerProveedores(_cuentaServicio, "BI");
            }

            if (RubroLista.Count == 0 || actualizar)
            {
                ObtenerRubros(_rubroServicio);
            }
        }
    }
}
