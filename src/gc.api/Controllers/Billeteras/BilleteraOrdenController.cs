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

    //[Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class BilleteraOrdenController : ControllerBase
    {
        private readonly IMapper _mapper;
        private IBilleteraOrdenServicio _billeteras_ordenesSv;
        private readonly IUriService _uriService;
        private readonly ILogger<BilleteraOrdenController> _logger;

        public BilleteraOrdenController(IBilleteraOrdenServicio servicio, IMapper mapper, IUriService uriService, ILogger<BilleteraOrdenController> logger)
        {
            _billeteras_ordenesSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }


        [HttpGet(Name = nameof(Getbilleteras_ordeness))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<BilleteraOrdenDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Getbilleteras_ordeness([FromQuery] QueryFilters filters)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var billeteras_ordeness = _billeteras_ordenesSv.GetAll(filters);
            var billeteras_ordenesDtos = _mapper.Map<IEnumerable<BilleteraOrdenDto>>(billeteras_ordeness);

            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = billeteras_ordeness.TotalCount,
                PageSize = billeteras_ordeness.PageSize,
                CurrentPage = billeteras_ordeness.CurrentPage,
                TotalPages = billeteras_ordeness.TotalPages,
                HasNextPage = billeteras_ordeness.HasNextPage,
                HasPreviousPage = billeteras_ordeness.HasPreviousPage,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getbilleteras_ordeness)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getbilleteras_ordeness)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<BilleteraOrdenDto>>(billeteras_ordenesDtos)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        // GET api/<billeteras_ordenesController>/5
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var billeteras_ordenes =  _billeteras_ordenesSv.Find(id);
            if (billeteras_ordenes == null)
            {
                return BadRequest("No se encontro los datos de la Orden de la Billetera");
            }

            var datoDto = _mapper.Map<BilleteraOrdenDto>(billeteras_ordenes);
            var response = new ApiResponse<BilleteraOrdenDto>(datoDto);
            return Ok(response);

        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK,Type = typeof(ApiResponse<(bool,string)>))]
        [Route("[action]/{ordenId}")]
        public IActionResult VerificaPago(string ordenId)
        {
            (bool, (string, string)) res = _billeteras_ordenesSv.VerificaPago(ordenId); // TODO
            var response = new ApiResponse<(bool, (string, string))>(res); 
            return Ok(response);
        }


        // POST api/<billeteras_ordenesController>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult Post([FromBody] BilleteraOrdenDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var billeteras_ordenes = _mapper.Map<BilleteraOrden>(datoDto);
            var res = _billeteras_ordenesSv.CargarOrden(billeteras_ordenes);

            var response = new ApiResponse<(bool,string)>(res);

            return Ok(response);
        }

        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]/{nroOrden}")]
        public IActionResult OrdenNotificado(string nroOrden, [FromBody] OrdenNotificado ordenNotificado)
        {
            string msg;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            if(ordenNotificado == null)
            {
                msg = $"Orden: {nroOrden} - No se recepcionó la orden de Notificación";
                _logger.Log(LogLevel.Error, msg);
                return BadRequest(msg);
            }
            if(ordenNotificado.Orden_Id != nroOrden)
            {
                msg = $"Se recepcionó Orden: {nroOrden} y testigo orden {ordenNotificado.Orden_Id}";
                _logger.Log(LogLevel.Error, msg);
                return BadRequest(msg);
            }

            (bool, string) res = _billeteras_ordenesSv.OrdenNotificado(ordenNotificado);

            var response = new ApiResponse<(bool, string)>(res);

            return Ok(response);           
        }

        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]/{nroOrden}")]
        public IActionResult OrdenRegistro(string nroOrden, [FromBody] OrdenRegistro ordenRegistro)
        {
            string msg;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            if (ordenRegistro == null)
            {
                msg = $"Orden: {nroOrden} - No se recepcionó la orden de Notificación";
                _logger.Log(LogLevel.Error, msg);
                return BadRequest(msg);
            }
            if (ordenRegistro.Orden_Id != nroOrden)
            {
                msg = $"Se recepcionó Orden: {nroOrden} y testigo orden {ordenRegistro.Orden_Id}";
                _logger.Log(LogLevel.Error, msg);
                return BadRequest(msg);
            }

            (bool, string) res = _billeteras_ordenesSv.OrdenRegistro(ordenRegistro);

            var response = new ApiResponse<(bool, string)>(res);

            return Ok(response);
        }

        // PUT api/<billeteras_ordenesController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] BilleteraOrdenDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var billeteras_ordenes = _mapper.Map<BilleteraOrden>(datoDto);
            billeteras_ordenes.Bo_Id = id; //garantizo que el id buscado es el que se envia al negocio
            var result = await _billeteras_ordenesSv.Update(billeteras_ordenes);
            var response = new ApiResponse<bool>(result);
            return Ok(response);

        }

        // DELETE api/<billeteras_ordenesController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = await _billeteras_ordenesSv.Delete(id);
            var response = new ApiResponse<bool>(res);
            return Ok(response);
        }
    }
}
