using gc.api.core.Contratos.Servicios.Asientos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Asientos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.api.Controllers.Asientos
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class AsientoDefinitivoController : ControllerBase
    {
        private readonly IAsientoDefinitivoServicio _asientoDefinitivoServicio;
        private readonly AppSettings _appSettings;
        private readonly ILogger<AsientoDefinitivoController> _logger;

        public AsientoDefinitivoController(
           IAsientoDefinitivoServicio asientoDefinitivoServicio,
           IOptions<AppSettings> appSettings,
           ILogger<AsientoDefinitivoController> logger)
        {
            _asientoDefinitivoServicio = asientoDefinitivoServicio;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una lista de asientos definitivos según los filtros proporcionados
        /// </summary>
        /// <param name="query">Filtros para la consulta de asientos</param>
        /// <returns>Lista de asientos definitivos</returns>
        [HttpPost("obtener-asientos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ApiResponse<List<AsientoDefGridDto>>> ObtenerAsientos([FromBody] QueryAsiento query)
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

            if (query.Rango && query.Desde > query.Hasta)
            {
                return BadRequest(new ApiResponse<string>("La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'."));
            }

            var reg = new AsientoDefGridDto { Total_paginas = 0, Total_registros = 0 };
            // Llamar al servicio para obtener los asientos
            var resultado = _asientoDefinitivoServicio.ObtenerAsientos(query);

            if (resultado != null && resultado.Count() > 0)
            {
                reg = resultado.First();
            }
            else
            {
                return NotFound($"No se encontraron asientos definitivos para el usuario {query.Usu_like}.");
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

            var response = new ApiResponse<List<AsientoDefGridDto>>(resultado)
            {
                Meta = metadata
            };
            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            // Devolver respuesta exitosa
            return Ok(response);
        }

        /// <summary>
        /// Obtiene el detalle de un asiento definitivo específico.
        /// </summary>
        /// <param name="id">Identificador del asiento</param>
        /// <returns>Detalle del asiento definitivo</returns>
        /// 
        [HttpGet]
        [Route("[action]")]
        public ActionResult<AsientoDetalleDto> ObtenerAsientoDetalle(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiResponse<string>("El identificador del asiento no puede estar vacío."));
            }

            //try
            //{
            // Llamar al servicio para obtener el detalle del asiento
            var resultado = _asientoDefinitivoServicio.ObtenerAsientoDetalle(id);

            if (resultado == null)
            {
                return NotFound($"No se encontró el asiento con identificador {id}.");
            }

            // Devolver respuesta exitosa
            return Ok(new ApiResponse<AsientoDetalleDto>(resultado));
        }

        
    }
}
