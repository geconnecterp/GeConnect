namespace geco_0000.API.Controllers.Codigos
{
    using AutoMapper;
    using gc.api.core.Contratos.Servicios;
    using gc.api.core.Entidades;
    using gc.infraestructura.Core.EntidadesComunes;
    using gc.infraestructura.Core.Exceptions;
    using gc.infraestructura.Core.Interfaces;
    using gc.infraestructura.Core.Responses;
    using gc.infraestructura.Dtos.Administracion;
    using gc.infraestructura.Dtos.Cajas;
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
    public class CajaController : ControllerBase
    {
        private readonly IMapper _mapper;
        private ICajaServicio _cajasSv;
        private readonly IUriService _uriService;
        private readonly ILogger<CajaController> _logger;

        public CajaController(ICajaServicio servicio, IMapper mapper, IUriService uriService, ILogger<CajaController> logger)
        {
            _cajasSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }


        [HttpGet(Name = nameof(Getcajass))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<CajaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Getcajass([FromQuery] QueryFilters filters)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var cajass = _cajasSv.GetAll(filters);
            var cajasDtos = _mapper.Map<IEnumerable<CajaDto>>(cajass);

            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = cajass.TotalCount,
                PageSize = cajass.PageSize,
                CurrentPage = cajass.CurrentPage,
                TotalPages = cajass.TotalPages,
                HasNextPage = cajass.HasNextPage,
                HasPreviousPage = cajass.HasPreviousPage,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getcajass)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getcajass)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<CajaDto>>(cajasDtos)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        // GET api/<cajasController>/5
        [HttpGet("{sucId}/{cajaId}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CajaDto>))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(ExceptionValidation))]
        [ProducesResponseType((int)HttpStatusCode.Conflict, Type = typeof(ExceptionValidation))]
        public IActionResult Get(string sucId,string cajaId)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var cajas = _cajasSv.Find(sucId,cajaId) ?? throw new NegocioException("La Caja buscada no se ha encontrado.");
            var datoDto = _mapper.Map<CajaDto>(cajas);
            var response = new ApiResponse<CajaDto>(datoDto);
            return Ok(response);

        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<bool>))]
        [Route("[action]")]
        public IActionResult ActualizaMePaId([FromBody] CajaUpMePaId datos)
        {
            bool res = _cajasSv.ActualizaMePaId(datos);
            ApiResponse<bool> resp;

            resp = new ApiResponse<bool>(res);

            return Ok(resp);
        }

        // POST api/<cajasController>
        [HttpPost]
        public async Task<IActionResult> Post(CajaDto datoDto)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var cajas = _mapper.Map<Caja>(datoDto);
            var res = await _cajasSv.AddAsync(cajas);

            var response = new ApiResponse<bool>(res);

            return Ok(response);
        }

        // PUT api/<cajasController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] CajaDto datoDto)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var cajas = _mapper.Map<Caja>(datoDto);
            cajas.Caja_Id = id; //garantizo que el id buscado es el que se envia al negocio
            var result = await _cajasSv.Update(cajas);
            var response = new ApiResponse<bool>(result);
            return Ok(response);

        }

        // DELETE api/<cajasController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = await _cajasSv.Delete(id);
            var response = new ApiResponse<bool>(res);
            return Ok(response);
        }
    }
}
