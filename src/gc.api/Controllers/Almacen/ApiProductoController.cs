namespace gc.api.Controllers.Almacen
{
    using AutoMapper;
    using Azure;
    using gc.api.core.Contratos.Servicios;
    using gc.api.core.Entidades;
    using gc.infraestructura.Core.EntidadesComunes;
    using gc.infraestructura.Core.Interfaces;
    using gc.infraestructura.Core.Responses;
    using gc.infraestructura.Dtos.Almacen;
    using gc.infraestructura.Dtos.Almacen.Rpr;
    using gc.infraestructura.Dtos.Almacen.Tr;
    using gc.infraestructura.Dtos.CuentaComercial;
    using gc.infraestructura.Dtos.Gen;
    using gc.infraestructura.Dtos.Productos;
    using gc.infraestructura.EntidadesComunes.Options;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
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
        private IApiProductoServicio _productosSv;
        private readonly ITipoMotivoServicio _tmServicio;
        private readonly IUriService _uriService;
        private readonly ILogger<ApiProductoController> _logger;

        public ApiProductoController(IApiProductoServicio servicio, IMapper mapper, IUriService uriService, ILogger<ApiProductoController> logger, ITipoMotivoServicio tipo)
        {
            _productosSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
            _tmServicio = tipo;
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

        #region Acciones especificas de GECO INFOPROD

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoBusquedaDto>))]
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
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoBusquedaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult ProductoBuscarPorIds([FromQuery] BusquedaBase search)
		{
			ApiResponse<List<ProductoBusquedaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			var res = _productosSv.ProductoBuscarPorIds(search);

			response = new ApiResponse<List<ProductoBusquedaDto>>(res);

			return Ok(response);
		}

		[HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<InfoProdStkD>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult InfoProductoStkD(string id, string admId)
        {
            ApiResponse<List<InfoProdStkD>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _productosSv.InfoProductoStkD(id, admId);

            response = new ApiResponse<List<InfoProdStkD>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<InfoProdStkBox>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult InfoProductoStkBoxes(string id, string adm, string depo)
        {
            ApiResponse<List<InfoProdStkBox>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _productosSv.InfoProductoStkBoxes(id, adm, depo);

            response = new ApiResponse<List<InfoProdStkBox>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<InfoProdStkA>>))]
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
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<InfoProdMovStk>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult InfoProductoMovStk(string id, string adm, string depo, string tmov, long desde, long hasta)
        {
            ApiResponse<List<InfoProdMovStk>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            //if (depo.Equals("porc."))
            //{
            //    depo = "%";
            //}
            //if (tmov.Equals("porc."))
            //{
            //    tmov = "%";
            //}
            var res = _productosSv.InfoProductoMovStk(id, adm, depo, tmov, new DateTime(desde), new DateTime(hasta));

            response = new ApiResponse<List<InfoProdMovStk>>(res);

            return Ok(response);
        }
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<InfoProdLP>>))]
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

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerTiposMotivo()
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _tmServicio.ObtenerTiposMotivo();
            return Ok(new ApiResponse<List<TipoMotivo>>(res));
        }
        #endregion

        #region Acciones para modulo RPR

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RPRAutorizacionPendienteDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RPRAutorizacionPendiente(string adm)
        {
            ApiResponse<List<RPRAutorizacionPendienteDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _productosSv.RPRObtenerAutorizacionPendiente(adm);

            response = new ApiResponse<List<RPRAutorizacionPendienteDto>>(res);

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RespuestaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RPRCargar(RPRCargarRequest request)
        {
            ApiResponse<List<RespuestaDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _productosSv.RPRCargar(request);

            response = new ApiResponse<List<RespuestaDto>>(res);

            return Ok(response);
        }

        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RespuestaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RPRElimina(string rp)
        {
            ApiResponse<List<RespuestaDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _productosSv.RPRElimina(rp);

            response = new ApiResponse<List<RespuestaDto>>(res);

            return Ok(response);
        }

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RespuestaDto>>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult RPRConfirma(RPRAConfirmarRequest request)
		{
			ApiResponse<List<RespuestaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			var res = _productosSv.RPRConfirma(request.rp, request.adm_id);

			response = new ApiResponse<List<RespuestaDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<JsonDto>>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult RPRObtenerJson(string rp)
		{
			ApiResponse<List<JsonDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			var res = _productosSv.RPREObtenerDatosJsonDesdeRP(rp);

            response = new ApiResponse<List<JsonDto>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RPRItemVerCompteDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RPRObtenerItemVerCompte(string rp)
        {
            ApiResponse<List<RPRItemVerCompteDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _productosSv.RPRObtenerDatosVerCompte(rp);

            response = new ApiResponse<List<RPRItemVerCompteDto>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RPRVerConteoDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RPRObtenerVerConteos(string rp)
        {
            ApiResponse<List<RPRVerConteoDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _productosSv.RPRObtenerConteos(rp);

            response = new ApiResponse<List<RPRVerConteoDto>>(res);

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RPRRegistroResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]

        public IActionResult RPRRegistrar(List<RPRProcuctoDto> prods)
        {
            ApiResponse<RPRRegistroResponseDto> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");

            if (prods.Count == 0)
            {
                return BadRequest("No se recepcionó dato alguno.");

            }

            var json = JsonConvert.SerializeObject(prods);

            ////validar json
            //if (!JsonValido(json))
            //{
            //    return BadRequest("El JSON recepcionado no es válido");
            //}

            var res = _productosSv.RPRRegistrarProductos(json);
            response = new ApiResponse<RPRRegistroResponseDto>(res);
            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RPRAutoComptesPendientesDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RPRObtenerAutoComptesPendientes(string adm_id)
        {
            ApiResponse<List<RPRAutoComptesPendientesDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            if (string.IsNullOrEmpty(adm_id))
                return BadRequest("No se recepciono dato alguno.");

            var result = _productosSv.RPRObtenerComptesPendientes(adm_id);
            response = new ApiResponse<List<RPRAutoComptesPendientesDto>>(result);
            return Ok(response);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult ResguardarProductoCarrito(TiProductoCarritoDto request)
        {
            if (request == null) return BadRequest("No se recepcionaron los datos");

            RespuestaDto resp = _productosSv.ResguardarProductoCarrito(request);

            var response = new ApiResponse<RespuestaDto>(resp);
            return Ok(response);
        }

        /// <summary>
        /// Método destinado a validar la estructura del Json antes de ser enviado a la base de datos
        /// </summary>
        /// <param name = "json" ></ param >
        /// < returns ></ returns >
        private bool JsonValido(string json)
        {
            try
            {
                JObject.Parse(json);
                return true;
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex.Message, "JSON No válido.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "JSON No válido.");
                return false;
            }
        }
        #endregion

    }
}
