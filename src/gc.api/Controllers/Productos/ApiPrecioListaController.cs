using gc.api.core.Contratos.Servicios.Ofertas;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Productos.Precio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;

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

        [HttpPost("ObtenerDetallePrecios")]
        public IActionResult GetDetalle(QueryFilters filters)
        {            
            var resultado = _plSv.ObtenerDetallePrecios(filters);

            return Ok(new ApiResponse<List<PrecioListaDetalleDto>>(resultado));
        }

		[HttpGet("ObtenerListaPreciosRubCta")]
		public IActionResult GetListaPreciosRubCta(string id)
		{
			var resultado = _plSv.ObtenerListaPreciosRubCta(id);

			return Ok(new ApiResponse<List<ListaPrecioRubCtaDto>>(resultado));
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult RegistrarModificacionesEnListaDePrecios(RegistrarModificacionesEnListaDePreciosRequest request)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _plSv.RegistrarModificacionesEnListaDePrecios(request);

			response = new ApiResponse<List<RespuestaDto>>(res);

			return Ok(response);
		}
	}
}
