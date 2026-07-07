using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace gc.api.Controllers.Caja
{
    //[Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiNotaCreditoController : ControllerBase
    {

        private readonly ILogger<ApiNotaCreditoController> _logger;
        private readonly IApiNotaCreditoServicio _apiNotaCreditoServicio;

        public ApiNotaCreditoController(ILogger<ApiNotaCreditoController> logger, IApiNotaCreditoServicio apiNotaCreditoServicio)
        {
            _logger = logger;
            _apiNotaCreditoServicio = apiNotaCreditoServicio;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<NCValidaResponseDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ValidarNC([FromBody] NCValidaRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("No se proporcionó una solicitud válida.");
            }

            var result = _apiNotaCreditoServicio.ValidarNC(request);

            return Ok(new ApiResponse<List<NCValidaResponseDto>>(result));

        }


        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<NCProductoBuscarResponseDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult BuscarProducto([FromBody] NCProductoBuscarRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("No se proporcionó una solicitud válida.");
            }

            var result = _apiNotaCreditoServicio.BuscarProducto(request);

            return Ok(new ApiResponse<List<NCProductoBuscarResponseDto>>(result));
        }


    }

}
