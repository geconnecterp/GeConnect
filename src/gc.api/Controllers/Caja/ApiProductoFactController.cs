using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Cajas.Request;
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
    }
}
