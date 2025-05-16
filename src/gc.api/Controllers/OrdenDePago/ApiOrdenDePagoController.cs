using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;

namespace gc.api.Controllers.OrdenDePago
{
	[Authorize]
	[Produces("application/json")]
	[Route("api/[controller]")]
	[ApiController]
	public class ApiOrdenDePagoController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IUriService _uriService;
		private readonly IOrdenDePagoServicio _ordenDePagoServicio;
		private readonly ILogger<ApiOrdenDePagoController> _logger;

		public ApiOrdenDePagoController(IOrdenDePagoServicio ordenDePagoServicio, IMapper mapper, IUriService uriService, ILogger<ApiOrdenDePagoController> logger)
		{
			_mapper = mapper;
			_uriService = uriService;
			_logger = logger;
			_ordenDePagoServicio = ordenDePagoServicio;
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OPValidacionPrevDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetOPValidacionesPrev(string cta_id)
		{
			ApiResponse<List<OPValidacionPrevDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _ordenDePagoServicio.GetOPValidacionesPrev(cta_id);

			response = new ApiResponse<List<OPValidacionPrevDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OPDebitoYCreditoDelProveedorDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetOPDebitoYCreditoDelProveedor(string cta_id, char tipo, bool excluye_notas, string admId, string usuId)
		{
			ApiResponse<List<OPDebitoYCreditoDelProveedorDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _ordenDePagoServicio.GetOPDebitoYCreditoDelProveedor(cta_id, tipo, excluye_notas, admId, usuId);

			response = new ApiResponse<List<OPDebitoYCreditoDelProveedorDto>>(res);

			return Ok(response);
		}
	}
}
