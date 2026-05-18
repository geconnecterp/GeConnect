using gc.api.Controllers.OrdenReparto;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenReparto;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Dtos.Ventas.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;

namespace gc.api.Controllers.Ventas
{
	[Authorize]
	[Produces("application/json")]
	[Route("api/[controller]")]
	[ApiController]
	public class ApiVentasController : ControllerBase
	{
		private readonly ILogger<ApiVentasController> _logger;
		private readonly IApiVentasServicio _iApiVentasServicio;
		public ApiVentasController(ILogger<ApiVentasController> logger, IApiVentasServicio iApiVentasServicio)
		{
			_logger = logger;
			_iApiVentasServicio = iApiVentasServicio;
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<VtasPVCtlProcesoDto>>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerVtasPVCtlProcesosLista(string adm_id)
		{

			if (string.IsNullOrWhiteSpace(adm_id))
			{
				_logger.LogWarning("Parámetro adm_id se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro rubro no puede estar vacío."));
			}

			var data = _iApiVentasServicio.ObtenerVtasPVCtlProcesosLista(adm_id);

			return Ok(new ApiResponse<List<VtasPVCtlProcesoDto>>(data));

		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<VtasPVCtlCierresDto>>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerVtasPVCtlCierresLista(string caja_nro_proceso)
		{

			if (string.IsNullOrWhiteSpace(caja_nro_proceso))
			{
				_logger.LogWarning("Parámetro caja_nro_proceso se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro caja_nro_proceso no puede estar vacío."));
			}

			var data = _iApiVentasServicio.ObtenerVtasPVCtlCierresLista(caja_nro_proceso);

			return Ok(new ApiResponse<List<VtasPVCtlCierresDto>>(data));

		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<VtasPVCtlRendDto>>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerVtasPVCtlRendLista(string caja_nro_proceso, int caja_nro_cierre)
		{

			if (string.IsNullOrWhiteSpace(caja_nro_proceso))
			{
				_logger.LogWarning("Parámetro caja_nro_proceso se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro caja_nro_proceso no puede estar vacío."));
			}
			if (caja_nro_cierre <= 0)
			{
				_logger.LogWarning("Parámetro caja_nro_cierre se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro caja_nro_cierre no puede estar vacío."));
			}

			var data = _iApiVentasServicio.ObtenerVtasPVCtlRendLista(caja_nro_proceso, caja_nro_cierre);

			return Ok(new ApiResponse<List<VtasPVCtlRendDto>>(data));

		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<VtasPVCtlRendDetalleDto>>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerVtasPVCtlRendDetalleLista(string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, string tcf_id)
		{

			if (string.IsNullOrWhiteSpace(caja_nro_proceso))
			{
				_logger.LogWarning("Parámetro caja_nro_proceso se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro caja_nro_proceso no puede estar vacío."));
			}
			if (caja_nro_cierre <= 0)
			{
				_logger.LogWarning("Parámetro caja_nro_cierre se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro caja_nro_cierre no puede estar vacío."));
			}

			var data = _iApiVentasServicio.ObtenerVtasPVCtlRendDetalleLista(caja_nro_proceso, caja_nro_cierre, caja_nro_rend, tcf_id);

