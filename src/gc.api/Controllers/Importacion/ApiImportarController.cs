using gc.api.Controllers.Almacen;
using gc.api.core.Contratos.Servicios.Importacion;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Importacion;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Productos.Actualiza;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace gc.api.Controllers.Importacion
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiImportarController : ControllerBase
    {
        private readonly ILogger<ApiImportarController> _logger;
        private readonly IApiImportarServicio _importarServicio;
        public ApiImportarController(ILogger<ApiImportarController> logger, IApiImportarServicio importarServicio)
        {
            _logger = logger;
            _importarServicio = importarServicio;
        }

        #region Metodos de IMPORTACIÓN


        [HttpGet("precio-file-dato")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<PrecioFileDatos>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ObtenerPrecioFileDatos()
        {
            
            var resultado = _importarServicio.ObtenerPrecioFileDatos();

            if (resultado == null)
            {
                return BadRequest("No se pudo obtener el listado de los datos de referencia para la importación de listas de precio. Verifique los datos ingresados.");
            }

            return Ok(new ApiResponse<List<PrecioFileDatos>>(resultado));
        }

        [HttpGet("precio-file-perfil")]
        public IActionResult ObtenerPerfilDeProveedor(string ctaId)
        {
            if(string.IsNullOrEmpty(ctaId))
            {
                return BadRequest("El ID de cliente no puede estar vacío.");
            }
           
            List<MapeoColumnaDto> resultado = _importarServicio.ObtenerPerfilDeProveedor(ctaId);

            return Ok(new ApiResponse<List<MapeoColumnaDto>>(resultado));
        }

        [HttpPost("cargar-perfil-precio")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RespuestaCPDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult CargarImportacionPrecioPerfil(AbmPlusGenDto req)
        {
            RespuestaDto respPerfil = new();
            var perfilSolicitado = !string.IsNullOrWhiteSpace(req?.Json2);
            var perfilGuardado = false;
            var mensajePerfil = perfilSolicitado
                ? "No se pudo guardar el formato de importación."
                : "La importación se procesó sin modificar el formato guardado.";

            if (req == null || 
                string.IsNullOrEmpty(req.Objeto) || 
                string.IsNullOrEmpty(req.Usuario) ||
                string.IsNullOrEmpty(req.Administracion) || 
                string.IsNullOrEmpty(req.Json))
            {
                return BadRequest("Los datos del perfil de precios son inválidos.");
            }

            // Si falla el perfil, la carga de precios debe continuar y devolver una
            // advertencia específica al sitio.
            if (perfilSolicitado)
            {
                try
                {
                    respPerfil = _importarServicio.CargaPerfilCuenta(ctaId: req.Objeto, usu: req.Usuario, adm: req.Administracion, json: req.Json2);
                    perfilGuardado = respPerfil.resultado == 0;
                    mensajePerfil = perfilGuardado
                        ? "El formato de importación se guardó correctamente."
                        : respPerfil.resultado_msj;
                    _logger.LogInformation("Resultado de carga del perfil para {CtaId}: {Resultado}", req.Objeto, respPerfil.resultado);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Problemas para la carga del perfil");
                    mensajePerfil = "La importación continuó, pero no se pudo guardar el formato.";
                }
            }

            List<RespuestaCPDto> resultado = _importarServicio.CargarImportacionPrecioPerfil(req);

            foreach (var registro in resultado)
            {
                registro.perfil_solicitado = perfilSolicitado;
                registro.perfil_guardado = perfilGuardado;
                registro.perfil_msj = mensajePerfil;
            }

            
            return Ok(new ApiResponse<List<RespuestaCPDto>>(resultado));
        }
        #endregion

        #region METODOS DE ACTUALIZACION

        [HttpGet("proveedores-actualizar")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<ActualizaProveedorDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ObtenerProveedoresConProductosParaActualizar()
        {

            var resultado = _importarServicio.ObtenerProveedoresConProductosParaActualizar();

            if (resultado == null)
            {
                return BadRequest("No se pudo obtener el listado de los datos de referencia para la importación de listas de precio. Verifique los datos ingresados.");
            }

            return Ok(new ApiResponse<List<ActualizaProveedorDto>>(resultado));
        }

        [HttpPost("productos-actualizar")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<ProductoDetalleDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ObtenerProductosDelProveedorParaActualizar(QueryFilters filters)
        {
            var reg = new ProductoDetalleDto { Total_Paginas = 0, Total_Registros = 0 };
            var lista = _importarServicio.ObtenerProductosDelProveedorParaActualizar(filters);

            //if (lista == null)
            //{
            //    return BadRequest("No se pudo obtener el listado de los datos de referencia para la importación de listas de precio. Verifique los datos ingresados.");
            //}

            if (lista.Count > 0)
            {
                reg = lista.First();
            }
            else
            {
                return NotFound("No se pudo obtener el listado de los datos de referencia para la importación de listas de precio. Verifique los datos ingresados.");
            }

            var metadata = new MetadataGrid
            {
                TotalCount = reg.Total_Registros,
                PageSize = filters.Registros ?? 0,
                CurrentPage = filters.Pagina ?? 0,
                TotalPages = reg.Total_Paginas,
                HasNextPage = (filters.Pagina ?? 0) < reg.Total_Paginas,
                HasPreviousPage = (filters.Pagina ?? 0) > 1,
                //NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(ObtenerVendedores)) ?? "").ToString(),
                //PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(ObtenerVendedores)) ?? "").ToString(),
            };

            var response = new ApiResponse<List<ProductoDetalleDto>>(lista)
            {
                Meta = metadata
            };

            return Ok(response);
        }

        [HttpPost("confirmar-actualizacion-precio")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ConfirmarActualizacionPrecioProductosDeProveedor(AbmGenDto req)
        {
            if (req == null ||
                string.IsNullOrEmpty(req.Objeto) ||
                string.IsNullOrEmpty(req.Usuario) ||
                string.IsNullOrEmpty(req.Administracion) ||
                string.IsNullOrEmpty(req.Json))
            {
                return BadRequest("Los datos para confirmar la actualización de precios son inválidos.");
            }
            RespuestaDto resultado = _importarServicio.ConfirmarActualizacionPrecioProductosDeProveedor(req);
            if (resultado == null)
            {
                return BadRequest("No se pudo confirmar la actualización de precios. Verifique los datos ingresados.");
            }
            return Ok(new ApiResponse<RespuestaDto>(resultado));
        }
        #endregion
    }
}
