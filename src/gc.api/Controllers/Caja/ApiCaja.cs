using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
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
        public IActionResult BusquedaClientes(string busqueda, string adm_id, string usu_id)
        {
            if (string.IsNullOrEmpty(busqueda))
            {
                return BadRequest("El parámetro busqueda es requerido.");
            }

            if (string.IsNullOrEmpty(usu_id) || string.IsNullOrEmpty(adm_id))
            {
                return BadRequest("Los datos del usuario y la sucursal es necesario");
            }

            var res = _apiCajaServicio.BusquedaClientes(busqueda, adm_id, usu_id);
            return Ok(new ApiResponse<List<CuentaBusquedaResultadoDto>>(res));
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaDatosResultadoDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult BuscarDatosCliente(string origen, string valor, string adm_id, string usu_id)
        {
            if (string.IsNullOrEmpty(origen) || string.IsNullOrEmpty(valor))
            {
                return BadRequest("Faltan identificadores importantes, origen o valor. Verifique");
            }

            if (string.IsNullOrEmpty(usu_id) || string.IsNullOrEmpty(adm_id))
            {
                return BadRequest("Los datos del usuario y la sucursal es necesario");
            }

            var res = _apiCajaServicio.BusquedaDatosCliente(origen, valor, adm_id, usu_id);
            return Ok(new ApiResponse<CuentaDatosResultadoDto>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ConfirmaConsumidorFinal(ClienteRequestDto req)
        {
            if (req == null)
            {
                return BadRequest("El parámetro req es requerido.");
            }

            var res = _apiCajaServicio.ConfirmaConsumidorFinal(req);
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
        public IActionResult CierreCajaGral(CajaCerrarRequest request)
        {
            if (request == null)
                return BadRequest("El parámetro request es requerido.");

            if (string.IsNullOrEmpty(request.usu_id) || string.IsNullOrEmpty(request.adm_id))
                return BadRequest("Los parámetros usu_id y adm_id son requeridos.");

            var res = _apiCajaServicio.CierreCajaGral(request.usu_id, request.adm_id);
            return Ok(new ApiResponse<RespuestaDto>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult HabilitarCajaGral(CajaHabilitarRequest request)
        {
            if (request == null)
                return BadRequest("El parámetro request es requerido.");

            if (string.IsNullOrEmpty(request.usu_id) || string.IsNullOrEmpty(request.adm_id))
                return BadRequest("Los parámetros usu_id y adm_id son requeridos.");

            var res = _apiCajaServicio.HabilitarCajaGral(request.usu_id, request.adm_id);
            return Ok(new ApiResponse<RespuestaDto>(res));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerPVAbiertos(string admId)
        {
            var res = _apiCajaServicio.ObtenerPVAbiertos(admId);
            return Ok(new ApiResponse<List<CajaPVAbiertosDto>>(res));
        }


        // RespuestaDto ValidaEstadoPV(CajaValidaPVDto req);
        //RespuestaDto CargaStkDeFactura(CargaStkDto req);

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ValidaEstadoPV(CajaValidaPVDto req)
        {
            if (req == null)
                return BadRequest("El parámetro req es requerido.");
            var res = _apiCajaServicio.ValidaEstadoPV(req);
            return Ok(new ApiResponse<RespuestaDto>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult CargaStkDeFactura(CargaStkDto req)
        {
            if (req == null)
                return BadRequest("El parámetro req es requerido.");
            var res = _apiCajaServicio.CargaStkDeFactura(req);
            return Ok(new ApiResponse<RespuestaDto>(res));
        }
    }
}
