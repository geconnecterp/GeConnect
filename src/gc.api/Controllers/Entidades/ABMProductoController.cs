using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
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
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ProductoListaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult GetProductos([FromQuery] QueryFilters filters)
        {
            ProductoListaDto reg = new ProductoListaDto() { Total_Paginas = 0, Total_Registros = 0 };
            //_logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var prods = _abmProductoServicio.Buscar(filters);
            if (prods.Count > 0)
            {
                reg = prods.First();
            }
            
            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = reg.Total_Registros,
                PageSize = filters.Registros.Value,
                CurrentPage = filters.Pagina.Value,
                TotalPages = reg.Total_Paginas,
                HasNextPage = filters.Pagina.Value<reg.Total_Paginas,
                HasPreviousPage = filters.Pagina.Value > 1,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetProductos)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetProductos)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<ProductoListaDto>>(prods)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ProductoListaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult BuscaProductos(QueryFilters filters)
        {
            ProductoListaDto reg = new ProductoListaDto() { Total_Paginas = 0, Total_Registros = 0 };
            //_logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var prods = _abmProductoServicio.Buscar(filters);
            if (prods.Count > 0)
            {
                reg = prods.First();
            }

            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = reg.Total_Registros,
                PageSize = filters.Registros.Value,
                CurrentPage = filters.Pagina.Value,
                TotalPages = reg.Total_Paginas,
                HasNextPage = filters.Pagina.Value < reg.Total_Paginas,
                HasPreviousPage = filters.Pagina.Value > 1,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetProductos)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(GetProductos)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<ProductoListaDto>>(prods)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }
    }
}
