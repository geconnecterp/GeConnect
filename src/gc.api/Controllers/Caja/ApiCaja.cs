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
        public IActionResult ValidaIntegridadUsuarioCaja(CajaReqDto req)
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

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult AperturaCaja(CajaReqDto req)
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

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult CierreCaja(CajaReqDto req)
        {
            if (string.IsNullOrEmpty(req.usu_id) ||
                string.IsNullOrEmpty(req.caja_id) ||
                string.IsNullOrEmpty(req.adm_id))
            {
                return BadRequest("Los parámetros usu_id, caja_id y adm_id son requeridos.");
            }

            var res = _apiCajaServicio.CierreCaja(req);
            return Ok(new ApiResponse<RespuestaDto>(res));
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaBusquedaResultadoDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult BusquedaCaja_b_cuenta(string busqueda)
        {
            if (string.IsNullOrEmpty(busqueda))
            {
                return BadRequest("El parámetro busqueda es requerido.");
            }

            var res = _apiCajaServicio.BusquedaCaja_b_cuenta(busqueda);
            return Ok(new ApiResponse<CuentaBusquedaResultadoDto>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoDatosResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerProductoDatos(ProductoDatosRequestDto req)
        {
            if (req == null)
            {
                return BadRequest("El parámetro req es requerido.");
            }

            var res = _apiCajaServicio.ObtenerProductoDatos(req);
            return Ok(new ApiResponse<ProductoDatosResponseDto>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult Cargar_CF(CargaCFRequestDto req)
        {
            if (req == null)
            {
                return BadRequest("El parámetro req es requerido.");
            }

            var res = _apiCajaServicio.Cargar_CF(req);
            return Ok(new ApiResponse<RespuestaDto>(res));
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CajaDatosDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerDatosCF(string caja_id)
        {
            if (string.IsNullOrEmpty(caja_id))
            {
                return BadRequest("El parámetro caja_id es requerido.");
            }

            var res = _apiCajaServicio.ObtenerDatosCF(caja_id);
            return Ok(new ApiResponse<CajaDatosDto>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult CierreCajaGral(string usu_id, string adm_id)
        {
            if (string.IsNullOrEmpty(usu_id) || string.IsNullOrEmpty(adm_id))
            {
                return BadRequest("Los parámetros usu_id y adm_id son requeridos.");
            }

            var res = _apiCajaServicio.CierreCajaGral(usu_id, adm_id);
            return Ok(new ApiResponse<RespuestaDto>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult HabilitarCajaGral(string usu_id, string adm_id)
        {
            if (string.IsNullOrEmpty(usu_id) || string.IsNullOrEmpty(adm_id))
            {
                return BadRequest("Los parámetros usu_id y adm_id son requeridos.");
            }

            var res = _apiCajaServicio.HabilitarCajaGral(usu_id, adm_id);
            return Ok(new ApiResponse<RespuestaDto>(res));
        }
    }
}
