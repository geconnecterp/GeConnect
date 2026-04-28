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
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiProductoFactController : ControllerBase
    {
        private readonly ILogger<ApiProductoFactController> _logger;
        private readonly IApiProductoFactServicio _apiProductoFactServicio;

        public ApiProductoFactController(ILogger<ApiProductoFactController> logger, IApiProductoFactServicio apiProductoFactServicio)
        {
            _logger = logger;
            _apiProductoFactServicio = apiProductoFactServicio;
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

            var res = _apiProductoFactServicio.ObtenerProductoDatos(req);
            return Ok(new ApiResponse<List<ProductoDatosResponseDto>>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CalculaFilasResDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult CalcularFilas(CalcularFilasReqDto req)
        {
            if (req == null)
            {
                return BadRequest("El parámetro req es requerido.");
            }

            var res = _apiProductoFactServicio.CalcularFilas(req);
            return Ok(new ApiResponse<CalculaFilasResDto>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<PrefacturaResDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerPrefactura(PrefacturaReqDto req)
        {
            if (req == null)
            {
                return BadRequest("El parámetro req es requerido.");
            }

            var res = _apiProductoFactServicio.ObtenerPrefactura(req);
            return Ok(new ApiResponse<List<PrefacturaResDto>>(res));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<CotizacionResDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerCotizacion(CotizacionReqDto req)
        {
            if (req == null)
            {
                return BadRequest("El parámetro req es requerido.");
            }

            var res = _apiProductoFactServicio.ObtenerCotizacion(req);
            return Ok(new ApiResponse<List<CotizacionResDto>>(res));
        }
    }
}