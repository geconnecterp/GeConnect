namespace gc.api.Controllers.Almacen
{
    using AutoMapper;
    using gc.api.core.Contratos.Servicios;
    using gc.api.core.Entidades;
    using gc.infraestructura.Core.EntidadesComunes;
    using gc.infraestructura.Core.Interfaces;
    using gc.infraestructura.Core.Responses;
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
    public class ProveedorController : ControllerBase
    {
        private readonly IMapper _mapper;
        private IProveedorServicio _proveedoresSv;
        private readonly IUriService _uriService;
        private readonly ILogger<ProveedorController> _logger;

        public ProveedorController(IProveedorServicio servicio, IMapper mapper, IUriService uriService, ILogger<ProveedorController> logger)
        {
            _proveedoresSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }


        [HttpGet(Name = nameof(Getproveedoress))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ProveedorDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Getproveedoress([FromQuery] QueryFilters filters)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var proveedoress = _proveedoresSv.GetAll(filters);
            var proveedoresDtos = _mapper.Map<IEnumerable<ProveedorDto>>(proveedoress);

            // presentando en el header información basica sobre la paginación
            var metadata = new Metadata
            {
                TotalCount = proveedoress.TotalCount,
                PageSize = proveedoress.PageSize,
                CurrentPage = proveedoress.CurrentPage,
                TatalPages = proveedoress.TotalPages,
                HasNextPage = proveedoress.HasNextPage,
                HasPreviousPage = proveedoress.HasPreviousPage,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getproveedoress))).ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getproveedoress))).ToString(),

            };

            var response = new ApiResponse<IEnumerable<ProveedorDto>>(proveedoresDtos)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        // GET api/<proveedoresController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var proveedores = await _proveedoresSv.FindAsync(id);
            var datoDto = _mapper.Map<ProveedorDto>(proveedores);
            var response = new ApiResponse<ProveedorDto>(datoDto);
            return Ok(response);

        }

        // POST api/<proveedoresController>
        [HttpPost]
        public async Task<IActionResult> Post(ProveedorDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var proveedores = _mapper.Map<Proveedor>(datoDto);
            var res = await _proveedoresSv.AddAsync(proveedores);

            var response = new ApiResponse<bool>(res);

            return Ok(response);
        }

        // PUT api/<proveedoresController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] ProveedorDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var proveedores = _mapper.Map<Proveedor>(datoDto);
            proveedores.Cta_Id = id; //garantizo que el id buscado es el que se envia al negocio
            var result = await _proveedoresSv.Update(proveedores);
            var response = new ApiResponse<bool>(result);
            return Ok(response);

        }

        // DELETE api/<proveedoresController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = await _proveedoresSv.Delete(id);
            var response = new ApiResponse<bool>(res);
            return Ok(response);
        }
    }
}
