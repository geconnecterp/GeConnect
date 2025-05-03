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
    public class ABMProveedorController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        private readonly IABMProveedorServicio _abmProveedorServicio;
        public ABMProveedorController(IABMProveedorServicio abmProveedorServicio, IMapper mapper, IUriService uriService)
        {
			_abmProveedorServicio = abmProveedorServicio;
            _mapper = mapper;
            _uriService = uriService;
        }

        [HttpGet(Name = nameof(GetProveedores))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ABMProveedorSearchDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult GetProveedores([FromQuery] QueryFilters filters)
        {
			ABMProveedorSearchDto reg = new ABMProveedorSearchDto() { total_paginas = 0, total_registros = 0 };
            //_logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var clis = _abmProveedorServicio.Buscar(filters);
            if (clis.Count > 0)
            {
                reg = clis.First();
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
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetProveedores)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetProveedores)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<ABMProveedorSearchDto>>(clis)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ABMProveedorSearchDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult BuscarProveedores(QueryFilters filters)
        {
			ABMProveedorSearchDto reg = new() { total_paginas = 0, total_registros = 0 };
            var clis = _abmProveedorServicio.Buscar(filters);
            if (clis.Count > 0)
            {
                reg = clis.First();
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
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(BuscarProveedores)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(BuscarProveedores)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<ABMProveedorSearchDto>>(clis)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }
    }
}
