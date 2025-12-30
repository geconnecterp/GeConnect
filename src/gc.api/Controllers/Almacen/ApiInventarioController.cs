using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Dtos.Inventario.Dto;
using gc.infraestructura.Dtos.Inventario.Request;
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
	public class ApiInventarioController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IUriService _uriService;
		private readonly ILogger<ApiInventarioController> _logger;
		private readonly IInventarioServicio _inventarioServicio;

		public ApiInventarioController(IMapper mapper, IUriService uriService, ILogger<ApiInventarioController> logger,
									   IInventarioServicio inventarioServicio)
		{
			_mapper = mapper;
			_uriService = uriService;
			_logger = logger;
			_inventarioServicio = inventarioServicio;
		}

		[HttpPost("ObtenerInventarioLista")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<InventarioListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<InventarioListaDto> ObtenerInventarioLista(GetInventarioListaRequest req)
		{
			if (req == null)
			{
				return BadRequest("Request nulo.");
			}
			var resultado = _inventarioServicio.GetInventarioLista(req);

			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
			}

			return Ok(new ApiResponse<List<InventarioListaDto>>(resultado));
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RubroEnInventarioDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetRubroParaInventario(string inv_nro, string usu_id)
		{
			ApiResponse<List<RubroEnInventarioDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _inventarioServicio.GetRubrosEnInventario(inv_nro, usu_id);

			response = new ApiResponse<List<RubroEnInventarioDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<UsuarioEnInventarioDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetUsuariosParaInventario(string inv_nro)
		{
			ApiResponse<List<UsuarioEnInventarioDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _inventarioServicio.GetUSuariosEnInventario(inv_nro);

			response = new ApiResponse<List<UsuarioEnInventarioDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ConfirmarInventario([FromBody] ConfirmarInventarioRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _inventarioServicio.ConfirmarInventario(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<InventarioBoxDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetInventarioBox([FromBody] InventarioRequestDto req)
		{
			//validamos los parametros
			if(req== null || string.IsNullOrEmpty(req.inv_nro) || string.IsNullOrEmpty(req.usu_id))
			{
				return BadRequest("Parámetros inválidos.");
            }
			ApiResponse<List<InventarioBoxDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _inventarioServicio.GetInventarioBox(req.inv_nro, req.usu_id);
			response = new ApiResponse<List<InventarioBoxDto>>(res);
			return Ok(response);
        }

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<InventarioPlanillaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetInventarioPlanilla([FromBody] InventarioRequestDto req)
		{
            //validamos los parametros
			if (req == null || string.IsNullOrEmpty(req.inv_nro) || string.IsNullOrEmpty(req.usu_id))
			{
				return BadRequest("Parámetros inválidos.");
            }
			ApiResponse<List<InventarioPlanillaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _inventarioServicio.GetInventarioPlanilla(req.inv_nro, req.usu_id);
			response = new ApiResponse<List<InventarioPlanillaDto>>(res);
			return Ok(response);
        }

		[HttpPost("ObtenerInventarioDatos")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<InventarioListaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<InventarioListaDto> ObtenerInventarioDatos(GetInventarioDatosRequest req)
		{
			if (req == null)
			{
				return BadRequest("Request nulo.");
			}
			var resultado = _inventarioServicio.GetInventarioDatos(req);

			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
			}

			return Ok(new ApiResponse<List<InventarioListaDto>>(resultado));
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult RegistrarControlDeStock([FromBody] RegistrarStockDeControlRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _inventarioServicio.RegistrarControlDeStock(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}

		[HttpPost("ObtenerProductosEnValorizacion")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductosEnValorizacionDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<ProductosEnValorizacionDto> GetProductosEnValorizacion(ProductosEnValorizacionRequest req)
		{
			if (req == null)
			{
				return BadRequest("Request nulo.");
			}
			var resultado = _inventarioServicio.GetProductosEnValorizacion(req);

			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
			}

			return Ok(new ApiResponse<List<ProductosEnValorizacionDto>>(resultado));
		}

		[HttpPost("ObtenerConteosEnValorizacion")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ConteoEnValorizacionDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<ConteoEnValorizacionDto> GetConteosEnValorizacion(ConteosEnValorizacionRequest req)
		{
			if (req == null)
			{
				return BadRequest("Request nulo.");
			}
			var resultado = _inventarioServicio.GetConteoEnValorizacion(req);

			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
			}

			return Ok(new ApiResponse<List<ConteoEnValorizacionDto>>(resultado));
		}

		[HttpPost("VerificaConteo")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<RespuestaDto> VerificaConteo([FromBody] InventarioRequestDto request)
		{
			if(request == null)
			{
				return BadRequest("Parametros del Conteo erroneos.");
            }
			var resultado = _inventarioServicio.ValidarConteo(request);
			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
            }
			return Ok(new ApiResponse<RespuestaDto>(resultado));
        }

        [HttpPost("ObtenerConteos")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<InventarioConteoDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<InventarioConteoDto> ObtenerConteos([FromBody] InventarioRequestDto req)
		{
			if (req == null)
			{
				return BadRequest("Parametros del Conteo erroneos.");
            }
			var resultado = _inventarioServicio.GetInventarioConteo(req);
			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
            }
			return Ok(new ApiResponse<List<InventarioConteoDto>>(resultado));

        }

		[HttpPost("ConfirmarConteo")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<RespuestaDto> ConfirmarConteo([FromBody] InventarioRequestDto request)
		{
			if (request == null)
			{
				return BadRequest("Parametros del Conteo erroneos.");
            }
			var resultado = _inventarioServicio.InventarioConfirmarConteo(request);
			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
            }
			return Ok(new ApiResponse<RespuestaDto>(resultado));
        }

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult RegistrarValorizacion([FromBody] RegistrarValorizacionRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _inventarioServicio.RegistrarValorizacion(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}

		[HttpPost("ObtenerProductosEnCierre")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoEnCierreDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<ProductoEnCierreDto> GetProductosEnCierre(ProductosEnCierreRequest req)
		{
			if (req == null)
			{
				return BadRequest("Request nulo.");
			}
			var resultado = _inventarioServicio.GetProductosEnCierre(req);

			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
			}

			return Ok(new ApiResponse<List<ProductoEnCierreDto>>(resultado));
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult RegistrarCierre([FromBody] RegistrarCierreRequest r)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _inventarioServicio.RegistrarCierre(r);
			response = new ApiResponse<List<RespuestaDto>>(res);
			return Ok(response);
		}

		[HttpPost("GetReporteStockVsConteo")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<InvRepoStkVsConteoDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<InvRepoStkVsConteoDto> GetReporteStockVsConteo(ReporteInventarioRequest req)
		{
			if (req == null)
			{
				return BadRequest("Request nulo.");
			}
			var resultado = _inventarioServicio.GetReporteStockVsConteo(req);

			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
			}

			return Ok(new ApiResponse<List<InvRepoStkVsConteoDto>>(resultado));
		}

		[HttpPost("GetReporteValorizacionPorSector")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<InvRepoValPorSecDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<InvRepoValPorSecDto> GetReporteValorizacionPorSector(ReporteInventarioRequest req)
		{
			if (req == null)
			{
				return BadRequest("Request nulo.");
			}
			var resultado = _inventarioServicio.GetReporteValorizacionPorSector(req);

			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
			}

			return Ok(new ApiResponse<List<InvRepoValPorSecDto>>(resultado));
		}

		[HttpPost("GetReporteValorizacionPorRubro")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<InvRepoValPorRubDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<InvRepoValPorRubDto> GetReporteValorizacionPorRubro(ReporteInventarioRequest req)
		{
			if (req == null)
			{
				return BadRequest("Request nulo.");
			}
			var resultado = _inventarioServicio.GetReporteValorizacionPorRubro(req);

			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
			}

			return Ok(new ApiResponse<List<InvRepoValPorRubDto>>(resultado));
		}

		[HttpPost("GetReporteValorizadoDetalle")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<InvRepoValorDetalleDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<InvRepoValorDetalleDto> GetReporteValorizadoDetalle(ReporteInventarioRequest req)
		{
			if (req == null)
			{
				return BadRequest("Request nulo.");
			}
			var resultado = _inventarioServicio.GetReporteValorizadoDetalle(req);

			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
			}

			return Ok(new ApiResponse<List<InvRepoValorDetalleDto>>(resultado));
		}

		[HttpPost("GetReporteConteosPorUsu")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<InvRepoConteosPorUsuDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<InvRepoConteosPorUsuDto> GetReporteConteosPorUsu(ReporteInventarioRequest req)
		{
			if (req == null)
			{
				return BadRequest("Request nulo.");
			}
			var resultado = _inventarioServicio.GetReporteConteosPorUsu(req);

			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
			}

			return Ok(new ApiResponse<List<InvRepoConteosPorUsuDto>>(resultado));
		}
	}
}
