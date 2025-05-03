using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
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
	public class ApiMedioDePagoController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IUriService _uriService;
		private readonly IMedioDePagoServicio _medioDePagoServicio;
		private readonly ILogger<ApiMedioDePagoController> _logger;

		public ApiMedioDePagoController(IMedioDePagoServicio medioDePagoServicio, IMapper mapper, IUriService uriService, ILogger<ApiMedioDePagoController> logger)
		{
			_medioDePagoServicio = medioDePagoServicio;
			_mapper = mapper;
			_uriService = uriService;
			_logger = logger;
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<MedioDePagoABMDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetMedioDePagoParaABM(string ins_id)
		{
			ApiResponse<List<MedioDePagoABMDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _medioDePagoServicio.GetMedioDePagoParaABM(ins_id);

			response = new ApiResponse<List<MedioDePagoABMDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OpcionCuotaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetOpcionesDeCuotaParaABM(string ins_id)
		{
			ApiResponse<List<OpcionCuotaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _medioDePagoServicio.GetOpcionesDeCuotaParaABM(ins_id);

			response = new ApiResponse<List<OpcionCuotaDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OpcionCuotaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetOpcionDeCuotaParaABM(string ins_id, int cuota)
		{
			ApiResponse<List<OpcionCuotaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _medioDePagoServicio.GetOpcionDeCuotaParaABM(ins_id, cuota);

			response = new ApiResponse<List<OpcionCuotaDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetCuentaFinYContableLista(string ins_id)
		{
			ApiResponse<List<FinancieroListaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _medioDePagoServicio.GetCuentaFinYContableListaParaABM(ins_id);

			response = new ApiResponse<List<FinancieroListaDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetCuentaFinYContable(string ctaf_id)
		{
			ApiResponse<List<FinancieroListaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _medioDePagoServicio.GetCuentaFinYContableParaABM(ctaf_id);

			response = new ApiResponse<List<FinancieroListaDto>>(res);

			return Ok(response);
		}
		//
	}
}
