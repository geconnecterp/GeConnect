using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
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
	public class ApiSectorController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IUriService _uriService;
		private readonly ISectorServicio _sectorServicio;
		private readonly ILogger<ApiSectorController> _logger;
		public ApiSectorController(IMapper mapper, IUriService uriService, ILogger<ApiSectorController> logger, ISectorServicio sectorServicio)
		{
			_mapper = mapper;
			_uriService = uriService;
			_logger = logger;
			_sectorServicio = sectorServicio;
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<SectorDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetRubro(string rub_id)
		{
			ApiResponse<List<RubroListaABMDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _sectorServicio.GetRubro(rub_id);

			response = new ApiResponse<List<RubroListaABMDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<SectorDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetRubroParaABM(string sec_id)
		{
			ApiResponse<List<RubroListaABMDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _sectorServicio.GetRubroParaABM(sec_id);

			response = new ApiResponse<List<RubroListaABMDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<SectorDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetSectorParaABM(string sec_id)
		{
			ApiResponse<List<SectorDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _sectorServicio.GetSectorParaABM(sec_id);

			response = new ApiResponse<List<SectorDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<SubSectorDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetSubSectorParaABM(string sec_id)
		{
			ApiResponse<List<SubSectorDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _sectorServicio.GetSubSectorParaABM(sec_id);

			response = new ApiResponse<List<SubSectorDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<SubSectorDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetSubSector(string rubg_id)
		{
			ApiResponse<List<SubSectorDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _sectorServicio.GetSubSector(rubg_id);

			response = new ApiResponse<List<SubSectorDto>>(res);

			return Ok(response);
		}
	}
}
