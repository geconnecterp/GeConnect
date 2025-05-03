using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;

namespace gc.api.Controllers.Codigos
{
	[Produces("application/json")]
	[Route("api/[controller]")]
	[ApiController]
	public class TipoComprobanteController : ControllerBase
	{
		private readonly IMapper _mapper;
		private ITiposComprobanteServicio _tipos_comprobanteSv;
		private readonly ILogger<TipoComprobanteController> _logger;

		public TipoComprobanteController(ITiposComprobanteServicio tiposComprobanteServicio, IMapper mapper, ILogger<TipoComprobanteController> logger)
		{
			_logger = logger;
			_mapper = mapper;
			_tipos_comprobanteSv=tiposComprobanteServicio;
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<TipoComprobanteDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetTipoComprobanteListaPorCuenta(string cuenta)
		{
			ApiResponse<List<TipoComprobanteDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _tipos_comprobanteSv.GetTipoComprobanteListaPorCuenta(cuenta);

			response = new ApiResponse<List<TipoComprobanteDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<TipoComprobanteDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetTipoComprobanteListaPorTipoAfip(string afip_id)
		{
			ApiResponse<List<TipoComprobanteDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _tipos_comprobanteSv.GetTipoComprobanteListaPorTipoAfip(afip_id);

			response = new ApiResponse<List<TipoComprobanteDto>>(res);

			return Ok(response);
		}
	}
}
