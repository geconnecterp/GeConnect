using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Inventario;
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
	}
}
