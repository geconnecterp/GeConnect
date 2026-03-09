using gc.api.core.Contratos.Servicios;
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
        public IActionResult ObtenerOrdenesReparto(ORRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning("Solicitud de ordenes de reparto vacía o nula.");
                    return BadRequest(new ApiResponse<string>("La solicitud no puede estar vacía."));
                }

                var data = _orSv.ObtenerOrdenesReparto(request);
                return Ok(new ApiResponse<List<OrdenRepartoListDto>>(data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las ordenes de reparto");
                return BadRequest(new ApiResponse<string>("Ocurrió un error al procesar la solicitud."));
            }
        }

    }
}
