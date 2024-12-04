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
    public class ABMProductoController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        private readonly ILogger<ABMProductoController> _logger;
        private readonly IABMProductoServicio _abmProductoServicio;

        public ABMProductoController(IABMProductoServicio productoServicio, IMapper mapper, IUriService uriService)
        {
            _abmProductoServicio = productoServicio;
            _mapper = mapper;
            _uriService = uriService;
        }


        [HttpGet(Name = nameof(GetProductos))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ABMProductoSearchDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult GetProductos([FromQuery] QueryFilters filters)
        {
            ABMProductoSearchDto reg = new ABMProductoSearchDto() { total_paginas = 0, total_registros = 0 };
            //_logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var prods = _abmProductoServicio.Buscar(filters);
            if (prods.Count > 0)
            {
                reg = prods.First();
            }
            
            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = reg.total_registros,
                PageSize = filters.Registros.Value,
                CurrentPage = filters.Pagina.Value,
                TotalPages = reg.total_paginas,
                HasNextPage = filters.Pagina.Value<reg.total_paginas,
                HasPreviousPage = filters.Pagina.Value > 1,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetProductos)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetProductos)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<ABMProductoSearchDto>>(prods)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }
    }
}
