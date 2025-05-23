using gc.api.core.Contratos.Servicios.Asientos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace gc.api.Controllers.Asientos
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiAsientoTemporalController : ControllerBase
    {
        private readonly IAsientoTemporalServicio _asientoTemporalServicio;

        public ApiAsientoTemporalController(IAsientoTemporalServicio asientoTemporalServicio)
        {
            _asientoTemporalServicio = asientoTemporalServicio;
        }

        /// <summary>
        /// Obtiene una lista de asientos temporales según los filtros proporcionados
        /// </summary>
        /// <param name="query">Filtros para la consulta de asientos</param>
        /// <returns>Lista de asientos temporales</returns>
        [HttpPost("obtener-asientos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ApiResponse<List<AsientoGridDto>>> ObtenerAsientos([FromBody] QueryAsiento query)
        {
            // Validar parámetros de entrada
            if (query == null)
            {
                return BadRequest(new ApiResponse<string>("El objeto de consulta no puede ser nulo."));
            }

            if (query.Rango && query.Desde > query.Hasta)
            {
                return BadRequest(new ApiResponse<string>("La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'."));
            }

            var reg = new AsientoGridDto { Total_paginas = 0, Total_registros = 0 };
            // Llamar al servicio para obtener los asientos
            var resultado = _asientoTemporalServicio.ObtenerAsientos(query);

            if (resultado != null && resultado.Count() > 0)
            {
                reg = resultado.First();
            }
            else
            {
                return NotFound("No se encontraron asientos temporales.");
            }

            // Presentar en el header información básica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = reg.Total_registros,
                PageSize = query.TotalRegistros,
                CurrentPage = query.Paginas,
                TotalPages = reg.Total_paginas,
                HasNextPage = query.Paginas < reg.Total_paginas,
                HasPreviousPage = query.Paginas > 1
            };

            var response = new ApiResponse<List<AsientoGridDto>>(resultado)
            {
                Meta = metadata
            };
            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            // Devolver respuesta exitosa
            return Ok(response);
        }

        /// <summary>
        /// Pasa los asientos temporales seleccionados a contabilidad.
        /// </summary>
        /// <param name="asientoPasa">Datos necesarios para el traspaso de asientos.</param>
        /// <returns>Resultado de la operación de traspaso.</returns>
        [HttpPost("pasar-asientos-contabilidad")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ApiResponse<List<RespuestaDto>>> PasarAsientosTmpAContabilidad([FromBody] AsientoPasaDto asientoPasa)
        {
            // Validar parámetros de entrada
            if (asientoPasa == null)
            {
                return BadRequest(new ApiResponse<string>("El objeto de traspaso no puede ser nulo."));
            }

            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(asientoPasa.JsonDiaMovi))
            {
                return BadRequest(new ApiResponse<string>("El ID del movimiento es obligatorio."));
            }

            if (string.IsNullOrWhiteSpace(asientoPasa.Usu_id))
            {
                return BadRequest(new ApiResponse<string>("El ID del usuario es obligatorio."));
            }

            if (string.IsNullOrWhiteSpace(asientoPasa.Adm_id))
            {
                return BadRequest(new ApiResponse<string>("El ID de la administración es obligatorio."));
            }

            // Llamar al servicio para realizar el traspaso
            var resultado = _asientoTemporalServicio.PasarAsientosTmpAContabilidad(asientoPasa);

            // Devolver respuesta exitosa con el objeto RespuestaDto completo
            return Ok(new ApiResponse<List<RespuestaDto>>(resultado));


        }

        /// <summary>
        /// Obtiene el detalle de un asiento temporal específico.
        /// </summary>
        /// <param name="moviId">Identificador del movimiento del asiento.</param>
        /// <returns>Detalle del asiento temporal.</returns>
        [HttpGet("obtener-asiento-detalle/{moviId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ApiResponse<AsientoDetalleDto>> ObtenerAsientoDetalle(string moviId)
        {
            // Validar parámetros de entrada
            if (string.IsNullOrWhiteSpace(moviId))
            {
                return BadRequest(new ApiResponse<string>("El identificador del asiento no puede estar vacío."));
            }

            //try
            //{
            // Llamar al servicio para obtener el detalle del asiento
            var resultado = _asientoTemporalServicio.ObtenerAsientoDetalle(moviId);

            if (resultado == null)
            {
                return NotFound($"No se encontró el asiento con identificador {moviId}.");
            }

            // Devolver respuesta exitosa
            return Ok(new ApiResponse<AsientoDetalleDto>(resultado));
            //}
            //catch (Exception ex)
            //{
            //    // Manejar excepciones
            //    return StatusCode(StatusCodes.Status500InternalServerError,
            //        new ApiResponse<string>($"Error al obtener el detalle del asiento: {ex.Message}"));
            //}
        }

    }
}
