using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.Dtos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
		private readonly IUriService _uriService;
		private readonly IFinancieroServicio _financieroServicio;
		public ApiFinancierosController(IMapper mapper, IUriService uriService, ILogger<ApiFinancierosController> logger, IFinancieroServicio financieroServicio)
		{
			_logger = logger;
			_financieroServicio = financieroServicio;
			_uriService = uriService;
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

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroChequeDepositadoDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroChequeDepositado([FromBody] FinancieroChequeDepositadoRequest r)
		{
			ApiResponse<List<FinancieroChequeDepositadoDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroChequeDepositado(r);
			response = new ApiResponse<List<FinancieroChequeDepositadoDto>>(res);
			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<PerfilUserDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroTraUsu(FinancieroTraUsuRequest request)
		{
			ApiResponse<List<PerfilUserDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroTraUsu(request);

			response = new ApiResponse<List<PerfilUserDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<MovimientoFinancieroListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult BuscarMovimientoFinanciero(ConsultaMovFinancierosRequest request)
		{
			MovimientoFinancieroListaDto reg = new() { total_paginas = 0, total_registros = 0 };

			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.BuscarMovimientoFinanciero(request);

			if (res.Count > 0)
				reg = res.First();

			var metadata = new MetadataGrid
			{
				TotalCount = reg.total_registros,
				PageSize = request.Registros ?? 0,
				CurrentPage = request.Pagina ?? 0,
				TotalPages = reg.total_paginas,
				HasNextPage = (request.Pagina ?? 0) < reg.total_paginas,
				HasPreviousPage = (request.Pagina ?? 0) > 1,
				NextPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(BuscarMovimientoFinanciero)) ?? "").ToString(),
				PreviousPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(BuscarMovimientoFinanciero)) ?? "").ToString(),

			};

			var response = new ApiResponse<IEnumerable<MovimientoFinancieroListaDto>>(res)
			{
				Meta = metadata
			};
			Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult MovimientoFinancieroAnular([FromBody] MovimientoFinancieroAnularRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.MovimientoFinancieroAnular(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}
	}
}
