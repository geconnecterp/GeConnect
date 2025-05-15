using gc.api.core.Contratos.Servicios.Asientos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Asientos;
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
    }
}
