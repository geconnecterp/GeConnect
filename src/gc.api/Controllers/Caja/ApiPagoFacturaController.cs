using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Servicios.LineaCaja;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace gc.api.Controllers.Caja
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiPagoFacturaController : ControllerBase
    {
        private readonly ILogger<ApiPagoFacturaController> _logger;
        private readonly IApiPagoFactServicio _apiPagoFactServicio;

        public ApiPagoFacturaController(ILogger<ApiPagoFacturaController> logger, IApiPagoFactServicio servicio)
        {
            _logger = logger;
            _apiPagoFactServicio = servicio;
        }
        /*
          List<ValoresPendientesResDto> ObtenerValoresPendientes(ValoresPendientesReqDto req);
        List<ValoresNCResDto> ObtenerValoresNC(ValoresNCReqDto req);
        List<ValoresMPResDto> ObtenerValoresMP(ValoresMPReqDto req);
        List<ValoresInsResDto> ObtenerValoresIns(ValoresInsReqDto req); 
         */

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoDatosResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerValoresPendientes(ValoresPendientesReqDto req)
        {
            if (req == null)
            {
                return BadRequest("El parámetro req es requerido.");
            }

            var res = _apiPagoFactServicio.ObtenerValoresPendientes(req);
            return Ok(new ApiResponse<List<ValoresPendientesResDto>>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoDatosResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerValoresNC(ValoresNCReqDto req)
        {
            if (req == null)
            {
                return BadRequest("El parámetro req es requerido.");
            }

            var res = _apiPagoFactServicio.ObtenerValoresNC(req);
            return Ok(new ApiResponse<List<ValoresNCResDto>>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoDatosResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerValoresMP(ValoresMPReqDto req)
        {
            if (req == null)
            {
                return BadRequest("El parámetro req es requerido.");
            }

            var res = _apiPagoFactServicio.ObtenerValoresMP(req);
            return Ok(new ApiResponse<List<ValoresMPResDto>>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoDatosResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerValoresIns(ValoresInsReqDto req)
        {
            if (req == null)
            {
                return BadRequest("El parámetro req es requerido.");
            }

            var res = _apiPagoFactServicio.ObtenerValoresIns(req);
            return Ok(new ApiResponse<List<ValoresInsResDto>>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ConfirmarOperacionCaja(CajaOpeConfirmarReq req)
        {
            if (req == null)
            {
                return BadRequest("El parámetro req es requerido.");
            }
            var res = _apiPagoFactServicio.ConfirmarOperacionCaja(req);
            return Ok(new ApiResponse<RespuestaDto>(res));

        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<FactPendienteResponseDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerFacturasPendientes(FactPendienteRequestDto req)
        {
            if (req == null)
            {
                return BadRequest("El parámetro req es requerido.");
            }
            var res = _apiPagoFactServicio.ObtenerFacturasPendientes(req);
            return Ok(new ApiResponse<List<FactPendienteResponseDto>>(res));
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<CtaCteResponseDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerCtaCte(string cta_id, string adm_id)
        {
            if (string.IsNullOrEmpty(cta_id) || string.IsNullOrEmpty(adm_id))
            {
                return BadRequest("Los parámetros cta_id y adm_id son requeridos.");
            }
            var res = _apiPagoFactServicio.ObtenerCtaCte(cta_id, adm_id);
            return Ok(new ApiResponse<List<CtaCteResponseDto>>(res));
        }
    }
}
