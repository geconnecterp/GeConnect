using gc.api.core.Contratos.Servicios.Ofertas;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Productos.Precio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Ofertas
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiPrecioListaController : ControllerBase
    {
        private readonly ILogger<ApiPrecioListaController> _logger;
        private readonly IApiPrecioListaServicio _plSv;
        public ApiPrecioListaController(ILogger<ApiPrecioListaController> logger, IApiPrecioListaServicio servicio)
        {
            _plSv = servicio;
            _logger = logger;
        }
        [HttpGet("ObtenerListaPrecios")]
        public IActionResult Get()
        {
            var resultado = _plSv.ObtenerListaPrecios();

            return Ok(new ApiResponse<List<PrecioListaDto>>(resultado));
        }

    }
}
