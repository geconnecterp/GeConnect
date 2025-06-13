using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.api.Controllers.Libros
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiLMayorController : ControllerBase
    {
        private readonly IApiLMayorServicio _apiLMayorServicio;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ApiLMayorController> _logger;

        public ApiLMayorController(
            IApiLMayorServicio apiLMayorServicio,
            IOptions<AppSettings> appSettings,
            ILogger<ApiLMayorController> logger)
        {
            _apiLMayorServicio = apiLMayorServicio;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una lista de asientos definitivos según los filtros proporcionados
        /// </summary>
        /// <param name="query">Filtros para la consulta de libro mayor</param>
        /// <returns>Detalle del libro Mayor</returns>
        [HttpPost("obtener-libro-mayor")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ApiResponse<List<LMayorRegListaDto>>> ObtenerLibroMayor([FromBody] LMayorFiltroDto query)
        {
            // Validar parámetros de entrada
            if (query == null)
            {
                return BadRequest(new ApiResponse<string>("El objeto de consulta no puede ser nulo."));
            }

            // Validación específica para asientos definitivos: el ejercicio es obligatorio
            if (query.eje_nro <= 0)
            {
                return BadRequest(new ApiResponse<string>("Debe seleccionar un ejercicio contable para consultar el libro mayor."));
            }

            if (query.rango && query.desde > query.hasta)
            {
                return BadRequest(new ApiResponse<string>("La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'."));
            }

            var reg = new LMayorRegListaDto { Total_paginas = 0, Total_registros = 0 };
            // Llamar al servicio para obtener los asientos
            var resultado = _apiLMayorServicio.ObtenerLibroMayor(query.eje_nro, query.ccb_id, query.rango, query.desde, query.hasta, query.incluirTemporales, query.Registros, query.Pagina, query.Sort ?? "");

            if (resultado != null && resultado.Count() > 0)
            {
                reg = resultado.First();
            }
            else
            {
                return NotFound($"No se encontraron registros del Libro Mayor.");
            }

            // Presentar en el header información básica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = reg.Total_registros,
                PageSize = query.Registros,
                CurrentPage = query.Pagina,
                TotalPages = reg.Total_paginas,
                HasNextPage = query.Pagina < reg.Total_paginas,
                HasPreviousPage = query.Pagina > 1
            };

            var response = new ApiResponse<List<LMayorRegListaDto>>(resultado)
            {
                Meta = metadata
            };
            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            // Devolver respuesta exitosa
            return Ok(response);
        }
    }
}
