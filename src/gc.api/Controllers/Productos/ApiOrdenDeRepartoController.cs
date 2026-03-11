using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
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
	public class ApiOrdenDeRepartoController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly ILogger<ApiOrdenDeRepartoController> _logger;
		private readonly IApiOrdenDeRepartoServicio _orSrv;

		public ApiOrdenDeRepartoController(IMapper mapper, ILogger<ApiOrdenDeRepartoController> logger, IApiOrdenDeRepartoServicio servicio)
		{
			_mapper = mapper;
			_logger = logger;
			_orSrv = servicio;
		}

		[HttpGet("estados")]
		public IActionResult GetEstadosDeOrdenDeReparto()
		{
			const string msgError = "Error en la invocación de la API - Obtener Estados de Orden de Reparto";
			try
			{
				var estados = _orSrv.GetOrdenDeRepartoEstados();
				return Ok(new ApiResponse<List<OrdenDeRepartoEstadoDto>>(estados));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, msgError);
				return StatusCode(StatusCodes.Status500InternalServerError, new { error = true, msg = msgError });
			}
		}

		[HttpPost("buscar-ordenes-de-reparto")]
		public IActionResult BuscarOrdenesDeReparto(QueryFilters filtro)
		{
			const string msgError = "Error en la invocación de la API - Búsqueda de OR";
			try
			{
				if (filtro == null)
				{
					return BadRequest("No se recepcionó el filtro de la búsqueda de OR.");
				}

				var request = MapToRequest(filtro);
				var resultados = _orSrv.ObtenerListaOrdenDeReparto(request);

				var response = new ApiResponse<List<OrdenDeRepartoListaDto>>(resultados)
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

		// Obtiene el detalle de un presupuesto por id
		[HttpGet("buscar-pedidos-en-orden-de-reparto/{id}")]
		public IActionResult ObtenerPedidosEnOrdenDeReparto(string id)
		{
			const string msgError = "Error en la invocación de la API - Obtener Pedidos en Orden de Reparto";
			try
			{
				if (string.IsNullOrWhiteSpace(id))
				{
					return BadRequest("Debe indicar el identificador de la orden.");
				}

				var detalle = _orSrv.ObtenerPedidosEnOrdenDeReparto(id);
				return Ok(new ApiResponse<List<PedidoEnOrdenDeRepartoDto>>(detalle));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, msgError);
				return StatusCode(StatusCodes.Status500InternalServerError, new { error = true, msg = msgError });
			}
		}

		[HttpPost("orden-de-reparto/confirmar")]
		public IActionResult ConfirmarOrdenDeReparto(ConfirmaOrdenDeRepartoRequest req)
		{
			if (req == null)
			{
				return BadRequest("No se recepcionó la información para confirmar la orden de reparto.");
			}
			var respuesta = _orSrv.ConfirmarOrdenDeReparto(req);
			return Ok(new ApiResponse<RespuestaDto>(respuesta));
		}

		[HttpPost("analiza-aut-orden-de-reparto")]
		public IActionResult AnalizarAutOrdenDeReparto(AnalizarAutOrdenDeRepartoRequest request)
		{
			const string msgError = "Error en la invocación de la API - AnalizarAutOrdenDeReparto";
			try
			{
				if (Request == null)
					return BadRequest("No se recepcionó el filtro - AnalizarAutOrdenDeReparto");

				var resultados = _orSrv.AnalizarAutOrdenDeReparto(request);
				return Ok(new ApiResponse<List<AnalizarAutOrdenDeRepartoDto>>(resultados));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, msgError);
				return StatusCode(StatusCodes.Status500InternalServerError, new { error = true, msg = msgError });
			}
		}

		[HttpPost("aponer-en-curso-or")]
		public IActionResult APonerEnCursoOrdenDeReparto(APonerEnCursoOrdenDeRepartoRequest req)
		{
			if (req == null)
			{
				return BadRequest("No se recepcionó la información para analizar y poner en curso la orden de reparto.");
			}
			var respuesta = _orSrv.APonerEnCursoOrdenDeReparto(req);
			return Ok(new ApiResponse<RespuestaDto>(respuesta));
		}

		private static MetadataGrid? BuildMetadata(List<OrdenDeRepartoListaDto>? lista, QueryFilters filtro)
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

		private static OrdenDeRepartoRequest MapToRequest(QueryFilters filtro)
		{
			return new OrdenDeRepartoRequest
			{
				Registros = filtro.Registros ?? 0,
				Pagina = filtro.Pagina ?? 0,
				Desde = filtro.FechaD ?? DateTime.MinValue,
				Hasta = filtro.FechaH ?? DateTime.MaxValue,
				ore_list = ToCsv(filtro.Rel01),
				rp_list = ToCsv(filtro.Rel02),
			};
		}

		private static string? ToCsv(List<string>? values)
		{
			if (values == null || values.Count == 0) return null;
			return string.Join(",", values);
		}
	}
}
