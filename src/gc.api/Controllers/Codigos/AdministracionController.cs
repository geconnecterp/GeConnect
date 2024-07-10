namespace gc.api.Controllers.Codigos
{
    using AutoMapper;
    using gc.api.core.Contratos.Servicios;
    using gc.api.core.Entidades;
    using gc.infraestructura.Core.EntidadesComunes;
    using gc.infraestructura.Core.Interfaces;
    using gc.infraestructura.Core.Responses;
    using gc.infraestructura.Dtos.Administracion;
    using log4net.Filter;
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
    public class AdministracionController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAdministracionServicio _adminSv;
        private readonly IUriService _uriService;
        private readonly ILogger<AdministracionController> _logger;

        public AdministracionController(IAdministracionServicio servicio, IMapper mapper, IUriService uriService, ILogger<AdministracionController> logger)
        {
            _adminSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }


        [HttpGet(Name = nameof(Getadministraciones))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<AdministracionDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Getadministraciones([FromQuery] QueryFilters filters)
        {
            //_logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var administracioness = _adminSv.GetAll(filters);
            var administracionesDtos = _mapper.Map<IEnumerable<AdministracionDto>>(administracioness);

            // presentando en el header información basica sobre la paginación
            var metadata = new Metadata
            {
                TotalCount = administracioness.TotalCount,
                PageSize = administracioness.PageSize,
                CurrentPage = administracioness.CurrentPage,
                TatalPages = administracioness.TotalPages,
                HasNextPage = administracioness.HasNextPage,
                HasPreviousPage = administracioness.HasPreviousPage,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getadministraciones)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getadministraciones)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<AdministracionDto>>(administracionesDtos)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        // GET api/<administracionesController>/5
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var Administracion = _adminSv.Find(id);
            var datoDto = _mapper.Map<AdministracionDto>(Administracion);
            var response = new ApiResponse<AdministracionDto>(datoDto);
            return Ok(response);

        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<AdministracionLoginDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult GetAdministraciones4Login()
        {
            var administracioness = _adminSv.GetAll(new QueryFilters());
            var combo = _mapper.Map<List<AdministracionLoginDto>>(administracioness);

            var response = new ApiResponse<List<AdministracionLoginDto>>(combo);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK,Type =typeof(ApiResponse<bool>))]
        [Route("[action]")]
        public IActionResult ActualizaMePaId([FromBody] AdmUpdateMePaDto datos)
        {
            bool res = _adminSv.ActualizaMePaId(datos);
            ApiResponse<bool> resp;

            resp = new ApiResponse<bool>(res);

            return Ok(resp);
        }

        //// POST api/<administracionesController>
        //[HttpPost]
        //public async Task<IActionResult> Post(AdministracionDto datoDto)
        //{
        //    _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
        //    var Admin = _mapper.Map<Administracion>(datoDto);
        //    var res = await _adminSv.AddAsync(Admin);

        //    var response = new ApiResponse<bool>(res);

        //    return Ok(response);
        //}

        //// PUT api/<administracionesController>/5
        //[HttpPut("{id}")]
        //public async Task<IActionResult> Put(string id, [FromBody] AdministracionDto datoDto)
        //{
        //    _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
        //    var Administracion = _mapper.Map<Administracion>(datoDto);
        //    Administracion.Adm_id = id; //garantizo que el id buscado es el que se envia al negocio
        //    var result = await _adminSv.Update(Administracion);
        //    var response = new ApiResponse<bool>(result);
        //    return Ok(response);

        //}

        //// DELETE api/<administracionesController>/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(string id)
        //{
        //    _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
        //    var res = await _adminSv.Delete(id);
        //    var response = new ApiResponse<bool>(res);
        //    return Ok(response);
        //}
    }
}
