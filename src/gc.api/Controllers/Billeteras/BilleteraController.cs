namespace gc.api.Controllers.Billeteras
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
    public class BilleteraController : ControllerBase
    {
        private readonly IMapper _mapper;
        private IBilleteraServicio _billeterasSv;
        private readonly IUriService _uriService;
        private readonly ILogger<BilleteraController> _logger;

        public BilleteraController(IBilleteraServicio servicio, IMapper mapper, IUriService uriService, ILogger<BilleteraController> logger)
        {
            _billeterasSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }


        [HttpGet(Name = nameof(Getbilleterass))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<BilleteraDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Getbilleterass([FromQuery] QueryFilters filters)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var billeterass = _billeterasSv.GetAll(filters);
            var billeterasDtos = _mapper.Map<IEnumerable<BilleteraDto>>(billeterass);

            // presentando en el header información basica sobre la paginación
            var metadata = new Metadata
            {
                TotalCount = billeterass.TotalCount,
                PageSize = billeterass.PageSize,
                CurrentPage = billeterass.CurrentPage,
                TatalPages = billeterass.TotalPages,
                HasNextPage = billeterass.HasNextPage,
                HasPreviousPage = billeterass.HasPreviousPage,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getbilleterass))).ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getbilleterass))).ToString(),

            };

            var response = new ApiResponse<IEnumerable<BilleteraDto>>(billeterasDtos)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        // GET api/<billeterasController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var billeteras = await _billeterasSv.FindAsync(id);
            var datoDto = _mapper.Map<BilleteraDto>(billeteras);
            var response = new ApiResponse<BilleteraDto>(datoDto);
            return Ok(response);

        }


        // POST api/<billeterasController>
        [HttpPost]
        public async Task<IActionResult> Post(BilleteraDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var billeteras = _mapper.Map<Billetera>(datoDto);
            var res = await _billeterasSv.AddAsync(billeteras);

            var response = new ApiResponse<bool>(res);

            return Ok(response);
        }

        // PUT api/<billeterasController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] BilleteraDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var billeteras = _mapper.Map<Billetera>(datoDto);
            billeteras.Bill_id = id; //garantizo que el id buscado es el que se envia al negocio
            var result = await _billeterasSv.Update(billeteras);
            var response = new ApiResponse<bool>(result);
            return Ok(response);

        }

        // DELETE api/<billeterasController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = await _billeterasSv.Delete(id);
            var response = new ApiResponse<bool>(res);
            return Ok(response);
        }
    }
}
