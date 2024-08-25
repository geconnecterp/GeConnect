namespace gc.api.Controllers.Almacen
{
    using AutoMapper;
    using gc.api.core.Contratos.Servicios;
    using gc.api.core.Entidades;
    using gc.infraestructura.Core.EntidadesComunes;
    using gc.infraestructura.Core.Interfaces;
    using gc.infraestructura.Core.Responses;
    using gc.infraestructura.Dtos.Administracion;
    using gc.infraestructura.Dtos.Almacen;
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
    public class ApiDepositoController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IDepositoServicio _depositosSv;
        private readonly IUriService _uriService;
        private readonly ILogger<ApiDepositoController> _logger;

        public ApiDepositoController(IDepositoServicio servicio, IMapper mapper, IUriService uriService, ILogger<ApiDepositoController> logger)
        {
            _depositosSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }


        [HttpGet(Name = nameof(Getdepositoss))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<DepositoDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Getdepositoss([FromQuery] QueryFilters filters)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var depositoss = _depositosSv.GetAll(filters);
            var depositosDtos = _mapper.Map<IEnumerable<DepositoDto>>(depositoss);

            // presentando en el header información basica sobre la paginación
            var metadata = new Metadata
            {
                TotalCount = depositoss.TotalCount,
                PageSize = depositoss.PageSize,
                CurrentPage = depositoss.CurrentPage,
                TatalPages = depositoss.TotalPages,
                HasNextPage = depositoss.HasNextPage,
                HasPreviousPage = depositoss.HasPreviousPage,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getdepositoss))).ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getdepositoss))).ToString(),

            };

            var response = new ApiResponse<IEnumerable<DepositoDto>>(depositosDtos)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        // GET api/<depositosController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var depositos = await _depositosSv.FindAsync(id);
            var datoDto = _mapper.Map<DepositoDto>(depositos);
            var response = new ApiResponse<DepositoDto>(datoDto);
            return Ok(response);

        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<DepositoDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult GetDepositoXAdm(string adm_id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var depositos = _depositosSv.ObtenerDepositosDeAdministracion(adm_id);
            var datoDto = _mapper.Map<List<DepositoDto>>(depositos);
            var response = new ApiResponse<List<DepositoDto>>(datoDto);
            return Ok(response);
        }

        // POST api/<depositosController>
        [HttpPost]
        public async Task<IActionResult> Post(DepositoDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var depo = _mapper.Map<Deposito>(datoDto);
            var res = await _depositosSv.AddAsync(depo);

            var response = new ApiResponse<bool>(res);

            return Ok(response);
        }

        // PUT api/<depositosController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] DepositoDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var depositos = _mapper.Map<Deposito>(datoDto);
            depositos.Depo_Id = id; //garantizo que el id buscado es el que se envia al negocio
            var result = await _depositosSv.Update(depositos);
            var response = new ApiResponse<bool>(result);
            return Ok(response);

        }

        // DELETE api/<depositosController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = await _depositosSv.Delete(id);
            var response = new ApiResponse<bool>(res);
            return Ok(response);
        }
    }
}
