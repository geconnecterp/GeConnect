using gc.api.core.Contratos.Servicios.ABM;
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
    [Authorize]
    [ApiController]
    public class ABMRepartidorController : ControllerBase
    {
        private readonly ILogger<ABMRepartidorController> _logger;
        private readonly IABMRepartidorServicio _rpServicio;
        private readonly IUriService _uriService;

        public ABMRepartidorController(ILogger<ABMRepartidorController> logger, IABMRepartidorServicio rpServicio,
            IUriService uriService)
        {
            _logger = logger;
            _rpServicio = rpServicio;
            _uriService = uriService;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<ABMRepartidorDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ObtenerRepartidores( QueryFilters filters)
        {
            var reg = new ABMRepartidorDto() { Total_Paginas = 0, Total_Registros = 0 };
            var lista = _rpServicio.ObtenerRepartidores(filters);

            if (lista.Count > 0)
            {
                reg = lista.First();
            }
            else
            {
                return NotFound("No se encontraron los Repartidores.");
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
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(ObtenerRepartidores)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(ObtenerRepartidores)) ?? "").ToString(),

            };

            var response = new ApiResponse<List<ABMRepartidorDto>>(lista)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ABMRepartidorDatoDto))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ObtenerRepartidorPorId(string ve_id)
        {
            var vendedor = _rpServicio.ObtenerRepartidorPorId(ve_id);
            if (vendedor.Count()==0)
            {
                return NotFound("No se encontró el Repartidor");
            }
            var vend = vendedor.First();
            return Ok(new ApiResponse<ABMRepartidorDatoDto>(vend));
        }

    }
}
