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
    public class ABMSectorController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        private readonly IABMSectorServicio _abmSecServicio;

        public ABMSectorController(IABMSectorServicio abmSectorServicio, IMapper mapper, IUriService uriService)
        {
            _abmSecServicio = abmSectorServicio;
            _mapper = mapper;
            _uriService = uriService;
        }

        [HttpGet(Name = nameof(GetSectores))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ABMClienteSearchDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult GetSectores([FromQuery] QueryFilters filters)
        {
            ABMSectorSearchDto reg = new ABMSectorSearchDto() { total_paginas = 0, total_registros = 0 };
            
            var clis = _abmSecServicio.Buscar(filters);
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
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetSectores)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetSectores)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<ABMSectorSearchDto>>(clis)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ABMSectorSearchDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult BuscarSectores(QueryFilters filters)
        {
            ABMSectorSearchDto reg = new() { total_paginas = 0, total_registros = 0 };
            
            var clis = _abmSecServicio.Buscar(filters);
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
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(BuscarSectores)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(BuscarSectores)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<ABMSectorSearchDto>>(clis)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }
    }
}
