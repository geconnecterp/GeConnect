using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace gc.api.Controllers.Caja
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiCaja : ControllerBase
    {
        private readonly ILogger<ApiCaja> _logger;
        private readonly IApiCajaServicio _apiCajaServicio;
        public ApiCaja(ILogger<ApiCaja> logger, IApiCajaServicio apiCajaServicio)
        {
            _logger = logger;
            _apiCajaServicio = apiCajaServicio;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ValidaIntegridadUsuarioCaja(CajaValidaReqDto req)
        {
            try
            {
                if (string.IsNullOrEmpty(req.usu_id) ||
                    string.IsNullOrEmpty(req.caja_id) ||
                    string.IsNullOrEmpty(req.adm_id))
                {
                    return BadRequest("Los parámetros usu_id, caja_id y adm_id son requeridos.");
                }

                var res = _apiCajaServicio.ValidaIntegridadUsuarioCaja(req);
                return Ok(new ApiResponse<RespuestaDto>(res));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar la integridad del usuario en la caja.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Hubo un error al validar la integridad del usuario en la caja.");
            }
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult AperturaCaja(CajaValidaReqDto req)
        {
            try
            {
                if (string.IsNullOrEmpty(req.usu_id) ||
                    string.IsNullOrEmpty(req.caja_id) ||
                    string.IsNullOrEmpty(req.adm_id))
                {
                    return BadRequest("Los parámetros usu_id, caja_id y adm_id son requeridos.");
                }
                var res = _apiCajaServicio.AperturaCaja(req);
                return Ok(new ApiResponse<RespuestaDto>(res));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar la integridad del usuario en el cierre de caja.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Hubo un error al validar la integridad del usuario en el cierre de caja.");
            }
        }
    }
}
