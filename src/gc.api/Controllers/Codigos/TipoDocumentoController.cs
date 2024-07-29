namespace gc.api.Controllers.Codigos
{
    using AutoMapper;
    using gc.api.core.Contratos.Servicios;
    using gc.api.core.Entidades;
    using gc.infraestructura.Core.EntidadesComunes;
    using gc.infraestructura.Core.Interfaces;
    using gc.infraestructura.Core.Responses;
    using gc.infraestructura.Dtos;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;

    //[Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class TipoDocumentoController : ControllerBase
    {
        private readonly IMapper _mapper;
        private ITiposDocumentoServicio _tipos_documentosSv;
        private readonly IUriService _uriService;
        private readonly ILogger<TipoDocumentoController> _logger;

        public TipoDocumentoController(ITiposDocumentoServicio servicio, IMapper mapper, IUriService uriService, ILogger<TipoDocumentoController> logger)
        {
            _tipos_documentosSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }


        [HttpGet(Name = nameof(Gettipos_documentoss))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<TipoDocumentoDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Gettipos_documentoss([FromQuery] QueryFilters filters)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var tipos_documentoss = _tipos_documentosSv.GetAll(filters);
            var tipos_documentosDtos = _mapper.Map<IEnumerable<TipoDocumentoDto>>(tipos_documentoss);

            // presentando en el header información basica sobre la paginación
            var metadata = new Metadata
            {
                TotalCount = tipos_documentoss.TotalCount,
                PageSize = tipos_documentoss.PageSize,
                CurrentPage = tipos_documentoss.CurrentPage,
                TatalPages = tipos_documentoss.TotalPages,
                HasNextPage = tipos_documentoss.HasNextPage,
                HasPreviousPage = tipos_documentoss.HasPreviousPage,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Gettipos_documentoss))).ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Gettipos_documentoss))).ToString(),

            };

            var response = new ApiResponse<IEnumerable<TipoDocumentoDto>>(tipos_documentosDtos)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        // GET api/<tipos_documentosController>/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var tipos_documentos = await _tipos_documentosSv.FindAsync(id);
            var datoDto = _mapper.Map<TipoDocumentoDto>(tipos_documentos);
            var response = new ApiResponse<TipoDocumentoDto>(datoDto);
            return Ok(response);

        }


        // POST api/<tipos_documentosController>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(TipoDocumentoDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var tipos_documentos = _mapper.Map<TipoDocumento>(datoDto);
            var res = await _tipos_documentosSv.AddAsync(tipos_documentos);

            var response = new ApiResponse<bool>(res);

            return Ok(response);
        }

        // PUT api/<tipos_documentosController>/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(string id, [FromBody] TipoDocumentoDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var tipos_documentos = _mapper.Map<TipoDocumento>(datoDto);
            tipos_documentos.Tdoc_Id = id; //garantizo que el id buscado es el que se envia al negocio
            var result = await _tipos_documentosSv.Update(tipos_documentos);
            var response = new ApiResponse<bool>(result);
            return Ok(response);

        }

        // DELETE api/<tipos_documentosController>/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = await _tipos_documentosSv.Delete(id);
            var response = new ApiResponse<bool>(res);
            return Ok(response);
        }
    }
}
