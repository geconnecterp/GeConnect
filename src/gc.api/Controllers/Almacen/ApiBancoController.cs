using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
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
	public class ApiBancoController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IUriService _uriService;
		private readonly IBancoServicio _bancoServicio;
		private readonly ILogger<ApiSectorController> _logger;
		public ApiBancoController(IMapper mapper, IUriService uriService, ILogger<ApiSectorController> logger, ISectorServicio sectorServicio, IBancoServicio bancoServicio)
		{
			_mapper = mapper;
			_uriService = uriService;
			_logger = logger;
			_bancoServicio = bancoServicio;
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<BancoDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetBancoParaABM(string ctaf_id)
		{
			ApiResponse<List<BancoDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			var res = _bancoServicio.GetBancoParaABM(ctaf_id);

			response = new ApiResponse<List<BancoDto>>(res);

			return Ok(response);
		}
	}
}
