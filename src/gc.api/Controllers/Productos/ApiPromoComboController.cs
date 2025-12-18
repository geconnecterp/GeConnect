using gc.api.core.Contratos.Servicios.Ofertas;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.Dtos.Productos.PromoCombo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Ofertas
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiPromoComboController : ControllerBase
    {
        private readonly IApiPromoComboServicio _promoComboServicio;
        private readonly ILogger<ApiPromoComboController> _logger;


        public ApiPromoComboController(IApiPromoComboServicio promoComboServicio, ILogger<ApiPromoComboController> logger)
        {
            _promoComboServicio = promoComboServicio ?? throw new ArgumentNullException(nameof(promoComboServicio));
            _logger = logger;
        }

        /// <summary>
        /// Obtiene los tipos disponibles para el combo
        /// </summary>
        /// <returns>Lista de tipos de combo</returns>
        /// <response code="200">Devuelve la lista de tipos</response>
        /// <response code="500">Si ocurre un error durante el proceso</response>
        [HttpGet("combos-tipos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ObtenerTipos()
        {
            try
            {
                var tipos = _promoComboServicio.ObtenerComboTipo();

                return Ok(new ApiResponse<List<ComboTipoDto>>(tipos));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { ok = false, mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los estados disponibles para el combo
        /// </summary>
        /// <returns>Lista de estados de combo</returns>
        /// <response code="200">Devuelve la lista de estados</response>
        /// <response code="500">Si ocurre un error durante el proceso</response>
        [HttpGet("combos-estados")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ObtenerEstados()
        {
            try
            {
                var estados = _promoComboServicio.ObtenerComboEstado();
                return Ok(new ApiResponse<List<ComboEstadoDto>>(estados));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { ok = false, mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el detalle de combos según los filtros especificados
        /// </summary>
        /// <param name="filtros">Filtros para la búsqueda de combos</param>
        /// <returns>Lista de combos que cumplen con los filtros</returns>
        /// <response code="200">Devuelve la lista de combos filtrados</response>
        /// <response code="400">Si los filtros son inválidos</response>
        /// <response code="500">Si ocurre un error durante el proceso</response>
        [HttpPost("combos-buscar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BuscarCombos([FromBody] QueryFilters filtros)
        {
            // Validar que el modelo sea válido
            if (!ModelState.IsValid)
            {
                return BadRequest(new { ok = false, mensaje = "Los filtros proporcionados no son válidos" });
            }

            ComboListaDto reg = new ComboListaDto { Total_Paginas = 0, Total_Registros = 0 };

            // Asegurar valores por defecto para paginación
            filtros.Pagina = filtros.Pagina <= 0 ? 1 : filtros.Pagina;
            filtros.Registros = filtros.Registros <= 0 ? 10 : filtros.Registros;

            var combos = _promoComboServicio.ObtenerDetalleDeCombos(filtros);

            if (combos.Count > 0)
            {
                reg = combos[0];
                // Incluir información de paginación en los headers de la respuesta
                var metadata = new MetadataGrid
                {
                    TotalCount = reg.Total_Registros,
                    PageSize = filtros.Registros.Value,
                    CurrentPage = filtros.Pagina.Value,
                    TotalPages = reg.Total_Paginas,
                    HasNextPage = filtros.Pagina < reg.Total_Paginas,
                    HasPreviousPage = filtros.Pagina > 1
                };
                //Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(metadata));
                return Ok(new ApiResponse<List<ComboListaDto>>(combos) { Meta = metadata });
            }
            else
            {
                return Ok(new ApiResponse<List<ComboListaDto>>(combos));
            }
        }

        /// <summary>
        /// Obtiene los datos de un combo por su identificador
        /// </summary>
        /// <param name="id">Identificador único del combo</param>
        /// <returns>Datos detallados del combo solicitado</returns>
        /// <response code="200">Devuelve los datos del combo</response>
        /// <response code="404">Si el combo no existe</response>
        /// <response code="500">Si ocurre un error durante el proceso</response>
        [HttpGet("combo/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ObtenerComboPorId(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { ok = false, mensaje = "El identificador del combo es requerido" });
                }

                var combo = _promoComboServicio.ObtenerComboPorId(id);
                
                if (combo == null)
                {
                    return NotFound(new { ok = false, mensaje = $"No se encontró el combo con id {id}" });
                }
                
                return Ok(new ApiResponse<ComboDatosDto>(combo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener combo por ID {ComboId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { ok = false, mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los canales asociados a un combo específico
        /// </summary>
        /// <param name="id">Identificador único del combo</param>
        /// <returns>Lista de canales del combo</returns>
        /// <response code="200">Devuelve la lista de canales</response>
        /// <response code="404">Si el combo no existe o no tiene canales asociados</response>
        /// <response code="500">Si ocurre un error durante el proceso</response>
        [HttpGet("combo/{id}/canales")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ObtenerCanalesDeCombo(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { ok = false, mensaje = "El identificador del combo es requerido" });
                }

                var canales = _promoComboServicio.ObtenerCanalesDeCombo(id);
                
                if (canales == null || canales.Count == 0)
                {
                    return NotFound(new { ok = false, mensaje = $"No se encontraron canales para el combo con id {id}" });
                }
                
                return Ok(new ApiResponse<List<ComboCanalDto>>(canales));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener canales para el combo {ComboId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { ok = false, mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los productos asociados a un combo específico
        /// </summary>
        /// <param name="id">Identificador único del combo</param>
        /// <returns>Lista de productos del combo</returns>
        /// <response code="200">Devuelve la lista de productos</response>
        /// <response code="404">Si el combo no existe o no tiene productos asociados</response>
        /// <response code="500">Si ocurre un error durante el proceso</response>
        [HttpGet("combo/{id}/productos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ObtenerProductosDeCombo(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { ok = false, mensaje = "El identificador del combo es requerido" });
                }

                var productos = _promoComboServicio.ObtenerProductosDeCombo(id);
                
                if (productos == null || productos.Count == 0)
                {
                    return NotFound(new { ok = false, mensaje = $"No se encontraron productos para el combo con id {id}" });
                }
                
                return Ok(new ApiResponse<List<ComboProductoDto>>(productos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos para el combo {ComboId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { ok = false, mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los productos sustitutos asociados a un producto dentro de un combo específico
        /// </summary>
        /// <param name="id">Identificador único del combo</param>
        /// <param name="productoId">Identificador único del producto</param>
        /// <returns>Lista de productos sustitutos</returns>
        /// <response code="200">Devuelve la lista de productos sustitutos</response>
        /// <response code="400">Si los parámetros son inválidos</response>
        /// <response code="404">Si no se encuentran sustitutos</response>
        /// <response code="500">Si ocurre un error durante el proceso</response>
        [HttpGet("combo/{id}/producto/{productoId}/sustitutos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ObtenerProductosSustitutosDeCombo(string id, string productoId)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { ok = false, mensaje = "El identificador del combo es requerido" });
                }

                if (string.IsNullOrEmpty(productoId))
                {
                    return BadRequest(new { ok = false, mensaje = "El identificador del producto es requerido" });
                }

                var sustitutos = _promoComboServicio.ObtenerProductosSustitutosDeCombo(id, productoId);
                
                // Aquí no devolvemos 404 si la lista está vacía, ya que es válido que un producto no tenga sustitutos
                // Simplemente devolvemos una lista vacía
                
                return Ok(new ApiResponse<List<ComboSustitutoDto>>(sustitutos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sustitutos para el producto {ProductoId} del combo {ComboId}", productoId, id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { ok = false, mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Confirma la creación o actualización de un combo
        /// </summary>
        /// <param name="request">Datos del combo a confirmar</param>
        /// <returns>Resultado de la operación de confirmación</returns>
        /// <response code="200">Operación completada con éxito</response>
        /// <response code="400">Si los datos proporcionados no son válidos</response>
        /// <response code="500">Si ocurre un error durante el proceso</response>
        [HttpPost("combo-confirmar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ConfirmarCombo([FromBody] AbmPlusGenDto request)
        {
            try
            {
                // Validar que el modelo sea válido
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { ok = false, mensaje = "Los datos proporcionados no son válidos", errores = ModelState });
                }

                // Llamar al servicio para confirmar el combo
                var resultado = _promoComboServicio.ConfirmarCombo(request);

                // Verificar si la operación fue exitosa según el código de resultado
                
                    return Ok(new ApiResponse<RespuestaDto>(resultado));
              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar combo {@ComboData}", request);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { ok = false, mensaje = ex.Message });
            }
        }

        [HttpPost("combo-repo")]
        public IActionResult ObtenerCombosRepo(ComboReqDto req)
        {
            try
            {
                if(req== null)
                {
                    return BadRequest(new { ok = false, mensaje = "Los datos de la solicitud son requeridos" });
                }   

                List<ComboRepoDto> combos = _promoComboServicio.ObtenerCombosRepo(req);
                return Ok(new ApiResponse<List<ComboRepoDto>>(combos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener combos repo");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { ok = false, mensaje = ex.Message });
            }
        }
    }
}