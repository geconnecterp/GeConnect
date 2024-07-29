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
    public class ProductoController : ControllerBase
    {
        private readonly IMapper _mapper;
        private IProductoServicio _productosSv;
        private readonly IUriService _uriService;
        private readonly ILogger<ProductoController> _logger;

        public ProductoController(IProductoServicio servicio, IMapper mapper, IUriService uriService, ILogger<ProductoController> logger)
        {
            _productosSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }


        [HttpGet(Name = nameof(Getproductoss))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ProductoDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Getproductoss([FromQuery] QueryFilters filters)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var productoss = _productosSv.GetAll(filters);
            var productosDtos = _mapper.Map<IEnumerable<ProductoDto>>(productoss);

            // presentando en el header información basica sobre la paginación
            var metadata = new Metadata
            {
                TotalCount = productoss.TotalCount,
                PageSize = productoss.PageSize,
                CurrentPage = productoss.CurrentPage,
                TatalPages = productoss.TotalPages,
                HasNextPage = productoss.HasNextPage,
                HasPreviousPage = productoss.HasPreviousPage,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getproductoss))).ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getproductoss))).ToString(),

            };

            var response = new ApiResponse<IEnumerable<ProductoDto>>(productosDtos)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        // GET api/<productosController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var productos = await _productosSv.FindAsync(id);
            var datoDto = _mapper.Map<ProductoDto>(productos);
            var response = new ApiResponse<ProductoDto>(datoDto);
            return Ok(response);

        }


        // POST api/<productosController>
        [HttpPost]
        public async Task<IActionResult> Post(ProductoDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var productos = _mapper.Map<Producto>(datoDto);
            var res = await _productosSv.AddAsync(productos);

            var response = new ApiResponse<bool>(res);

            return Ok(response);
        }

        // PUT api/<productosController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] ProductoDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var productos = _mapper.Map<Producto>(datoDto);
            productos.Cta_Id = id; //garantizo que el id buscado es el que se envia al negocio
            var result = await _productosSv.Update(productos);
            var response = new ApiResponse<bool>(result);
            return Ok(response);

        }

        // DELETE api/<productosController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = await _productosSv.Delete(id);
            var response = new ApiResponse<bool>(res);
            return Ok(response);
        }
    }
}
