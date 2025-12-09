using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Discontinuo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Productos
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiDiscontinuoController : ControllerBase
    {
        private readonly IApiDiscontinuoServicio _servicio;

        public ApiDiscontinuoController(IApiDiscontinuoServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpPost("discontinuo-confirmar")]
        public IActionResult ConfirmarDiscontinuo([FromBody] AbmGenDto req)
        {
            if (req == null || string.IsNullOrEmpty(req.Json) || string.IsNullOrEmpty(req.Usuario)
                || string.IsNullOrEmpty(req.Administracion))
            {
                return BadRequest("Alguno de los parametros para la confirmacion del discontinuo ha faltado. Verifique");
            }
            var resultado = _servicio.ConfirmarDiscontinuo(req);

            if (resultado.resultado < 0)
            {
                return BadRequest(resultado);
            }

            return Ok(new ApiResponse<RespuestaDto>(resultado));
        }

        [HttpPost("discontinuo-productos")]
        public IActionResult ObtenerProductosDiscontinuos([FromBody] QueryFilters filters)
        {
            if (filters == null)
            {
                return BadRequest("Faltan los filtros para obtener los productos discontinuos. Verifique");
            }
            var datos = _servicio.ObtenerProductosDiscontinuos(filters);
            return Ok(new ApiResponse<List<DiscontinuoDetalleDto>>(datos));
        }
    }
}
