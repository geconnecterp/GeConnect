namespace gc.api.Controllers.Almacen
{
    using AutoMapper;
    using gc.api.core.Contratos.Servicios;
    using gc.api.core.Entidades;
    using gc.infraestructura.Core.EntidadesComunes;
    using gc.infraestructura.Core.Interfaces;
    using gc.infraestructura.Core.Responses;
    using gc.infraestructura.Dtos.Almacen;
    using gc.infraestructura.Dtos.Productos;
    using gc.infraestructura.EntidadesComunes.Options;
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
    public class ApiProductoController : ControllerBase
    {
        private readonly IMapper _mapper;
        private IProductoServicio _productosSv;
        private readonly IUriService _uriService;
        private readonly ILogger<ApiProductoController> _logger;

        public ApiProductoController(IProductoServicio servicio, IMapper mapper, IUriService uriService, ILogger<ApiProductoController> logger)
        {
            _productosSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
        }

       

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<ProductoListaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ProductoListaBuscar([FromQuery] BusquedaProducto search)
        {
            ApiResponse<List<ProductoListaDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            List<ProductoListaDto> res = _productosSv.ProductoListaBuscar(search);

            response = new ApiResponse<List<ProductoListaDto>>(res);

            return Ok(response);
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

        #region Acciones especificas de GECO

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ProductoBuscar([FromQuery] BusquedaBase search)
        {
            ApiResponse<ProductoBusquedaDto> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _productosSv.ProductoBuscar(search);

            response = new ApiResponse<ProductoBusquedaDto>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult InfoProductoStkD(string id, string admId)
        {
            ApiResponse<List<InfoProdStkD>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _productosSv.InfoProductoStkD(id,admId);

            response = new ApiResponse<List<InfoProdStkD>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult InfoProductoStkBoxes(string id, string adm, string depo)
        {
            ApiResponse<List<InfoProdStkBox>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _productosSv.InfoProductoStkBoxes(id, adm,depo);

            response = new ApiResponse<List<InfoProdStkBox>>(res);

            return Ok(response);
        }
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult InfoProductoStkA(string id, string admId)
        {
            ApiResponse<List<InfoProdStkA>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _productosSv.InfoProductoStkA(id, admId);

            response = new ApiResponse<List<InfoProdStkA>>(res);

            return Ok(response);
        }
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult InfoProductoMovStk(string id, string adm, string depo, string tmov, DateTime desde, DateTime hasta)
        {
            ApiResponse<List<InfoProdMovStk>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _productosSv.InfoProductoMovStk(id, adm,depo,tmov,desde,hasta);

            response = new ApiResponse<List<InfoProdMovStk>>(res);

            return Ok(response);
        }
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult InfoProductoLP(string id)
        {
            ApiResponse<List<InfoProdLP>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _productosSv.InfoProductoLP(id);

            response = new ApiResponse<List<InfoProdLP>>(res);

            return Ok(response);
        }
        #endregion

    }
}
