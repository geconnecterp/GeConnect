using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace gc.api.Controllers.Entidades
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ABMClienteController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        private readonly IABMClienteServicio _abmClienteServicio;
        public ABMClienteController(IABMClienteServicio abmClienteServicio, IMapper mapper, IUriService uriService)
        {
            _abmClienteServicio = abmClienteServicio;
            _mapper = mapper;
            _uriService = uriService;
        }

        [HttpGet(Name = nameof(GetClientes))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ABMClienteSearchDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult GetClientes([FromQuery] QueryFilters filters)
        {
            ABMClienteSearchDto reg = new ABMClienteSearchDto() { total_paginas = 0, total_registros = 0 };
            //_logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var clis = _abmClienteServicio.Buscar(filters);
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
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetClientes)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetClientes)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<ABMClienteSearchDto>>(clis)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ABMClienteSearchDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult BuscarClientes(QueryFilters filters)
        {
            ABMClienteSearchDto reg = new ABMClienteSearchDto() { total_paginas = 0, total_registros = 0 };
            //_logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var clis = _abmClienteServicio.Buscar(filters);
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
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(BuscarClientes)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(BuscarClientes)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<ABMClienteSearchDto>>(clis)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }
    }
}
