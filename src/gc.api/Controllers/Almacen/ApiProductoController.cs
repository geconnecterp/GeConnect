namespace gc.api.Controllers.Almacen
{
    using AutoMapper;
    using gc.api.core.Contratos.Servicios;
    using gc.api.core.Entidades;
    using gc.infraestructura.Core.EntidadesComunes;
    using gc.infraestructura.Core.Exceptions;
    using gc.infraestructura.Core.Interfaces;
    using gc.infraestructura.Core.Responses;
    using gc.infraestructura.Dtos.Almacen;
    using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
    using gc.infraestructura.Dtos.Almacen.AjusteDeStock.Request;
    using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor;
    using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor.Request;
    using gc.infraestructura.Dtos.Almacen.Info;
    using gc.infraestructura.Dtos.Almacen.Request;
    using gc.infraestructura.Dtos.Almacen.Response;
    using gc.infraestructura.Dtos.Almacen.Rpr;
    using gc.infraestructura.Dtos.Almacen.Tr;
    using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
    using gc.infraestructura.Dtos.Box;
    using gc.infraestructura.Dtos.CuentaComercial;
    using gc.infraestructura.Dtos.Gen;
    using gc.infraestructura.Dtos.General;
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
    using NDeCYPI = gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI;

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
        private readonly IProductoDepositoServicio _prodDepoSv;
        private readonly IListaPrecioServicio _lpSv;

        public ApiProductoController(IApiProductoServicio servicio, IMapper mapper, IUriService uriService,
            ILogger<ApiProductoController> logger, ITipoMotivoServicio tipo, IProductoDepositoServicio prodDepoSv, IListaPrecioServicio lpSv)
        {
            _productosSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
            _tmServicio = tipo;
            _prodDepoSv = prodDepoSv;
            _lpSv = lpSv;
        }



        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<ProductoListaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ProductoListaBuscar(BusquedaProducto filters)
        {
            var reg = new ProductoListaDto { Total_Paginas = 0, Total_Registros = 0 };

            ApiResponse<List<ProductoListaDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            List<ProductoListaDto> res = _productosSv.ProductoListaBuscar(filters);

            if (res.Count > 0)
            {
                reg = res.First();
            }

            var metadata = new MetadataGrid
            {
                TotalCount = reg.Total_Registros,
                PageSize = filters.Registros ?? 0,
                CurrentPage = filters.Pagina ?? 0,
                TotalPages = reg.Total_Paginas,
                HasNextPage = (filters.Pagina ?? 0) < reg.Total_Paginas,
                HasPreviousPage = (filters.Pagina ?? 0) > 1,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getproductoss)) ?? string.Empty).ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getproductoss)) ?? string.Empty).ToString(),

            };

            response = new ApiResponse<List<ProductoListaDto>>(res)
            {
                Meta = metadata,
            };

            return Ok(response);
        }

        [HttpGet(Name = nameof(Getproductoss))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ProductoDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Getproductoss([FromQuery] QueryFilters filters)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var productoss = _productosSv.GetAll(filters);
            var productosDtos = _mapper.Map<IEnumerable<ProductoDto>>(productoss);

            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = productoss.TotalCount,
                PageSize = productoss.PageSize,
                CurrentPage = productoss.CurrentPage,
                TotalPages = productoss.TotalPages,
                HasNextPage = productoss.HasNextPage,
                HasPreviousPage = productoss.HasPreviousPage,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getproductoss)) ?? string.Empty).ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getproductoss)) ?? string.Empty).ToString(),

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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var productos = await _productosSv.FindAsync(id);
            var datoDto = _mapper.Map<ProductoDto>(productos);
            var response = new ApiResponse<ProductoDto>(datoDto);
            return Ok(response);

        }


        // POST api/<productosController>
        [HttpPost]
        public async Task<IActionResult> Post(ProductoDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var productos = _mapper.Map<Producto>(datoDto);
            var res = await _productosSv.AddAsync(productos);

            var response = new ApiResponse<bool>(res);

            return Ok(response);
        }

        // PUT api/<productosController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] ProductoDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.InfoProductoStkD(id, admId);

            response = new ApiResponse<List<InfoProdStkD>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<InfoProdStkBox>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult InfoProductoStkBoxes(string id, string adm, string depo, string box = "")
        {
            ApiResponse<List<InfoProdStkBox>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.InfoProductoStkBoxes(id, adm, depo, box);

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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.InfoProductoLP(id);

            response = new ApiResponse<List<InfoProdLP>>(res);

            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerTiposMotivo()
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _tmServicio.ObtenerTiposMotivo();
            return Ok(new ApiResponse<List<TipoMotivo>>(res));
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<NDeCYPI.InfoProdIExMesDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult InfoProdIExMes(string admId, string pId, int meses)
        {
            ApiResponse<List<NDeCYPI.InfoProdIExMesDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.InfoProdIExMes(admId, pId, meses);

            response = new ApiResponse<List<NDeCYPI.InfoProdIExMesDto>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<NDeCYPI.InfoProdIExSemanaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult InfoProdIExSemana(string admId, string pId, int semanas)
        {
            ApiResponse<List<NDeCYPI.InfoProdIExSemanaDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.InfoProdIExSemana(admId, pId, semanas);

            response = new ApiResponse<List<NDeCYPI.InfoProdIExSemanaDto>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<ProductoNCPISustitutoDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult InfoProdSustituto(string pId, string tipo, string admId, bool soloProv)
        {
            ApiResponse<List<ProductoNCPISustitutoDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.InfoProdSustituto(pId, tipo, admId, soloProv);

            response = new ApiResponse<List<ProductoNCPISustitutoDto>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<NDeCYPI.InfoProductoDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult InfoProd(string pId)
        {
            ApiResponse<List<NDeCYPI.InfoProductoDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.InfoProd(pId);

            response = new ApiResponse<List<NDeCYPI.InfoProductoDto>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<TipoAjusteDeStockDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerTipoDeAjusteDeStock()
        {
            ApiResponse<List<TipoAjusteDeStockDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.ObtenerTipoDeAjusteDeStock();

            response = new ApiResponse<List<TipoAjusteDeStockDto>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<AjustePrevioCargadoDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerAJPreviosCargados(string admId)
        {
            ApiResponse<List<AjustePrevioCargadoDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.ObtenerAJPreviosCargados(admId);

            response = new ApiResponse<List<AjustePrevioCargadoDto>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<DevolucionRevertidoDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerAJREVERTIDO(string ajId)
        {
            ApiResponse<List<AjusteRevertidoDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.ObtenerAJREVERTIDO(ajId);

            response = new ApiResponse<List<AjusteRevertidoDto>>(res);

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RespuestaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ConfirmarAjusteStk(ConfirmarAjusteStkRequest request)
        {
            ApiResponse<List<RespuestaDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.ConfirmarAjusteStk(request);

            response = new ApiResponse<List<RespuestaDto>>(res);

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RespuestaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult AJ_CargaConteosPrevios(CargarJsonGenRequest request)
        {
            ApiResponse<RespuestaDto> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.AJ_CargaConteosPrevios(request.json_str, request.admid);

            response = new ApiResponse<RespuestaDto>(res);

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RespuestaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult DV_CargaConteosPrevios(CargarJsonGenRequest request)
        {
            ApiResponse<RespuestaDto> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.DV_CargaConteosPrevios(request.json_str, request.admid);

            response = new ApiResponse<RespuestaDto>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<DevolucionPrevioCargadoDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerDPPreviosCargados(string admId, string ctaId)
        {
            ApiResponse<List<DevolucionPrevioCargadoDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.ObtenerDPPreviosCargados(admId, ctaId);

            response = new ApiResponse<List<DevolucionPrevioCargadoDto>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<DevolucionRevertidoDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ObtenerDPREVERTIDO(string dvCompte)
        {
            ApiResponse<List<DevolucionRevertidoDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.ObtenerDPREVERTIDO(dvCompte);

            response = new ApiResponse<List<DevolucionRevertidoDto>>(res);

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RespuestaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ConfirmarDP(ConfirmarDPRequest request)
        {
            ApiResponse<List<RespuestaDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.ConfirmarDP(request);

            response = new ApiResponse<List<RespuestaDto>>(res);

            return Ok(response);
        }
        #endregion

        #region Acciones para modulo RPR

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<AutorizacionPendienteDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RPRAutorizacionPendiente(string adm)
        {
            ApiResponse<List<AutorizacionPendienteDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.RPRObtenerAutorizacionPendiente(adm);

            response = new ApiResponse<List<AutorizacionPendienteDto>>(res);

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RespuestaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RPRCargar(CargarJsonGenRequest request)
        {
            ApiResponse<List<RespuestaDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.RPRConfirma(request.rp, request.adm_id);

            response = new ApiResponse<List<RespuestaDto>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RPRxULDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RPRxUL(string rp)
        {
            ApiResponse<List<RPRxULDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.RPRxUL(rp);

            response = new ApiResponse<List<RPRxULDto>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<RPRxULDetalleDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RPRxULDetalle(string ulId)
        {
            ApiResponse<List<RPRxULDetalleDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.RPRxULDetalle(ulId);

            response = new ApiResponse<List<RPRxULDetalleDto>>(res);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<JsonDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RPRObtenerJson(string rp)
        {
            ApiResponse<List<JsonDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.RPRObtenerConteos(rp);

            response = new ApiResponse<List<RPRVerConteoDto>>(res);

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult TRCargarCtrlSalida(TRProdsCtrlSalDto prods) ////PARA FACTORIZAR ACA ####################################
        {
            ApiResponse<RespuestaDto> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");

            if (prods.ProdsCargar.Count == 0)
            {
                return BadRequest("No se recepcionó dato alguno.");

            }

            var json = JsonConvert.SerializeObject(prods.ProdsCargar);

            /*
             {
                "item": 1,
                "ope": "RPR",
                "nro_auto": "00-00001234",
                "cta_id": "C0001234",
                "usu_id": "super",
                "adm_id": "0000",
                "p_id": "000145",
                "bulto_up": "6",
                "bulto": "100",
                "uni_suelta": "0",
                "vto": null,
                "cantidad": "6000",
                "nro_tra": "00-0000123415"
             },
             */

            RespuestaDto res = _productosSv.TRCargarCtrlSalida(json);
            response = new ApiResponse<RespuestaDto>(res);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RegistroResponseDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]

        public IActionResult RPRRegistrar(List<ProductoGenDto> prods, bool esMod = false)   ////PARA FACTORIZAR ACA ####################################
        {
            ApiResponse<RegistroResponseDto> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");

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

            var res = _productosSv.RPRRegistrarProductos(json, esMod);
            response = new ApiResponse<RegistroResponseDto>(res);
            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<AutoComptesPendientesDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RPRObtenerAutoComptesPendientes(string adm_id)
        {
            ApiResponse<List<AutoComptesPendientesDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            if (string.IsNullOrEmpty(adm_id))
                return BadRequest("No se recepciono dato alguno.");

            var result = _productosSv.RPRObtenerComptesPendientes(adm_id);
            response = new ApiResponse<List<AutoComptesPendientesDto>>(result);
            return Ok(response);
        }


        [HttpPost]
        [Route("[action]")]
        public IActionResult ValidaProductoCarrito(TiProductoCarritoDto request)
        {
            if (request == null) return BadRequest("No se recepcionaron los datos");

            RespuestaDto resp = _productosSv.ValidarProductoCarrito(request);

            var response = new ApiResponse<RespuestaDto>(resp);
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

        [HttpPost]
        [Route("[action]")]
        public IActionResult ObtenerTRPendientes(ObtenerTRPendientesRequest request)
        {
            if (request == null) return BadRequest("No se recepcionaron los datos");

            List<TRPendienteDto> resp = _productosSv.ObtenerTRPendientes(request);

            var response = new ApiResponse<List<TRPendienteDto>>(resp);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerTRAutSucursales(string admId)
        {
            if (admId == null) return BadRequest("No se recepcionaron los datos");

            List<TRAutSucursalesDto> resp = _productosSv.ObtenerTRAut_Sucursales(admId);

            var response = new ApiResponse<List<TRAutSucursalesDto>>(resp);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerTRAutPI(string admId, string admIdLista)
        {
            if (admId == null) return BadRequest("No se recepcionaron los datos");

            List<TRAutPIDto> resp = _productosSv.ObtenerTRAut_PI(admId, admIdLista);

            var response = new ApiResponse<List<TRAutPIDto>>(resp);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerTRAutPIDetalle(string piCompte)
        {
            if (piCompte == null) return BadRequest("No se recepcionaron los datos");

            List<TRAutPIDetalleDto> resp = _productosSv.ObtenerTRAut_PI_Detalle(piCompte);

            var response = new ApiResponse<List<TRAutPIDetalleDto>>(resp);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerTRAutDepositos(string admId)
        {
            if (admId == null) return BadRequest("No se recepcionaron los datos");

            List<TRAutDepoDto> resp = _productosSv.ObtenerTRAut_Depositos(admId);

            var response = new ApiResponse<List<TRAutDepoDto>>(resp);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult TRAutAnaliza(string listaPi, string listaDepo, bool stkExistente, bool sustituto, int palletNro)
        {
            if (string.IsNullOrWhiteSpace(listaPi) || string.IsNullOrWhiteSpace(listaDepo)) return BadRequest("No se recepcionaron los datos");

            List<TRAutAnalizaDto> resp = _productosSv.TRAutAnaliza(listaPi, listaDepo, stkExistente, sustituto, palletNro);

            var response = new ApiResponse<List<TRAutAnalizaDto>>(resp);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult TRObtenerSustituto(string pId, string listaDepo, string admIdDes, string tipo)
        {
            if (string.IsNullOrWhiteSpace(pId) || string.IsNullOrWhiteSpace(tipo)) return BadRequest("No se recepcionaron los datos");

            List<TRProductoParaAgregar> resp = _productosSv.TRObtenerSustituto(pId, listaDepo == "N" ? "" : listaDepo, admIdDes, tipo);

            var response = new ApiResponse<List<TRProductoParaAgregar>>(resp);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult TRConfirmaAutorizaciones(TRConfirmaRequest request)
        {
            ApiResponse<List<RespuestaDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.TRConfirmaAutorizaciones(request);

            //response = new ApiResponse<RespuestaDto>(res);
            response = new ApiResponse<List<RespuestaDto>>(res);
            response = new ApiResponse<List<RespuestaDto>>(res);

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult TRValidarTransferencia(TRValidarTransferenciaRequest request)
        {
            ApiResponse<List<RespuestaDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.TRValidarTransferencia(request);

            response = new ApiResponse<List<RespuestaDto>>(res);

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoNCPIDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult NCPICargarListaDeProductos(NCPICargarListaDeProductosRequest request)
        {
            ApiResponse<List<ProductoNCPIDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.NCPICargarListaDeProductos(request);

            response = new ApiResponse<List<ProductoNCPIDto>>(res);

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoParaOcDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult CargarProductosDeOC(CargarProductoParaOcRequest request)
        {
            ApiResponse<List<ProductoParaOcDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.CargarProductosDeOC(request);
            response = new ApiResponse<List<ProductoParaOcDto>>(res);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult CargarTopesDeOC(string admId)
        {
            ApiResponse<List<OrdenDeCompraTopeDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.CargarTopesDeOC(admId);
            response = new ApiResponse<List<OrdenDeCompraTopeDto>>(res);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrdenDeCompraConceptoDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult CargarResumenDeOC(CargarResumenDeOCRequest request)
        {
            ApiResponse<List<OrdenDeCompraConceptoDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.CargarResumenDeOC(request);
            response = new ApiResponse<List<OrdenDeCompraConceptoDto>>(res);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ConfirmarOC(ConfirmarOCRequest request)
        {
            ApiResponse<RespuestaDto> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.ConfirmarOC(request);

            response = new ApiResponse<RespuestaDto>(res.First());

            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerOrdenDeCompraPorOcCompte(string ocCompte)
        {
            ApiResponse<List<OrdenDeCompraDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.ObtenerOrdenDeCompraPorOcCompte(ocCompte);
            response = new ApiResponse<List<OrdenDeCompraDto>>(res);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrdenDeCompraConsultaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult CargarOrdenDeCompraConsultaLista(BuscarOrdenesDeCompraRequest request)
        {
            OrdenDeCompraConsultaDto reg = new();
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.CargarOrdenDeCompraConsultaLista(request);
            if (res.Count > 0)
            {
                reg = res.First();
            }
            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = reg.total_registros,
                PageSize = request.Registros??0,
                CurrentPage = request.Pagina??0,
                TotalPages = reg.total_paginas,
                HasNextPage = (request.Pagina??0) < reg.total_paginas,
                HasPreviousPage = (request.Pagina??0) > 1,
                NextPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(CargarOrdenDeCompraConsultaLista)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(CargarOrdenDeCompraConsultaLista)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<OrdenDeCompraConsultaDto>>(res)
            {
                Meta = metadata
            };
            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult CargarDetalleDeOC(string oc_compte)
        {
            ApiResponse<List<OrdenDeCompraDetalleDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.CargarDetalleDeOC(oc_compte);
            response = new ApiResponse<List<OrdenDeCompraDetalleDto>>(res);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult CargarRprAsociadaDeOC(string oc_compte)
        {
            ApiResponse<List<OrdenDeCompraRprAsociadasDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.CargarRprAsociadaDeOC(oc_compte);
            response = new ApiResponse<List<OrdenDeCompraRprAsociadasDto>>(res);
            return Ok(response);
        }

        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult ModificarOC(ModificarOCRequest request)
        {
            ApiResponse<RespuestaDto> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.ModificarOC(request);
            response = new ApiResponse<RespuestaDto>(res.First());
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoNCPIDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult NCPICargarListaDeProductosPag(NCPICargarListaDeProductosRequest request)
        {
            ProductoNCPIDto reg = new ProductoNCPIDto();
            //ApiResponse<List<ProductoNCPIDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.NCPICargarListaDeProductos(request);
            if (res.Count > 0)
            {
                reg = res.First();
            }
            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = reg.total_registros,
                PageSize = request.Registros??0,
                CurrentPage = request.Pagina??0,
                TotalPages = reg.total_paginas,
                HasNextPage = (request.Pagina??0) < reg.total_paginas,
                HasPreviousPage =(request.Pagina??0)> 1,
                NextPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(NCPICargarListaDeProductosPag)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(NCPICargarListaDeProductosPag)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<ProductoNCPIDto>>(res)
            {
                Meta = metadata
            };
            //response = new ApiResponse<List<ProductoNCPIDto>>(res);
            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));
            return Ok(response);
        }

        [HttpGet]
        //[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrdenDeCompraListDto>))]
        //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult CargarOrdenesDeCompraList(string ctaId, string admId, string usuId)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            List<OrdenDeCompraListDto> oc = _productosSv.CargarOrdenesDeCompraList(ctaId, admId, usuId);
            var lista = _mapper.Map<List<OrdenDeCompraListDto>>(oc);

            var response = new ApiResponse<List<OrdenDeCompraListDto>>(lista);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<ProductoNCPIDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult NCPICargarListaDeProductosPag2(NCPICargarListaDeProductos2Request request)
        {
            ProductoNCPIDto reg = new();
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.NCPICargarListaDeProductos2(request);
            if (res.Count > 0)
            {
                reg = res.First();
            }
            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = reg.total_registros,
                PageSize = request.Registros??0,
                CurrentPage = request.Pagina??0,
                TotalPages = reg.total_paginas,
                HasNextPage =(request.Pagina??0)< reg.total_paginas,
                HasPreviousPage =(request.Pagina??0)> 1,
                NextPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(NCPICargarListaDeProductosPag2)) ?? "").ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(request, Url.RouteUrl(nameof(NCPICargarListaDeProductosPag2)) ?? "").ToString(),

            };

            var response = new ApiResponse<IEnumerable<ProductoNCPIDto>>(res)
            {
                Meta = metadata
            };
            //response = new ApiResponse<List<ProductoNCPIDto>>(res);
            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<NCPICargaPedidoResponse>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult NCPICargaPedido(NCPICargaPedidoRequest request)
        {
            ApiResponse<List<NCPICargaPedidoResponse>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            var res = _productosSv.NCPICargaPedido(request);

            response = new ApiResponse<List<NCPICargaPedidoResponse>>(res);

            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult TRVerConteos(string ti)
        {
            if (string.IsNullOrWhiteSpace(ti)) return BadRequest("No se recepcionaron los datos");

            List<TRVerConteosDto> resp = _productosSv.TRVerConteos(ti);

            var response = new ApiResponse<List<TRVerConteosDto>>(resp);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ControlSalidaTI(string ti, string adm, string usu)
        {
            if (string.IsNullOrEmpty(ti) || string.IsNullOrEmpty(adm) || string.IsNullOrEmpty(usu))
            {
                return BadRequest("Algunos de los Parametros necesarios para el Control de Salida no se recepcionaron.");
            }
            if (string.IsNullOrWhiteSpace(ti) || string.IsNullOrWhiteSpace(adm) || string.IsNullOrWhiteSpace(usu))
            {
                return BadRequest("Algunos de los Parametros necesarios para el Control de Salida no se recepcionaron.");
            }

            var resp = _productosSv.TRCtrlSalida(ti, adm, usu);

            var response = new ApiResponse<RespuestaDto>(resp);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult TRVerCtrlSalida(string tr, string user)
        {
            if (string.IsNullOrEmpty(tr) || string.IsNullOrEmpty(user))
            {
                return BadRequest("Algunos de los Parametros necesarios para el Control de Salida no se recepcionaron.");
            }
            if (string.IsNullOrWhiteSpace(tr) || string.IsNullOrWhiteSpace(user))
            {
                return BadRequest("Algunos de los Parametros necesarios para el Control de Salida no se recepcionaron.");
            }

            List<ProductoGenDto> resp = _productosSv.TRVerCtrlSalida(tr, user);

            var response = new ApiResponse<List<ProductoGenDto>>(resp);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult TRNuevaSinAuto(string tipo, string adm, string usu)
        {
            if (string.IsNullOrEmpty(tipo) || string.IsNullOrEmpty(adm) || string.IsNullOrEmpty(usu))
            {
                return BadRequest("Algunos de los Parametros necesarios para el Control de Salida no se recepcionaron.");
            }
            if (string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(adm) || string.IsNullOrWhiteSpace(usu))
            {
                return BadRequest("Algunos de los Parametros necesarios para el Control de Salida no se recepcionaron.");
            }

            var resp = _productosSv.TRNuevaSinAuto(tipo, adm, usu);

            var response = new ApiResponse<TIRespuestaDto>(resp);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult TRValidaPendiente(string usu)
        {
            if (string.IsNullOrEmpty(usu) || string.IsNullOrWhiteSpace(usu))
            {
                return BadRequest("Algunos de los Parametros necesarios para el Control de Salida no se recepcionaron.");
            }

            var resp = _productosSv.TRValidaPendiente(usu);

            var response = new ApiResponse<TIRespuestaDto>(resp);
            return Ok(response);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult TR_Confirma(TIRequestConfirmaDto confirma)
        {
            if (confirma == null)
            {
                return BadRequest("No se recepcionó los datos para confirmar la TR");
            }
            var resp = _productosSv.TR_Confirma(confirma);

            var response = new ApiResponse<RespuestaDto>(resp);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult BuscarFechaVto(string pId, string bId)
        {


            if (string.IsNullOrWhiteSpace(pId) || string.IsNullOrWhiteSpace(pId))
            {
                throw new NegocioException("No se puede buscar la Fecha de Vencimiento. Falta algunos de los datos necesarios. Intentelo de nuevo.");
            }

            if (string.IsNullOrEmpty(pId) || string.IsNullOrEmpty(pId))
            {
                throw new NegocioException("No se puede buscar la Fecha de Vencimiento. Falta algunos de los datos necesarios. Intentelo de nuevo.");
            }

            var res = _prodDepoSv.ObtenerFechaVencimiento(pId, bId);
            if (res == null)
            {
                return Ok(new ApiResponse<ProductoDepositoDto>(new ProductoDepositoDto()));
            }
            else
            {
                var prod = _mapper.Map<ProductoDeposito, ProductoDepositoDto>(res);
                return Ok(new ApiResponse<ProductoDepositoDto>(prod));
            }
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerBoxInfo(string box_id)
        {
            if (string.IsNullOrWhiteSpace(box_id) || string.IsNullOrWhiteSpace(box_id))
            {
                throw new NegocioException("No se ha recepcionado el BOX.");
            }
            var res = _productosSv.ObtenerBoxInfo(box_id);
            if (res == null)
            {
                return NotFound("No se encontro el Box.");
            }
            else
            {
                return Ok(new ApiResponse<BoxInfoDto>(res));
            }
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ConsultaUL(string tipo, long fecD, long fecH, string admId)
        {
            if (string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(tipo))
            {
                throw new NegocioException("No se recepcionó que tipo de consulta desea realizar.");
            }
            if (string.IsNullOrWhiteSpace(admId) || string.IsNullOrWhiteSpace(admId))
            {
                throw new NegocioException("No se recepcionó que Sucursal Administrativa es la que se desea consultar.");
            }

            DateTime? desde = fecD == 0 ? null : new DateTime(fecD);
            DateTime? hasta = fecH == 0 ? null : new DateTime(fecH);

            var res = _productosSv.ConsultarUL(tipo, admId, desde, hasta);
            return Ok(new ApiResponse<List<ConsULDto>>(res));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerBoxInfoStk(string box_id)
        {
            if (string.IsNullOrWhiteSpace(box_id) || string.IsNullOrWhiteSpace(box_id))
            {
                throw new NegocioException("No se ha recepcionado el BOX.");
            }
            var res = _productosSv.ObtenerBoxInfoStk(box_id);
            if (res == null)
            {
                return NotFound("No se encontró el detalle del STK.");
            }
            else
            {
                return Ok(new ApiResponse<List<BoxInfoStkDto>>(res));
            }
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerBoxInfoMovStk(string box_id, string? sm_tipo, long desde, long hasta)
        {
            if (string.IsNullOrWhiteSpace(box_id) || string.IsNullOrWhiteSpace(box_id))
            {
                throw new NegocioException("No se ha recepcionado el BOX.");
            }
            sm_tipo = sm_tipo ?? "%";

            var res = _productosSv.ObtenerBoxInfoMovStk(box_id, sm_tipo, new DateTime(desde), new DateTime(hasta));
            if (res == null || res.Count == 0)
            {
                return NotFound("No se encontraró el detalle de Movimientos.");
            }
            else
            {
                return Ok(new ApiResponse<List<BoxInfoMovStkDto>>(res));
            }
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerMedidas()
        {
            var res = _productosSv.ObtenerMedidas();
            return Ok(new ApiResponse<List<MedidaDto>>(res));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerIVASituacion()
        {
            var res = _productosSv.ObtenerIVASituacion();
            return Ok(new ApiResponse<List<IVASituacionDto>>(res));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerIVAAlicuotas()
        {
            var res = _productosSv.ObtenerIVAAlicuotas();
            return Ok(new ApiResponse<List<IVAAlicuotaDto>>(res));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerBarradoDeProd(string p_id)
        {
            if (string.IsNullOrEmpty(p_id))
            {
                return BadRequest("No se recepcionó el identificador del producto");
            }
            var res = _productosSv.ObtenerBarradoDeProd(p_id);
            return Ok(new ApiResponse<List<ProductoBarradoDto>>(res));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult BuscarBarrado(string p_id, string barradoId)
        {
            if (string.IsNullOrEmpty(barradoId))
            {
                return BadRequest("No se recepcionó el identificador del barrado");
            }
            var res = _productosSv.BuscarBarrado(p_id, barradoId);
            return Ok(new ApiResponse<ProductoBarradoDto>(res));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult ObtenerLimitesStkLista(string p_id)
        {
            if (string.IsNullOrEmpty(p_id))
            {
                return BadRequest("No se recepcionó el identificador del producto");
            }
            var res = _productosSv.ObtenerLimitesStkLista(p_id);

            return Ok(new ApiResponse<List<LimiteStkDto>>(res));
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult BuscaLimite(string p_id, string adm_id)
        {
            if (string.IsNullOrEmpty(p_id))
            {
                return BadRequest("No se recepcionó el identificador del producto");
            }

            if (string.IsNullOrEmpty(adm_id))
            {
                return BadRequest("No se recepcionó la sucursal");
            }

            var res = _productosSv.ObtenerLimiteStkDato(p_id, adm_id);

            return Ok(new ApiResponse<LimiteStkDto>(res));
        }

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult OCValidar(OCValidarRequest request)
		{
			ApiResponse<RespuestaDto> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
			var res = _productosSv.OCValidar(request.oc_compte, request.cta_id);

			response = new ApiResponse<RespuestaDto>(res);

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
