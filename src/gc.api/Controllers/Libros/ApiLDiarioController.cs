using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Libros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.api.Controllers.Libros
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiLDiarioController : ControllerBase
    {
        private readonly IApiLDiarioServicio _asientoLDServicio;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ApiLDiarioController> _logger;

        public ApiLDiarioController(
           IApiLDiarioServicio asientoLDServicio,
           IOptions<AppSettings> appSettings,
           ILogger<ApiLDiarioController> logger)
        {
            _asientoLDServicio = asientoLDServicio;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una lista de asientos definitivos según los filtros proporcionados
        /// </summary>
        /// <param name="query">Filtros para la consulta de asientos</param>
        /// <returns>Lista de asientos definitivos</returns>
        [HttpPost("obtener-asientos-ldiario")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ApiResponse<List<AsientoDetalleLDDto>>> ObtenerAsientosLibroDiario([FromBody] LDiarioRequest query)
        {
            // Validar parámetros de entrada
            if (query == null)
            {
                return BadRequest(new ApiResponse<string>("El objeto de consulta no puede ser nulo."));
            }

            // Validación específica para asientos definitivos: el ejercicio es obligatorio
            if (query.Eje_nro <= 0)
            {
                return BadRequest(new ApiResponse<string>("Debe seleccionar un ejercicio contable para consultar asientos definitivos."));
            }

            if (query.Periodo && query.Desde > query.Hasta)
            {
                return BadRequest(new ApiResponse<string>("La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'."));
            }

            var reg = new AsientoDetalleLDDto { Total_paginas = 0, Total_registros = 0 };
            // Llamar al servicio para obtener los asientos
            var resultado = _asientoLDServicio.ObtenerAsientoLibroDiario(query.Eje_nro, query.Periodo,
                query.Desde, query.Hasta,
                query.RangoFC, query.DesdeFC, query.HastaFC,
                query.Movimientos, 
                query.ConTemporales, 
                query.Regs, query.Pagina, 
                query.Orden);

            if (resultado != null && resultado.Count() > 0)
            {
                reg = resultado.First();
            }
            else
            {
                return NotFound($"No se encontraron asientos para el Libro Diario. Verifique.");
            }

            // Presentar en el header información básica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = reg.Total_registros,
                PageSize = query.Regs,
                CurrentPage = query.Pagina,
                TotalPages = reg.Total_paginas,
                HasNextPage = query.Pagina< reg.Total_paginas,
                HasPreviousPage = query.Pagina> 1
            };

            var response = new ApiResponse<List<AsientoDetalleLDDto>>(resultado)
            {
                Meta = metadata
            };
            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            // Devolver respuesta exitosa
            return Ok(response);
        }

        /// <summary>
        /// Obtiene una lista de asientos definitivos según los filtros proporcionados
        /// </summary>
        /// <param name="query">Filtros para la consulta de asientos</param>
        /// <returns>Lista de asientos definitivos</returns>
        [HttpPost("obtener-asientos-ldiario-resumen")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ApiResponse<List<LibroDiarioResumen>>> ObtenerAsientosLibroDiarioResumen([FromBody] LDiarioRequest query)
        {
            // Validar parámetros de entrada
            if (query == null)
            {
                return BadRequest(new ApiResponse<string>("El objeto de consulta no puede ser nulo."));
            }

            // Validación específica para asientos definitivos: el ejercicio es obligatorio
            if (query.Eje_nro <= 0)
            {
                return BadRequest(new ApiResponse<string>("Debe seleccionar un ejercicio contable para consultar asientos definitivos."));
            }

            if (query.Periodo && query.Desde > query.Hasta)
            {
                return BadRequest(new ApiResponse<string>("La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'."));
            }

            var reg = new LibroDiarioResumen { Total_paginas = 0, Total_registros = 0 };
            // Llamar al servicio para obtener los asientos
            var resultado = _asientoLDServicio.ObtenerAsientoLibroDiarioResumen(query.Eje_nro, query.Periodo,
                query.Desde, query.Hasta,
                query.RangoFC, query.DesdeFC, query.HastaFC,
                query.Movimientos,
                query.ConTemporales,
                query.Regs, query.Pagina,
                query.Orden);

            if (resultado != null && resultado.Count() > 0)
            {
                reg = resultado.First();
            }
            else
            {
                return NotFound($"No se encontraron asientos para el Libro Diario. Verifique.");
            }

            // Presentar en el header información básica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = reg.Total_registros,
                PageSize = query.Regs,
                CurrentPage = query.Pagina,
                TotalPages = reg.Total_paginas,
                HasNextPage = query.Pagina < reg.Total_paginas,
                HasPreviousPage = query.Pagina > 1
            };

            var response = new ApiResponse<List<LibroDiarioResumen>>(resultado)
            {
                Meta = metadata
            };
            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            // Devolver respuesta exitosa
            return Ok(response);
        }
    }
}
