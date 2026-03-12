using DocumentFormat.OpenXml.Spreadsheet;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.OrdenReparto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace gc.api.Controllers.OrdenReparto
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiORController : ControllerBase
    {
        private readonly IOrdenRepartoServicio _orSv;
        private readonly ILogger<ApiORController> _logger;

        public ApiORController(IOrdenRepartoServicio ordenReparto,
            ILogger<ApiORController> logger)
        {
            _logger = logger;
            _orSv = ordenReparto;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<OrdenRepartoListDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerOrdenesReparto(ORRequestDto request)
        {

            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Solicitud de ordenes de reparto vacía o nula.");
                    return BadRequest(new ApiResponse<string>("La solicitud no puede estar vacía."));
                }

                var reg = new OrdenRepartoListDto { total_paginas = 0, total_registros = 0 };

                var data = _orSv.ObtenerOrdenesReparto(request);

                if (data.Count > 0)
                {
                    reg = data[0];
                }

                var metadata = new MetadataGrid
                {
                    TotalCount = reg.total_registros,
                    PageSize = request.Registros,
                    CurrentPage = request.Pagina,
                    TotalPages = reg.total_paginas,
                    HasNextPage = request.Pagina < reg.total_paginas,
                    HasPreviousPage = request.Pagina > 1,
                    NextPageUrl = string.Empty,
                    PreviousPageUrl = string.Empty,
                };

                return Ok(new ApiResponse<List<OrdenRepartoListDto>>(data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las ordenes de reparto");
                return BadRequest(new ApiResponse<string>("Ocurrió un error al procesar la solicitud."));
            }
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<ORListaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerListaORbyRubro(string or_compte, string adm, string usu)
        {

            if (string.IsNullOrWhiteSpace(or_compte))
            {
                _logger.LogWarning("Parámetro or_compte se encuentra vacío o nulo.");
                return BadRequest(new ApiResponse<string>("El parámetro rubro no puede estar vacío."));
            }

            if (string.IsNullOrWhiteSpace(adm))
            {
                _logger.LogWarning("Parámetro adm se encuentra vacío o nulo.");
                return BadRequest(new ApiResponse<string>("El parámetro adm no puede estar vacío."));
            }

            if (string.IsNullOrWhiteSpace(usu))
            {
                _logger.LogWarning("Parámetro usu se encuentra vacío o nulo.");
                return BadRequest(new ApiResponse<string>("El parámetro usu no puede estar vacío."));
            }

            var data = _orSv.ObtenerListaORbyRubro(or_compte, adm, usu);

            return Ok(new ApiResponse<List<ORListaDto>>(data));

        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<ORListaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerListaORbyBox(string or_compte, string adm, string usu)
        {
            if (string.IsNullOrWhiteSpace(or_compte))
            {
                _logger.LogWarning("Parámetro or_compte se encuentra vacío o nulo.");
                return BadRequest(new ApiResponse<string>("El parámetro rubro no puede estar vacío."));
            }
            if (string.IsNullOrWhiteSpace(adm))
            {
                _logger.LogWarning("Parámetro adm se encuentra vacío o nulo.");
                return BadRequest(new ApiResponse<string>("El parámetro adm no puede estar vacío."));
            }
            if (string.IsNullOrWhiteSpace(usu))
            {
                _logger.LogWarning("Parámetro usu se encuentra vacío o nulo.");
                return BadRequest(new ApiResponse<string>("El parámetro usu no puede estar vacío."));
            }
            var data = _orSv.ObtenerListaORbyBox(or_compte, adm, usu);
            return Ok(new ApiResponse<List<ORListaDto>>(data));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<ORProdRequestDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerListaORProductos(ORProdRequestDto request)
        {
            if (request == null)
            {
                _logger.LogWarning("Solicitud de productos de orden de reparto vacía o nula.");
                return BadRequest(new ApiResponse<string>("La solicitud no puede estar vacía."));
            }
            if (string.IsNullOrWhiteSpace(request.or_compte))
            {
                _logger.LogWarning("Parámetro or_compte se encuentra vacío o nulo.");
                return BadRequest(new ApiResponse<string>("El parámetro or_compte no puede estar vacío."));
            }
            if (string.IsNullOrWhiteSpace(request.adm_id))
            {
                _logger.LogWarning("Parámetro adm_id se encuentra vacío o nulo.");
                return BadRequest(new ApiResponse<string>("El parámetro adm_id no puede estar vacío."));
            }
            if (string.IsNullOrWhiteSpace(request.usu_id))
            {
                _logger.LogWarning("Parámetro usu_id se encuentra vacío o nulo.");
                return BadRequest(new ApiResponse<string>("El parámetro usu_id no puede estar vacío."));
            }
            if (string.IsNullOrWhiteSpace(request.box_id))
            {
                _logger.LogWarning("Parámetro box_id se encuentra vacío o nulo.");
                return BadRequest(new ApiResponse<string>("El parámetro box_id no puede estar vacío."));
            }
            if (string.IsNullOrWhiteSpace(request.rub_id))
            {
                _logger.LogWarning("Parámetro rub_id se encuentra vacío o nulo.");
                return BadRequest(new ApiResponse<string>("El parámetro rub_id no puede estar vacío."));
            }
            var data = _orSv.ObtenerListaORProductos(request);
            return Ok(new ApiResponse<List<ORProductoDto>>(data));
        }
    }
}
