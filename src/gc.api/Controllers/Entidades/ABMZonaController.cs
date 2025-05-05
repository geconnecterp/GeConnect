using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.ABM;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
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
    public class ABMZonaController : ControllerBase
    {
        private readonly ILogger<ABMZonaController> _logger;
        private readonly IZonaServicio _rpServicio;
        private readonly IUriService _uriService;

        public ABMZonaController(ILogger<ABMZonaController> logger, IZonaServicio rpServicio,
            IUriService uriService)
        {
            _logger = logger;
            _rpServicio = rpServicio;
            _uriService = uriService;
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<ABMZonaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ObtenerZonaes( QueryFilters filters)
        {
            var reg = new ABMZonaDto() { Total_Paginas = 0, Total_Registros = 0 };
            var lista = _rpServicio.ObtenerZonas(filters);

            if (lista.Count > 0)
            {
                reg = lista.First();
            }
            else
            {
                return NotFound("No se encontraron los Zonaes.");
            }

            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = reg.Total_Registros,
                PageSize = filters.Registros??0,
                CurrentPage = filters.Pagina??0,
                TotalPages = reg.Total_Paginas,
                HasNextPage =(filters.Pagina??0)< reg.Total_Paginas,
                HasPreviousPage =(filters.Pagina??0)> 1,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(ObtenerZonaes)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(ObtenerZonaes)) ?? "").ToString(),

            };

            var response = new ApiResponse<List<ABMZonaDto>>(lista)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ZonaDto))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult ObtenerZonaPorId(string zn_id)
        {
            var vendedor = _rpServicio.ObtenerZonaPorId(zn_id);
            if (vendedor.Count()==0)
            {
                return NotFound("No se encontró la Zona");
            }
            var vend = vendedor.First();
            return Ok(new ApiResponse<ZonaDto>(vend));
        }

    }
}
