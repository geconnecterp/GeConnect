using gc.api.core.Contratos.Servicios.ABM;
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
    //[Authorize]
    [ApiController]
    public class ABMVendedorController : ControllerBase
    {
        private readonly ILogger<ABMClienteController> _logger;
        private readonly IABMVendedorServicio _vendedorServicio;
        private readonly IUriService _uriService;

        public ABMVendedorController(ILogger<ABMClienteController> logger, IABMVendedorServicio vendedorServicio,
            IUriService uriService)
        {
            _logger = logger;
            _vendedorServicio = vendedorServicio;
            _uriService = uriService;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<ABMVendedorDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ObtenerVendedores( QueryFilters filters)
        {
            var reg = new ABMVendedorDto() { Total_Paginas = 0, Total_Registros = 0 };
            var lista = _vendedorServicio.ObtenerVendedores(filters);

            if (lista.Count > 0)
            {
                reg = lista.First();
            }
            else
            {
                return NotFound("No se encontraron Vendedores");
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
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(ObtenerVendedores)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(ObtenerVendedores)) ?? "").ToString(),

            };

            var response = new ApiResponse<List<ABMVendedorDto>>(lista)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ABMVendedorDatoDto))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ObtenerVendedorPorId(string ve_id)
        {
            var vendedor = _vendedorServicio.ObtenerVendedorPorId(ve_id);
            if (vendedor.Count()==0)
            {
                return NotFound("No se encontró el Vendedor");
            }
            var vend = vendedor.First();
            return Ok(new ApiResponse<ABMVendedorDatoDto>(vend));
        }

    }
}