			return Ok(new ApiResponse<List<VtasPVCtlRendDetalleDto>>(data));

		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult CargaCtlNuevoItemDetalle(CargaCtlNuevoItemDetalleRequest request)
		{
			if (request == null)
			{
				_logger.LogWarning("Parámetro request se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro request no puede estar vacío."));
			}
			var data = _iApiVentasServicio.CargaCtlNuevoItemDetalle(request);
			return Ok(new ApiResponse<RespuestaDto>(data));
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GuardarCtlDetalle(GuardarCtlDetalleRequest request)
		{
			if (request == null)
			{
				_logger.LogWarning("Parámetro request se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro request no puede estar vacío."));
			}
			var data = _iApiVentasServicio.GuardarCtlDetalle(request);
			return Ok(new ApiResponse<RespuestaDto>(data));
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ConfirmarCtlArqueo(ConfirmarCtlArqueoRequest request)
		{
			if (request == null)
			{
				_logger.LogWarning("Parámetro request se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro request no puede estar vacío."));
			}
			var data = _iApiVentasServicio.ConfirmarCtlArqueo(request);
			return Ok(new ApiResponse<RespuestaDto>(data));
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult AnularCtlArqueo(AnularCtlArqueoRequest request)
		{
			if (request == null)
			{
				_logger.LogWarning("Parámetro request se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro request no puede estar vacío."));
			}
			var data = _iApiVentasServicio.AnularCtlArqueo(request);
			return Ok(new ApiResponse<RespuestaDto>(data));
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult AgregarMedioDePago(AgregarMedioDePagoRequest request)
		{
			if (request == null)
			{
				_logger.LogWarning("Parámetro request se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro request no puede estar vacío."));
			}
			var data = _iApiVentasServicio.AgregarMedioDePago(request);
			return Ok(new ApiResponse<RespuestaDto>(data));
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ConfirmacionContable(ConfirmacionContableRequest request)
		{
			if (request == null)
			{
				_logger.LogWarning("Parámetro request se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro request no puede estar vacío."));
			}
			var data = _iApiVentasServicio.ConfirmacionContable(request);
			return Ok(new ApiResponse<RespuestaDto>(data));
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<VtasPVCtlEntregaDto>>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerVtasPVCtlEntregaLista(string adm_id, char estado)
		{

			if (string.IsNullOrWhiteSpace(adm_id))
			{
				_logger.LogWarning("Parámetro adm_id se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro adm_id no puede estar vacío."));
			}
			if (estado == '\0')
			{
				_logger.LogWarning("Parámetro estado se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro estado no puede estar vacío."));
			}

			var data = _iApiVentasServicio.ObtenerVtasPVCtlEntregaLista(adm_id, estado);
			return Ok(new ApiResponse<List<VtasPVCtlEntregaDto>>(data));

		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<VtasPVCtlEntregaRendDto>>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerVtasPVCtlEntregaRendLista(string ent_compte)
		{

			if (string.IsNullOrWhiteSpace(ent_compte))
			{
				_logger.LogWarning("Parámetro ent_compte se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro ent_compte no puede estar vacío."));
			}

			var data = _iApiVentasServicio.ObtenerVtasPVCtlEntregaRendLista(ent_compte);
			return Ok(new ApiResponse<List<VtasPVCtlEntregaRendDto>>(data));

		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ConfirmarCtlEntrega(ConfirmarCtlEntregaRequest request)
		{
			if (request == null)
			{
				_logger.LogWarning("Parámetro request se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro request no puede estar vacío."));
			}
			var data = _iApiVentasServicio.ConfirmarCtlEntrega(request);
			return Ok(new ApiResponse<RespuestaDto>(data));
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult AnularCtlEntrega(AnularCtlEntregaRequest request)
		{
			if (request == null)
			{
				_logger.LogWarning("Parámetro request se encuentra vacío o nulo.");
				return BadRequest(new ApiResponse<string>("El parámetro request no puede estar vacío."));
			}
			var data = _iApiVentasServicio.AnularCtlEntrega(request);
			return Ok(new ApiResponse<RespuestaDto>(data));
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AnaVtaMesDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerAnaVtaMesLista(AnaVtaMesRequest request)
		{
			ApiResponse<List<AnaVtaMesDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _iApiVentasServicio.ObtenerAnaVtaMesLista(request);

			response = new ApiResponse<List<AnaVtaMesDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AnaVtaMesDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerAnaVtaMesDetalleDiaLista(AnaVtaMesRequest request)
		{
			ApiResponse<List<AnaVtaMesDetalleDiarioDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _iApiVentasServicio.ObtenerAnaVtaMesDetalleDiaLista(request);

			response = new ApiResponse<List<AnaVtaMesDetalleDiarioDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AnaVtaMesDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerAnaVtaMesDetalleHoraLista(AnaVtaMesRequest request)
		{
			ApiResponse<List<AnaVtaMesDetalleHoraDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _iApiVentasServicio.ObtenerAnaVtaMesDetalleHoraLista(request);
			response = new ApiResponse<List<AnaVtaMesDetalleHoraDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AnaVtaMesDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerAnaVtaMesDetalleSucursalLista(AnaVtaMesRequest request)
		{
			ApiResponse<List<AnaVtaMesDetalleSucursalDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _iApiVentasServicio.ObtenerAnaVtaMesDetalleSucursalLista(request);
			response = new ApiResponse<List<AnaVtaMesDetalleSucursalDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AnaVtaMesDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerAnaVtaMesDetalleAnualLista(AnaVtaMesRequest request)
		{
			ApiResponse<List<AnaVtaMesDetalleAnualDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _iApiVentasServicio.ObtenerAnaVtaMesDetalleAnualLista(request);
			response = new ApiResponse<List<AnaVtaMesDetalleAnualDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AnaVtaMesDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerAnaVtaMesDetalleCierreLista(AnaVtaMesRequest request)
		{
			ApiResponse<List<AnaVtaMesDetalleCierreDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _iApiVentasServicio.ObtenerAnaVtaMesDetalleCierreLista(request);
			response = new ApiResponse<List<AnaVtaMesDetalleCierreDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AnaValDeVtaMesDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerAnaDeValDeVtaMesLista(AnaDeValDeVtaMesRequest request)
		{
			ApiResponse<List<AnaValDeVtaMesDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _iApiVentasServicio.ObtenerAnaDeValDeVtaMesLista(request);
			response = new ApiResponse<List<AnaValDeVtaMesDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AnaValDeVtaDetDiarioDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerAnaDeValDeVtaDetDiarioLista(AnaDeValDeVtaMesRequest request)
		{
			ApiResponse<List<AnaValDeVtaDetDiarioDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _iApiVentasServicio.ObtenerAnaDeValDeVtaDetDiarioLista(request);
			response = new ApiResponse<List<AnaValDeVtaDetDiarioDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AnaValDeVtaDetPVDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerAnaDeValDeVtaDetPVLista(AnaDeValDeVtaMesRequest request)
		{
			ApiResponse<List<AnaValDeVtaDetPVDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _iApiVentasServicio.ObtenerAnaDeValDeVtaDetPVLista(request);
			response = new ApiResponse<List<AnaValDeVtaDetPVDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AnaValDeVtaDetCBDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ObtenerAnaDeValDeVtaDetCBLista(AnaDeValDeVtaMesRequest request)
		{
			ApiResponse<List<AnaValDeVtaDetCBDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _iApiVentasServicio.ObtenerAnaDeValDeVtaDetCBLista(request);
			response = new ApiResponse<List<AnaValDeVtaDetCBDto>>(res);

			return Ok(response);
		}
	}
}
