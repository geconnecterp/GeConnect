using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Dtos.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<InventarioDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<InventarioDto> ObtenerInventarioLista(GetInventarioListaRequest req)
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

			return Ok(new ApiResponse<List<InventarioDto>>(resultado));
		}
	}
}
