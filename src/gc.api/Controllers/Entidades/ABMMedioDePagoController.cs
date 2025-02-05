using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace gc.api.Controllers.Entidades
{
	[Produces("application/json")]
	[Route("api/[controller]")]
	[ApiController]
	public class ABMMedioDePagoController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IUriService _uriService;
		private readonly ILogger<ABMMedioDePagoController> _logger;
		private readonly IABMMedioDePagoServicio _abmMedioDePagoServicio;

		public ABMMedioDePagoController(IABMMedioDePagoServicio abmMedioDePagoServicio, IMapper mapper, IUriService uriService)
		{
			_abmMedioDePagoServicio = abmMedioDePagoServicio;
			_mapper = mapper;
			_uriService = uriService;
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ABMMedioDePagoSearchDto>>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult BuscarMediosDePago(QueryFilters filters)
		{
			ABMMedioDePagoSearchDto reg = new() { total_paginas = 0, total_registros = 0 };
			var clis = _abmMedioDePagoServicio.Buscar(filters);
			if (clis.Count > 0)
			{
				reg = clis.First();
			}

			// presentando en el header información basica sobre la paginación
			var metadata = new MetadataGrid
			{
				TotalCount = reg.total_registros,
				PageSize = filters.Registros.Value,
				CurrentPage = filters.Pagina.Value,
				TotalPages = reg.total_paginas,
				HasNextPage = filters.Pagina.Value < reg.total_paginas,
				HasPreviousPage = filters.Pagina.Value > 1,
				NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(BuscarMediosDePago)) ?? "").ToString(),
				PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(BuscarMediosDePago)) ?? "").ToString(),

			};

			var response = new ApiResponse<IEnumerable<ABMMedioDePagoSearchDto>>(clis)
			{
				Meta = metadata
			};

			Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

			return Ok(response);
		}
	}
}
