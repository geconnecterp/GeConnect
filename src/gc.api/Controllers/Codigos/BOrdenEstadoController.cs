namespace gc.api.Controllers.Codigos
{
    using AutoMapper;
    using gc.api.core.Contratos.Servicios;
    using gc.api.core.Entidades;
    using gc.infraestructura.Core.EntidadesComunes;
    using gc.infraestructura.Core.Interfaces;
    using gc.infraestructura.Core.Responses;
    using gc.infraestructura.Dtos.Billeteras;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;

    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class BOrdenEstadoController : ControllerBase
    {
        private readonly IMapper _mapper;
        private IBOrdenEstadoServicio _billeteras_ordenes_eSv;
        private readonly IUriService _uriService;
        private readonly ILogger<BOrdenEstadoController> _logger;

        public BOrdenEstadoController(IBOrdenEstadoServicio servicio, IMapper mapper, IUriService uriService, ILogger<BOrdenEstadoController> logger)
        {
            _billeteras_ordenes_eSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }


        [HttpGet(Name = nameof(Getbilleteras_ordenes_es))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<BOrdenEstadoDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Getbilleteras_ordenes_es([FromQuery] QueryFilters filters)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var billeteras_ordenes_es = _billeteras_ordenes_eSv.GetAll(filters);
            var billeteras_ordenes_eDtos = _mapper.Map<IEnumerable<BOrdenEstadoDto>>(billeteras_ordenes_es);

            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = billeteras_ordenes_es.TotalCount,
                PageSize = billeteras_ordenes_es.PageSize,
                CurrentPage = billeteras_ordenes_es.CurrentPage,
                TotalPages = billeteras_ordenes_es.TotalPages,
                HasNextPage = billeteras_ordenes_es.HasNextPage,
                HasPreviousPage = billeteras_ordenes_es.HasPreviousPage,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getbilleteras_ordenes_es)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getbilleteras_ordenes_es)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<BOrdenEstadoDto>>(billeteras_ordenes_eDtos)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        // GET api/<billeteras_ordenes_eController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var billeteras_ordenes_e = await _billeteras_ordenes_eSv.FindAsync(id);
            var datoDto = _mapper.Map<BOrdenEstadoDto>(billeteras_ordenes_e);
            var response = new ApiResponse<BOrdenEstadoDto>(datoDto);
            return Ok(response);

        }


        // POST api/<billeteras_ordenes_eController>
        [HttpPost]
        public async Task<IActionResult> Post(BOrdenEstadoDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var billeteras_ordenes_e = _mapper.Map<BOrdenEstado>(datoDto);
            var res = await _billeteras_ordenes_eSv.AddAsync(billeteras_ordenes_e);

            var response = new ApiResponse<bool>(res);

            return Ok(response);
        }

        // PUT api/<billeteras_ordenes_eController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] BOrdenEstadoDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var billeteras_ordenes_e = _mapper.Map<BOrdenEstado>(datoDto);
            billeteras_ordenes_e.Boe_id = id; //garantizo que el id buscado es el que se envia al negocio
            var result = await _billeteras_ordenes_eSv.Update(billeteras_ordenes_e);
            var response = new ApiResponse<bool>(result);
            return Ok(response);

        }

        // DELETE api/<billeteras_ordenes_eController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = await _billeteras_ordenes_eSv.Delete(id);
            var response = new ApiResponse<bool>(res);
            return Ok(response);
        }
    }
}
