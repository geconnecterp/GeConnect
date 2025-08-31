using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Productos.Actualiza;
using gc.sitio.core.Servicios.Contratos.Importacion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using X.PagedList;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class ActualizarPPController : ControladorActualizarPPBase
    {
        private readonly IImportarServicio _importarServicio;

        public ActualizarPPController(IOptions<AppSettings> options,
           IHttpContextAccessor contexto, ILogger<ActualizarPPController> logger,
           IImportarServicio importarServicio) :
            base(options, contexto, logger)
        {
            _importarServicio = importarServicio;
        }

        /// <summary>
        /// Retorna la vista principal con los tabs
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Titulo"] = "Actualizar Precios de Proveedores";
            return View();
        }

        /// <summary>
        /// Carga la vista parcial con el grid de proveedores
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CargarProveedores()
        {
            try
            {
                var respuesta = await _importarServicio.ObtenerProveedoresConProductosParaActualizar(TokenCookie);

                if (!respuesta.Ok)
                {
                    return PartialView("_gridMensaje", CrearRespuestaError(respuesta.Mensaje ?? "Error al obtener proveedores"));
                }

                if (respuesta?.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    return PartialView("_gridMensaje", CrearRespuestaError("No se encontraron proveedores con productos para actualizar"));
                }

                ProvedoresParaActualizar = respuesta.ListaEntidad;
                var grid = GenerarGridProveedores(respuesta.ListaEntidad);

                return PartialView("_gridActuProveedor", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error de negocio al obtener proveedores para actualizar");
                return PartialView("_gridMensaje", CrearRespuestaError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inesperado al obtener proveedores para actualizar");
                return PartialView("_gridMensaje", CrearRespuestaError("Error al obtener el listado de proveedores"));
            }
        }

        /// <summary>
        /// Obtiene los productos de un proveedor específico y retorna vista parcial
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ObtenerProductosProveedor(string ctaId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ctaId))
                {
                    return PartialView("_gridMensaje", CrearRespuestaError("ID de proveedor requerido"));
                }

                var filters = new QueryFilters { Id = ctaId };
                var respuesta = await _importarServicio.ObtenerProductosDelProveedorParaActualizar(filters, TokenCookie);

                if (!respuesta.Ok || respuesta.EsError)
                {
                    return PartialView("_gridMensaje", CrearRespuestaError(respuesta.Mensaje ?? "Error al obtener productos"));
                }

                if (respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    return PartialView("_gridMensaje", CrearRespuestaError("No se encontraron productos para este proveedor"));
                }

                var grid = GenerarGridProductos(respuesta.ListaEntidad);
                return PartialView("_gridActuProducto", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener productos para el proveedor {CtaId}", ctaId);
                return PartialView("_gridMensaje", CrearRespuestaError("Error interno al obtener productos"));
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarProveedores(string[] ctasId)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                if (ctasId.Length == 0)
                {
                    throw new NegocioException("Para confirmar es necesario especificar al menos una cuenta de proveedor.");
                }

                var proveedores = ProvedoresParaActualizar.Where(p => ctasId.Contains(p.cta_id)).ToList();

                //prod.P_Obs = prod.P_Obs.ToUpper();
                AbmGenDto abm = new AbmGenDto()
                {
                    Json = JsonConvert.SerializeObject(proveedores),
                    Objeto = "Cuentas",
                    Administracion = AdministracionId,
                    Usuario = UserName,
                    Abm = 'C'
                };

                var res = await _importarServicio.ConfirmarActualizacionPrecioProductosDeProveedor(abm, TokenCookie);
                if (res.Ok)
                {
                    if (res.Entidad.resultado == 0)
                    {
                        string msg;

                        msg = $"EL PROCESAMIENTO de 9 SE REALIZO SATISFACTORIAMENTE";

                        ProvedoresParaActualizar = [];

                        return Json(new { error = false, warn = false, msg });
                    }
                    else
                    {
                        throw new NegocioException(res.Entidad.resultado_msj);
                    }
                   
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.Entidad.resultado_msj, focus = res.Entidad.resultado_setfocus });
                }
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


        #region Métodos Privados



        /// <summary>
        /// Genera el grid optimizado para productos usando GridCoreSmart
        /// </summary>
        private GridCoreSmart<ProductoDetalleDto> GenerarGridProductos(List<ProductoDetalleDto> productos)
        {
            var productosOrdenados = productos
                .OrderBy(p => p.p_id)
                .ToList();

            var pagedList = new StaticPagedList<ProductoDetalleDto>(
                productosOrdenados,
                1,
                productos.Count,
                productos.Count
            );

            return new GridCoreSmart<ProductoDetalleDto>
            {
                ListaDatos = pagedList,
                CantidadReg = productos.Count,
                PrimerRegistro = 1,
                UltimoRegistro = productos.Count,
                RegistroFinal = productos.Count,
                CantidadPaginas = 1,
                PaginaActual = 1,
                Sort = "p_id",
                SortDir = "ASC",
                DatoAux01 = $"Productos cargados: {DateTime.Now:HH:mm:ss}"
            };
        }

        /// <summary>
        /// Genera el grid optimizado para proveedores usando GridCoreSmart
        /// </summary>
        private GridCoreSmart<ActualizaProveedorDto> GenerarGridProveedores(List<ActualizaProveedorDto> proveedores)
        {
            var proveedoresOrdenados = proveedores
                .OrderBy(p => p.cta_denominacion)
                .ToList();

            var pagedList = new StaticPagedList<ActualizaProveedorDto>(
                proveedoresOrdenados,
                1,
                proveedores.Count,
                proveedores.Count
            );

            return new GridCoreSmart<ActualizaProveedorDto>
            {
                ListaDatos = pagedList,
                CantidadReg = proveedores.Count,
                PrimerRegistro = 1,
                UltimoRegistro = proveedores.Count,
                RegistroFinal = proveedores.Count,
                CantidadPaginas = 1,
                PaginaActual = 1,
                Sort = "cta_denominacion",
                SortDir = "ASC",
                DatoAux01 = $"Proveedores cargados: {DateTime.Now:HH:mm:ss}"
            };
        }

        /// <summary>
        /// Crea una respuesta de error estandarizada
        /// </summary>
        private RespuestaGenerica<EntidadBase> CrearRespuestaError(string mensaje)
        {
            return new RespuestaGenerica<EntidadBase>
            {
                Mensaje = mensaje,
                Ok = false,
                EsWarn = false,
                EsError = true
            };
        }

        #endregion
    }

    
}