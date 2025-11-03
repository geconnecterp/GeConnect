using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Consultas.ReporteFinanciero;
using gc.infraestructura.Dtos.Consultas.ReporteFinanciero.Request;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
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

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroTraRepoDDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroTraRepoDDto(string tra_compte)
		{
			ApiResponse<List<FinancieroTraRepoDDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroTraRepoDDto(tra_compte);

			response = new ApiResponse<List<FinancieroTraRepoDDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroTraRepoCtagDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroTraRepoCtag(string tra_compte)
		{
			ApiResponse<List<FinancieroTraRepoCtagDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroTraRepoCtag(tra_compte);

			response = new ApiResponse<List<FinancieroTraRepoCtagDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<MovimientoFinancieroListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult BuscarMovimientoFinancieroReporte(ConsultaMovFinancierosRequest request)
		{
			ApiResponse<List<MovimientoFinancieroListaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.BuscarMovimientoFinancieroReporte(request);

			response = new ApiResponse<List<MovimientoFinancieroListaDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroBcoExtractoDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroBcoExtracto(FinancieroBcoExtractoRequest request)
		{
			ApiResponse<List<FinancieroBcoExtractoDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroBcoExtracto(request);

			response = new ApiResponse<List<FinancieroBcoExtractoDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroBcoCtaCteDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroBcoCtaCte(FinancieroBcoCtaCteRequest request)
		{
			ApiResponse<List<FinancieroBcoCtaCteDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroBcoCtaCte(request);

			response = new ApiResponse<List<FinancieroBcoCtaCteDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroBcoLibroResumenDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroBcoLibroResumen(FinancieroBcoLibroResumenRequest request)
		{
			ApiResponse<List<FinancieroBcoLibroResumenDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroBcoLibroResumen(request);

			response = new ApiResponse<List<FinancieroBcoLibroResumenDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroBcoLibroDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroBcoLibro(FinancieroBcoLibroRequest request)
		{
			ApiResponse<List<FinancieroBcoLibroDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroBcoLibro(request);

			response = new ApiResponse<List<FinancieroBcoLibroDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroBcoVencChequeEmitidoDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroBcoVencChequeEmitido(FinancieroBcoVencChequeEmitidoRequest request)
		{
			ApiResponse<List<FinancieroBcoVencChequeEmitidoDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroBcoVencChequeEmitido(request);

			response = new ApiResponse<List<FinancieroBcoVencChequeEmitidoDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroBcoVencChequeEmitidoListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroBcoVencChequeEmitidoLista(FinancieroBcoVencChequeEmitidoListaRequest request)
		{
			ApiResponse<List<FinancieroBcoVencChequeEmitidoListaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroBcoVencChequeEmitidoLista(request);

			response = new ApiResponse<List<FinancieroBcoVencChequeEmitidoListaDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ChequeModificadosListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetChequeModificadosLista(GetChequeModificadosListaRequest request)
		{
			ApiResponse<List<ChequeModificadosListaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetChequeModificadosLista(request);

			response = new ApiResponse<List<ChequeModificadosListaDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult SetChequeModificar([FromBody] GetChequeModificarListaRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.SetChequeModificar(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult SetFechaDeEntrega([FromBody] RegistrarFechaDeEntregaRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.SetFechaDeEntrega(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult SetRechazoDeCheque([FromBody] RegistrarRechazoDeChequeRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.SetRechazoDeCheque(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ECheqDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetECheqLista(PasoPrevioECheqRequest request)
		{
			ApiResponse<List<ECheqDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetECheqLista(request);

			response = new ApiResponse<List<ECheqDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult SetExtractoBancarioConfirmar([FromBody] SetExtractoBancarioConfirmaRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.SetExtractoBancarioConfirmar(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CrudExtractoBancarioDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetBcoExtractoDesdeFile(ExtractoBcoFileRequest request)
		{
			ApiResponse<List<CrudExtractoBancarioDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetBcoExtractoDesdeFile(request);

			response = new ApiResponse<List<CrudExtractoBancarioDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroConciliaDatosDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroConciliaDatos(FinancieroConciliaDatosRequest request)
		{
			ApiResponse<List<FinancieroConciliaDatosDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroConciliaDatos(request);

			response = new ApiResponse<List<FinancieroConciliaDatosDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroConciliaNroDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroConciliaNro(FinancieroConciliaNrosRequest request)
		{
			ApiResponse<List<FinancieroConciliaNroDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroConciliaNro(request);

			response = new ApiResponse<List<FinancieroConciliaNroDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult FinancieroExtractoDesconcilia([FromBody] FinancieroExtractoDesconciliaRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.FinancieroExtractoDesconcilia(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult FinancieroConciliacionExtractoConfirmar([FromBody] FinancieroConciliacionExtractoConfirmarRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.FinancieroConciliacionExtractoConfirmar(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<GastoProyListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetGastosProyLista()
		{
			ApiResponse<List<GastoProyListaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetGastosProyLista();

			response = new ApiResponse<List<GastoProyListaDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<GastoProyListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetGastosProyDatos(int items)
		{
			ApiResponse<List<GastoProyListaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetGastosProyDatos(items);

			response = new ApiResponse<List<GastoProyListaDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProyFinanDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetProyeccionFinanciera(BuscarProyFinanRequest request)
		{
			ApiResponse<List<ProyFinanDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetProyeccionFinanciera(request);

			response = new ApiResponse<List<ProyFinanDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<SaldoDeCuentaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetSaldoDeCuentas(BuscarSaldoDeCuentasRequest request)
		{
			ApiResponse<List<SaldoDeCuentaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetSaldoDeCuentas(request);

			response = new ApiResponse<List<SaldoDeCuentaDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FlujoDeIngresoDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFlujoDeIngreso(BuscarFlujoDeIngresoRequest request)
		{
			ApiResponse<List<FlujoDeIngresoDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFlujoDeIngreso(request);

			response = new ApiResponse<List<FlujoDeIngresoDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult FinancieroAnticipoEmpleadoConfirma([FromBody] CargaAnticipoEmpleadoRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.FinancieroAnticipoEmpleadoConfirma(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroTopeCtaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroTopePorCuenta(string cta_id)
		{
			ApiResponse<List<FinancieroTopeCtaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroTopePorCuenta(cta_id);

			response = new ApiResponse<List<FinancieroTopeCtaDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AnticipoDetalleDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetAnticipoDetalle(string an_compte)
		{
			ApiResponse<List<AnticipoDetalleDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetAnticipoDetalle(an_compte);

			response = new ApiResponse<List<AnticipoDetalleDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<FinancieroUsuarioDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFinancieroUsuarios(GetFinancieroUsuariosRequest request)
		{
			ApiResponse<List<FinancieroUsuarioDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.GetFinancieroUsuarios(request);

			response = new ApiResponse<List<FinancieroUsuarioDto>>(res);

			return Ok(response);
		}

		//[HttpPost]
		//[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AnticipoFinanEmpListaDto>))]
		//[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		//[Route("[action]")]
		//public IActionResult BuscarAnticipoFinancierosDeEmpleados(ConsultaAnticipoFinanEmpRequest request)
		//{
		//	ApiResponse<List<AnticipoFinanEmpListaDto>> response;
		//	_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
		//	var res = _financieroServicio.BuscarAnticipoFinancierosDeEmpleados(request);

		//	response = new ApiResponse<List<AnticipoFinanEmpListaDto>>(res);

		//	return Ok(response);
		//}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AnticipoFinanEmpListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult BuscarAnticipoFinancierosDeEmpleados(ConsultaAnticipoFinanEmpRequest request)
		{
			AnticipoFinanEmpListaDto reg = new() { total_paginas = 0, total_registros = 0 };

			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.BuscarAnticipoFinancierosDeEmpleados(request);

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
				NextPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(BuscarAnticipoFinancierosDeEmpleados)) ?? "").ToString(),
				PreviousPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(BuscarAnticipoFinancierosDeEmpleados)) ?? "").ToString(),

			};

			var response = new ApiResponse<IEnumerable<AnticipoFinanEmpListaDto>>(res)
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
		public IActionResult FinancieroAnticipoAnular([FromBody] FinancieroAnticipoAnularRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _financieroServicio.FinancieroAnticipoAnular(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}
	}
}
