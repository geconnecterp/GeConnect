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
	public class ABMBancoController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly IUriService _uriService;
		private readonly IABMBancoServicio _aBMBancoServicio;

		public ABMBancoController(IMapper mapper, IUriService uriService, IABMBancoServicio aBMBancoServicio)
		{
			_mapper = mapper;
			_uriService = uriService;
			_aBMBancoServicio = aBMBancoServicio;
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ABMSectorSearchDto>>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult BuscarBancos(QueryFilters filters)
		{
			ABMBancoSearchDto reg = new() { total_paginas = 0, total_registros = 0 };

			var bancos = _aBMBancoServicio.Buscar(filters);
			if (bancos.Count > 0)
			{
				reg = bancos.First();
			}

			// presentando en el header información basica sobre la paginación
			var metadata = new MetadataGrid
			{
				TotalCount = reg.total_registros,
				PageSize = filters.Registros??0,
				CurrentPage = filters.Pagina ?? 0,
				TotalPages = reg.total_paginas,
				HasNextPage = filters.Pagina < reg.total_paginas,
				HasPreviousPage = filters.Pagina > 1,
				NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(BuscarBancos)) ?? "").ToString(),
				PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(BuscarBancos)) ?? "").ToString(),

			};

			var response = new ApiResponse<IEnumerable<ABMBancoSearchDto>>(bancos)
			{
				Meta = metadata
			};

			Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

			return Ok(response);
		}
	}
}
