using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.CuentaComercial;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;

namespace gc.api.Controllers.Almacen
{
	[Authorize]
	[Produces("application/json")]
	[Route("api/[controller]")]
	[ApiController]
	public class ApiCuentaGastoController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IUriService _uriService;
		private readonly ICuentaGastoServicio _cuentaGastoServicio;
		private readonly ILogger<ApiSectorController> _logger;
		public ApiCuentaGastoController(IMapper mapper, IUriService uriService, ILogger<ApiSectorController> logger, ICuentaGastoServicio cuentaGastoServicio)
		{
			_mapper = mapper;
			_uriService = uriService;
			_logger = logger;
			_cuentaGastoServicio = cuentaGastoServicio;
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaGastoDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetCuentaGastoParaABM(string ctag_id)
		{
			ApiResponse<List<CuentaGastoDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _cuentaGastoServicio.GetCuentaGastoParaABM(ctag_id);

			response = new ApiResponse<List<CuentaGastoDto>>(res);

			return Ok(response);
		}
	}
}
