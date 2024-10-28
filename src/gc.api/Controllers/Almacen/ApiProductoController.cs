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
	using gc.infraestructura.Dtos.Almacen.Request;
	using gc.infraestructura.Dtos.Almacen.Response;
	using gc.infraestructura.Dtos.Almacen.Rpr;
	using gc.infraestructura.Dtos.Almacen.Tr;
    using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
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
    using System.Runtime.Intrinsics.Arm;
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
        private readonly IProductoDepositoServicio _prodDepoSv;

        public ApiProductoController(IApiProductoServicio servicio, IMapper mapper, IUriService uriService,
            ILogger<ApiProductoController> logger, ITipoMotivoServicio tipo, IProductoDepositoServicio prodDepoSv)
        {
            _productosSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
            _tmServicio = tipo;
            _prodDepoSv = prodDepoSv;
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
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<AutorizacionPendienteDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult RPRAutorizacionPendiente(string adm)
        {
            ApiResponse<List<AutorizacionPendienteDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
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
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult TRCargarCtrlSalida(TRProdsCtrlSalDto prods) ////PARA FACTORIZAR ACA ####################################
        {
            ApiResponse<RespuestaDto> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");

            if (prods.ProdsCargar.Count == 0)
            {
                return BadRequest("No se recepcionó dato alguno.");

            }

            var json = JsonConvert.SerializeObject(prods.ProdsCargar.Select(x => new { x}));

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

        public IActionResult RPRRegistrar(List<ProductoGenDto> prods)   ////PARA FACTORIZAR ACA ####################################
        {
            ApiResponse<RegistroResponseDto> response;
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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
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
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
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
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
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
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			var res = _productosSv.NCPICargarListaDeProductos(request);

			response = new ApiResponse<List<ProductoNCPIDto>>(res);

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<NCPICargaPedidoResponse>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult NCPICargaPedido(NCPICargaPedidoRequest request)
		{
			ApiResponse<List<NCPICargaPedidoResponse>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
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
        public IActionResult TRVerCtrlSalida(string tr,string user)
        {
            if (string.IsNullOrEmpty(tr)  || string.IsNullOrEmpty(user))
            {
                return BadRequest("Algunos de los Parametros necesarios para el Control de Salida no se recepcionaron.");
            }
            if (string.IsNullOrWhiteSpace(tr)  || string.IsNullOrWhiteSpace(user))
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
