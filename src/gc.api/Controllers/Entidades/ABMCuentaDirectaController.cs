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
	public class ABMCuentaDirectaController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IUriService _uriService;
		private readonly IABMCuentaDirectaServicio _aBMCuentaDirectaServicio;

		public ABMCuentaDirectaController(IMapper mapper, IUriService uriService, IABMBancoServicio aBMBancoServicio, IABMCuentaDirectaServicio aBMCuentaDirectaServicio)
		{
			_mapper = mapper;
			_uriService = uriService;
			_aBMCuentaDirectaServicio = aBMCuentaDirectaServicio;
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ABMCuentaDirectaSearchDto>>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult BuscarCuentasDirecta(QueryFilters filters)
		{
			ABMCuentaDirectaSearchDto reg = new() { total_paginas = 0, total_registros = 0 };

			var cd = _aBMCuentaDirectaServicio.Buscar(filters);
			if (cd.Count > 0)
			{
				reg = cd.First();
			}

			// presentando en el header información basica sobre la paginación
			var metadata = new MetadataGrid
			{
				TotalCount = reg.total_registros,
				PageSize = filters.Registros??0,
				CurrentPage = filters.Pagina??0,
				TotalPages = reg.total_paginas,
				HasNextPage =(filters.Pagina??0)< reg.total_paginas,
				HasPreviousPage =(filters.Pagina??0)> 1,
				NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(BuscarCuentasDirecta)) ?? "").ToString(),
				PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(BuscarCuentasDirecta)) ?? "").ToString(),

			};

			var response = new ApiResponse<IEnumerable<ABMCuentaDirectaSearchDto>>(cd)
			{
				Meta = metadata
			};

			Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

			return Ok(response);
		}
	}
}
