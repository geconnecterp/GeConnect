using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Caja
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiRendicionController : ControllerBase
    {
        private readonly ILogger<ApiRendicionController> _logger;
        private readonly IApiRendicionServicio _apiRendicionServicio;
        public ApiRendicionController(ILogger<ApiRendicionController> logger, IApiRendicionServicio apiRendicionServicio)
        {
            _logger = logger;
            _apiRendicionServicio = apiRendicionServicio;
        }
        [HttpPost]
        [ProducesResponseType((int)System.Net.HttpStatusCode.OK, Type = typeof(ApiResponse<List<RendicionResponseDto>>))]
        [ProducesResponseType((int)System.Net.HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult CargarRendiciones([FromBody] RendicionRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("No se proporcionó una solicitud válida.");
            }
            var result = _apiRendicionServicio.ObtenerRendiciones(request);
            return Ok(new ApiResponse<List<RendicionResponseDto>>(result));
        }

        [HttpPost]
        [ProducesResponseType((int)System.Net.HttpStatusCode.OK, Type = typeof(ApiResponse<List<RendicionNominalResponseDto>>))]
        [ProducesResponseType((int)System.Net.HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult CargarNominaciones([FromBody] RendicionNominalRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("No se proporcionó una solicitud válida.");
            }

            var result = _apiRendicionServicio.ObtenerNominaciones(request);
            return Ok(new ApiResponse<List<RendicionNominalResponseDto>>(result));
        }

        [HttpPost]
        [ProducesResponseType((int)System.Net.HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)System.Net.HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ConfirmarRendicion([FromBody] RendicionCargaRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("No se proporcionó una solicitud válida.");
            }

            var result = _apiRendicionServicio.ConfirmarRendicion(request);
            return Ok(new ApiResponse<RespuestaDto>(result));
        }
    }
}
