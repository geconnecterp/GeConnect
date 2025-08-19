using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;

namespace gc.api.Controllers.Financieros
{
	[Authorize]
	[Produces("application/json")]
	[Route("api/[controller]")]
	[ApiController]
	public class ApiFinancierosController : ControllerBase
	{
		private readonly ILogger<ApiFinancierosController> _logger;
		private readonly IFinancieroServicio _financieroServicio;
		public ApiFinancierosController(IMapper mapper, IUriService uriService, ILogger<ApiFinancierosController> logger, IFinancieroServicio financieroServicio)
		{
			_logger = logger;
			_financieroServicio = financieroServicio;
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult FinancieroConfirmarTransferencia([FromBody] ConfirmarTransferenciaRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.FinancieroConfirmarTransferencia(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroCuentaAlCobroRelaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetCuentaAlCobroRela(string ctaf_id)
		{
			ApiResponse<List<FinancieroCuentaAlCobroRelaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetCuentaAlCobroRela(ctaf_id);

			response = new ApiResponse<List<FinancieroCuentaAlCobroRelaDto>>(res);

			return Ok(response);
		}
	}
}
