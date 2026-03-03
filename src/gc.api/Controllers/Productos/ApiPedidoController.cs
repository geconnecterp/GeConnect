using gc.api.Controllers.Ofertas;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Productos
{
	[Authorize]
	[Produces("application/json")]
	[Route("api/[controller]")]
	[ApiController]
	public class ApiPedidoController : ControllerBase
	{
		private readonly ILogger<ApiPedidoController> _logger;
		private readonly IApiPedidoServicio _pedidoSrv;
		public ApiPedidoController(ILogger<ApiPedidoController> logger, IApiPedidoServicio servicio)
		{
			_logger = logger;
			_pedidoSrv = servicio;
		}

		// Buscar lista paginada de pedidos, devolviendo ApiResponse con Metadata
		[HttpPost("buscar-pedidos")]
		public IActionResult BuscarPedidos(QueryFilters filtro)
		{
			const string msgError = "Error en la invocación de la API - Búsqueda de Pedidos de Cliente";
			try
			{
				if (filtro == null)
				{
					return BadRequest("No se recepcionó el filtro de la búsqueda de Pedidos de Cliente.");
				}

				var request = MapToRequest(filtro);
				var resultados = _pedidoSrv.ObtenerListaPedidos(request);

				var response = new ApiResponse<List<PedidoListDto>>(resultados)
				{
					Meta = BuildMetadata(resultados, filtro)
				};

				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, msgError);
				return StatusCode(StatusCodes.Status500InternalServerError, new { error = true, msg = msgError });
			}
		}

		// Obtiene datos de un pedido por id
		[HttpGet("pedido/{id}")]
		public IActionResult ObtenerPedido(string id)
		{
			const string msgError = "Error en la invocación de la API - Obtener Pedido de Cliente";
			try
			{
				if (string.IsNullOrWhiteSpace(id))
				{
					return BadRequest("Debe indicar el identificador del pedido.");
				}

				var datos = _pedidoSrv.ObtenerPedido(id);
				return Ok(new ApiResponse<List<PedidoDto>>(datos));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, msgError);
				return StatusCode(StatusCodes.Status500InternalServerError, new { error = true, msg = msgError });
			}
		}

		// Obtiene el detalle de un presupuesto por id
		[HttpGet("pedido/detalle/{id}")]
		public IActionResult ObtenerDetalleDePedido(string id)
		{
			const string msgError = "Error en la invocación de la API - Obtener Detalle de Pedido";
			try
			{
				if (string.IsNullOrWhiteSpace(id))
				{
					return BadRequest("Debe indicar el identificador del pedido.");
				}

				var detalle = _pedidoSrv.ObtenerDetalleDePedido(id);
				return Ok(new ApiResponse<List<PedidoProductoDto>>(detalle));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, msgError);
				return StatusCode(StatusCodes.Status500InternalServerError, new { error = true, msg = msgError });
			}
		}

		[HttpPost("pedido/confirmar")]
		public IActionResult ConfirmarPedido(ConfirmarPedidoRequest req)
		{
			if (req == null)
			{
				return BadRequest("No se recepcionó la información para confirmar el pedido.");
			}
			var respuesta = _pedidoSrv.ConfirmarPedido(req);
			return Ok(new ApiResponse<RespuestaDto>(respuesta));
		}

		// Construye metadata del grid en base al primer elemento (evita recorrer la colección)
		private static MetadataGrid? BuildMetadata(List<PedidoListDto>? lista, QueryFilters filtro)
		{
			if (lista == null || lista.Count == 0)
			{
				return new MetadataGrid
				{
					TotalCount = 0,
					PageSize = filtro.Registros ?? 0,
					CurrentPage = filtro.Pagina ?? 0,
					TotalPages = 0,
					HasNextPage = false,
					HasPreviousPage = false,
					NextPageUrl = null,
					PreviousPageUrl = null
				};
			}

			var reg = lista[0];
			var pageSize = filtro.Registros ?? 0;
			var currentPage = filtro.Pagina ?? 0;
			var totalCount = reg.Total_registros;
			var totalPages = reg.Total_paginas;

			return new MetadataGrid
			{
				TotalCount = totalCount,
				PageSize = pageSize,
				CurrentPage = currentPage,
				TotalPages = totalPages,
				HasNextPage = currentPage < totalPages,
				HasPreviousPage = currentPage > 1,
				NextPageUrl = null,
				PreviousPageUrl = null
			};
		}

		// Mapea filtros a request del SP (minimizando asignaciones innecesarias)
		private static PedidoRequest MapToRequest(QueryFilters filtro)
		{
			return new PedidoRequest
			{
				Registros = filtro.Registros ?? 0,
				Pagina = filtro.Pagina ?? 0,
				Desde = filtro.FechaD ?? DateTime.MinValue,
				Hasta = filtro.FechaH ?? DateTime.MaxValue,
				cli_list = ToCsv(filtro.Rel01),
				pce_list = ToCsv(filtro.Rel02),
				ve_list = filtro.Rel03 != null && filtro.Rel03.Count > 0 ? string.Join(",", filtro.Rel03.Select(x => x.Id)) : null,
				rp_list = filtro.Rel04 != null && filtro.Rel04.Count > 0 ? string.Join(",", filtro.Rel04.Select(x => x.Id)) : null
			};
		}

		private static string? ToCsv(List<string>? values)
		{
			if (values == null || values.Count == 0) return null;
			return string.Join(",", values);
		}
	}
}
