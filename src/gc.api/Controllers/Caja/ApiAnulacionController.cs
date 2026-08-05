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
    public class ApiAnulacionController : ControllerBase
    {
        private readonly ILogger<ApiAnulacionController> _logger;
        private readonly IApiAnulacionServicio _apiAnulacionServicio;

        public ApiAnulacionController(ILogger<ApiAnulacionController> logger, IApiAnulacionServicio apiAnulacionServicio)
        {
            _logger = logger;
            _apiAnulacionServicio = apiAnulacionServicio;
        }

        [HttpPost]
        [ProducesResponseType((int)System.Net.HttpStatusCode.OK, Type = typeof(ApiResponse<List<AnulacionCobranzaResponseDto>>))]
        [ProducesResponseType((int)System.Net.HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult BuscarCobranzas([FromBody] AnulacionCobranzaBuscarRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("No se proporciono una solicitud valida.");
            }

            _logger.LogInformation(
                "Anulacion cobranza: buscando cobranzas. Cta={Cta}; Fecha={Fecha}; Proceso={Proceso}; Cierre={Cierre}; Adm={Adm}; Usuario={Usuario}",
                request.cta_id,
                request.fecha,
                request.caja_nro_proceso,
                request.caja_nro_cierre,
                request.adm_id,
                request.usu_id);

            var result = _apiAnulacionServicio.BuscarCobranzas(request);
            return Ok(new ApiResponse<List<AnulacionCobranzaResponseDto>>(result));
        }

        [HttpPost]
        [ProducesResponseType((int)System.Net.HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)System.Net.HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult AnularCobranza([FromBody] AnulacionCobranzaConfirmarRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("No se proporciono una solicitud valida.");
            }

            _logger.LogInformation(
                "Anulacion cobranza: confirmando anulacion. Cta={Cta}; ProcesoAnula={ProcesoAnula}; CierreAnula={CierreAnula}; OperacionAnula={OperacionAnula}; Caja={Caja}; Adm={Adm}; Usuario={Usuario}; Autoriza={Autoriza}",
                request.cta_id,
                request.caja_nro_proceso_anu,
                request.caja_nro_cierre_anu,
                request.caja_nro_operacion_anu,
                request.caja_id,
                request.adm_id,
                request.usu_id,
                request.usu_id_autoriza);

            var result = _apiAnulacionServicio.AnularCobranza(request);
            return Ok(new ApiResponse<RespuestaDto>(result));
        }
    }
}
