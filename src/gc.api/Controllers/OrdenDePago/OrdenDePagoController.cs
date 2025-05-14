using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
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
	public class OrdenDePagoController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IUriService _uriService;
		private readonly IOrdenDePagoServicio _ordenDePagoServicio;
		private readonly ILogger<OrdenDePagoController> _logger;

		public OrdenDePagoController(IOrdenDePagoServicio ordenDePagoServicio, IMapper mapper, IUriService uriService, ILogger<OrdenDePagoController> logger)
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
	}
}
