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
using System.Security.Claims;
using gc.infraestructura.EntidadesComunes.Options;

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
        private readonly IApiProductoServicio _productoServicio;

		public ApiInventarioController(IMapper mapper, IUriService uriService, ILogger<ApiInventarioController> logger,
									   IInventarioServicio inventarioServicio, IApiProductoServicio productoServicio)
		{
			_mapper = mapper;
			_uriService = uriService;
			_logger = logger;
			_inventarioServicio = inventarioServicio;
            _productoServicio = productoServicio;
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

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProveedorEnInventarioDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetProveedoresParaInventario(string inv_nro, string usu_id)
		{
			ApiResponse<List<ProveedorEnInventarioDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _inventarioServicio.GetProveedoresEnInventario(inv_nro, usu_id);

			response = new ApiResponse<List<ProveedorEnInventarioDto>>(res);

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
            var resultado = ValidarContextoPocket(request) ?? _inventarioServicio.ValidarConteo(request);
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
            var rechazo = ValidarContextoPocket(req) ?? _inventarioServicio.ValidarConteo(req);
            if (rechazo.resultado != 0) return BadRequest(rechazo.resultado_msj);
            req.p_id = "%";
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
            var rechazo = ValidarContextoPocket(request) ?? _inventarioServicio.ValidarConteo(request);
            if (rechazo.resultado != 0) return Ok(new ApiResponse<RespuestaDto>(rechazo));
            if (request.json == null || request.json.Count == 0)
                return Ok(new ApiResponse<RespuestaDto>(RechazoConteo("Debe agregar al menos un producto para confirmar el conteo.")));
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var fila in request.json)
            {
                if (fila == null || string.IsNullOrWhiteSpace(fila.p_id) || fila.p_id.Length > 10 || !ids.Add(fila.p_id))
                    return Ok(new ApiResponse<RespuestaDto>(RechazoConteo("El conteo contiene productos inválidos o repetidos.")));
                var producto = _productoServicio.ProductoBuscar(new BusquedaBase
                {
                    Busqueda = fila.p_id, Administracion = AdministracionPocket
                });
                if (producto == null || producto.P_id != fila.p_id || (producto.P_activo != "S" && producto.P_activo != "D"))
                    return Ok(new ApiResponse<RespuestaDto>(RechazoConteo($"El producto {fila.p_id} no existe o no está habilitado para contar.")));
                var errorCantidad = InventarioConteoReglas.ValidarCantidad(fila, producto.up_tipo);
                if (errorCantidad != null)
                    return Ok(new ApiResponse<RespuestaDto>(RechazoConteo($"{fila.p_id}: {errorCantidad}")));
                fila.up_id = producto.up_id;
                fila.up_tipo = producto.up_tipo;
                fila.p_desc = producto.P_desc;
                var validacion = _inventarioServicio.ValidarProductoConteo(new InventarioRequestDto
                {
                    inv_nro = request.inv_nro, usu_id = request.usu_id, p_id = fila.p_id
                });
                if (validacion.resultado != 0) return Ok(new ApiResponse<RespuestaDto>(validacion));
            }
            _logger.LogInformation("INV confirma snapshot. Inventario {Inventario}; tipo {Tipo}; contexto {Contexto}; usuario {Usuario}; productos {Cantidad}",
                request.inv_nro, request.tipo, request.tipo_id, request.usu_id, request.json.Count);
            var resultado = _inventarioServicio.InventarioConfirmarConteo(request);
            _logger.LogInformation("INV respuesta confirmación. Inventario {Inventario}; resultado {Resultado}; mensaje {Mensaje}",
                request.inv_nro, resultado?.resultado, resultado?.resultado_msj);
			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
            }
			return Ok(new ApiResponse<RespuestaDto>(resultado));
        }

        [HttpPost("VerificaProductoConteo")]
        public IActionResult VerificaProductoConteo([FromBody] InventarioRequestDto request)
        {
            if (request == null) return BadRequest("Parámetros inválidos.");
            var rechazo = ValidarContextoPocket(request);
            if (rechazo != null) return Ok(new ApiResponse<RespuestaDto>(rechazo));
            if (string.IsNullOrWhiteSpace(request.p_id) || request.p_id.Length > 10)
                return BadRequest("Producto inválido.");
            return Ok(new ApiResponse<RespuestaDto>(_inventarioServicio.ValidarProductoConteo(request)));
        }

        private string AdministracionPocket => User.FindFirst("AdmId")?.Value.Split('#')[0] ?? "";

        private static RespuestaDto RechazoConteo(string mensaje) => new() { resultado = 2, resultado_msj = mensaje };

        private RespuestaDto? ValidarContextoPocket(InventarioRequestDto request)
        {
            // El usuario y la sucursal nunca se toman del payload del navegador.
            request.usu_id = User.FindFirst("user")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var error = InventarioConteoReglas.ValidarContexto(request);
            if (error != null) return RechazoConteo(error);
            if (string.IsNullOrWhiteSpace(AdministracionPocket)) return RechazoConteo("No se pudo determinar la sucursal autenticada.");
            var permitidos = _inventarioServicio.GetInventarioLista(new GetInventarioListaRequest
            {
                desde = new DateTime(2020, 1, 1), hasta = DateTime.Today,
                adm_id = AdministracionPocket, usu_id = request.usu_id, inve_id = "S"
            });
            var inventario = permitidos?.FirstOrDefault(i => i.inv_nro == request.inv_nro);
            if (inventario == null) return RechazoConteo("El inventario ya no está disponible para el usuario y la sucursal.");
            if ((inventario.invt_id == 'B' ? 'B' : 'P') != request.tipo)
                return RechazoConteo("La modalidad no corresponde al inventario seleccionado.");
            return null;
        }

		[HttpPost("InventarioConfirmarModificacionDeConteo")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		public ActionResult<RespuestaDto> InventarioConfirmarModificacionDeConteo([FromBody] ConfirmarModificacionDeConteoRequest request)
		{
			if (request == null)
			{
				return BadRequest("Parametros del Conteo erroneos.");
			}
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var resultado = _inventarioServicio.InventarioConfirmarModificacionDeConteo(request);
			response = new ApiResponse<List<RespuestaDto>>(resultado);
			if (resultado == null)
			{
				return BadRequest("No se obtubieron resultados.");
			}
			return Ok(response);
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
