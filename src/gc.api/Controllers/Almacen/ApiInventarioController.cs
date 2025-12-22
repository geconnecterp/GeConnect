using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
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
    }
}
