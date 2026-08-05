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
            _logger.LogInformation("Rendiciones API: CargarRendiciones request. Adm={Adm}; Tipo={Tipo}", request.adm_id, request.tipo);
            var result = _apiRendicionServicio.ObtenerRendiciones(request);
            _logger.LogInformation("Rendiciones API: CargarRendiciones response. Registros={Registros}", result?.Count ?? 0);
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

            _logger.LogInformation("Rendiciones API: CargarNominaciones request. Adm={Adm}; Instrumento={Instrumento}", request.adm_id, request.ins_id);
            var result = _apiRendicionServicio.ObtenerNominaciones(request);
            _logger.LogInformation("Rendiciones API: CargarNominaciones response. Registros={Registros}", result?.Count ?? 0);
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

            _logger.LogInformation(
                "Rendiciones API: ConfirmarRendicion request. Caja={Caja}; Proceso={Proceso}; Cierre={Cierre}; Adm={Adm}; Usuario={Usuario}; JsonRendiciones={JsonRendiciones}",
                request.caja_id,
                request.caja_nro_proceso,
                request.caja_nro_cierre,
                request.adm_id,
                request.usu_id,
                request.json_rendiciones);

            var result = _apiRendicionServicio.ConfirmarRendicion(request);

            _logger.LogInformation(
                "Rendiciones API: ConfirmarRendicion response. Resultado={Resultado}; ResultadoId={ResultadoId}; Mensaje={Mensaje}; SetFocus={SetFocus}",
                result?.resultado,
                result?.resultado_id,
                result?.resultado_msj,
                result?.resultado_setfocus);

            return Ok(new ApiResponse<RespuestaDto>(result));
        }
    }
}
