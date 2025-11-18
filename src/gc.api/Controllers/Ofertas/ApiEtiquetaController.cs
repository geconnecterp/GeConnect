using gc.api.core.Contratos.Servicios.Ofertas;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Ofertas
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiEtiquetaController : ControllerBase
    {
        private readonly ILogger<ApiEtiquetaController> _logger;
        private readonly IApiEtiquetaServicio _etiqSv;
        public ApiEtiquetaController(ILogger<ApiEtiquetaController> logger, IApiEtiquetaServicio etiq)
        {
            _etiqSv = etiq;
            _logger = logger;
        }

        [HttpGet("ObtenerCargaPreviaUsuario/{adm_id}")]
        public IActionResult ObtenerCargaPreviaUsuario(string adm_id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(adm_id))
                {
                    return BadRequest("El parámetro adm_id es obligatorio.");
                }

                var resultado = _etiqSv.ObtenerCargaPreviaUsuario(adm_id);
                return Ok(new ApiResponse<List<CargaPreviaDto>>(resultado));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en {nameof(ObtenerCargaPreviaUsuario)}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }
        }

        // Nueva acción optimizada para ObtenerDetalleEtiquetas
        [HttpPost("ObtenerDetalleEtiquetas")]
        [ProducesResponseType(typeof(ApiResponse<List<IEDetalleDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult ObtenerDetalleEtiquetas([FromBody] QueryFilters? filters)
        {
            try
            {
                if (filters is null)
                {
                    return BadRequest("El cuerpo de la solicitud es obligatorio.");
                }

                var resultado = _etiqSv.ObtenerDetalleEtiquetas(filters);
                return Ok(new ApiResponse<List<IEDetalleDto>>(resultado));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en {Action}: {Message}", nameof(ObtenerDetalleEtiquetas), ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }
        }
    }
}
