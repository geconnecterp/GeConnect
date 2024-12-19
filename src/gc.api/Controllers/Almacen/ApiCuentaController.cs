namespace gc.api.Controllers.Almacen
{
    using AutoMapper;
    using gc.api.core.Contratos.Servicios;
    using gc.api.core.Entidades;
    using gc.infraestructura.Core.EntidadesComunes;
    using gc.infraestructura.Core.Interfaces;
    using gc.infraestructura.Core.Responses;
    using gc.infraestructura.Dtos;
    using gc.infraestructura.Dtos.Almacen;
    using gc.infraestructura.Dtos.CuentaComercial;
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
    public class ApiCuentaController : ControllerBase
    {
        private readonly IMapper _mapper;
        private ICuentaServicio _cuentasSv;
        private ITipoNegocioServicio _tipoNegocioServicio;
        private IZonaServicio _zonaServicio;
        private readonly IUriService _uriService;
        private readonly ILogger<ApiCuentaController> _logger;

        public ApiCuentaController(IZonaServicio zonaServicio, ICuentaServicio servicio, ITipoNegocioServicio tipoNegocioServicio, 
                                   IMapper mapper, IUriService uriService, ILogger<ApiCuentaController> logger)
        {
            _cuentasSv = servicio;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
            _tipoNegocioServicio = tipoNegocioServicio;
            _zonaServicio = zonaServicio;
        }


        [HttpGet(Name = nameof(Getcuentass))]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<CuentaDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Getcuentass([FromQuery] QueryFilters filters)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var cuentass = _cuentasSv.GetAll(filters);
            var cuentasDtos = _mapper.Map<IEnumerable<CuentaDto>>(cuentass);

            // presentando en el header información basica sobre la paginación
            var metadata = new MetadataGrid
            {
                TotalCount = cuentass.TotalCount,
                PageSize = cuentass.PageSize,
                CurrentPage = cuentass.CurrentPage,
                TotalPages = cuentass.TotalPages,
                HasNextPage = cuentass.HasNextPage,
                HasPreviousPage = cuentass.HasPreviousPage,
                NextPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getcuentass))).ToString(),
                PreviousPageUrl = _uriService.GetPostPaginationUri(filters, Url.RouteUrl(nameof(Getcuentass))).ToString(),

            };

            var response = new ApiResponse<IEnumerable<CuentaDto>>(cuentasDtos)
            {
                Meta = metadata
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult GetCuentaComercialLista(string texto, char tipo)
        {
            ApiResponse<List<CuentaDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _cuentasSv.GetCuentaComercialLista(texto, tipo);

            response = new ApiResponse<List<CuentaDto>>(res);

            return Ok(response);
        }

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetOCxCuenta(string cta_id)
		{
			ApiResponse<List<RPROrdenDeCompraDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			var res = _cuentasSv.GetOCporCuenta(cta_id);

			response = new ApiResponse<List<RPROrdenDeCompraDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetOCDetalle(string oc_compte)
		{
			ApiResponse<List<RPROrdenDeCompraDetalleDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			var res = _cuentasSv.GetDetalleDeOC(oc_compte);

			response = new ApiResponse<List<RPROrdenDeCompraDetalleDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaABMDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetCuentaParaABM(string cta_id)
		{
			ApiResponse<List<CuentaABMDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			var res = _cuentasSv.GetCuentaParaABM(cta_id);

			response = new ApiResponse<List<CuentaABMDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaFPDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetCuentaFormaDePago(string cta_id)
		{
			ApiResponse<List<CuentaFPDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			var res = _cuentasSv.GetCuentaFormaDePago(cta_id);

			response = new ApiResponse<List<CuentaFPDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaContactoDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetCuentaContactos(string cta_id)
		{
			ApiResponse<List<CuentaContactoDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _cuentasSv.GetCuentContactos(cta_id);

			response = new ApiResponse<List<CuentaContactoDto>>(res);

			return Ok(response);
		}

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaContactoDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult GetCuentaContactosPorCuentaYTC(string cta_id, string tc_id)
        {
            ApiResponse<List<CuentaContactoDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _cuentasSv.GetCuentContactosporCuentaYTC(cta_id, tc_id);

            response = new ApiResponse<List<CuentaContactoDto>>(res);

            return Ok(response);
        }

        [HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaObsDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetCuentaObs(string cta_id)
		{
			ApiResponse<List<CuentaObsDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _cuentasSv.GetCuentaObs(cta_id);

			response = new ApiResponse<List<CuentaObsDto>>(res);

			return Ok(response);
		}

		[HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaNotaDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetCuentaNota(string cta_id)
		{
			ApiResponse<List<CuentaNotaDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _cuentasSv.GetCuentaNota(cta_id);

			response = new ApiResponse<List<CuentaNotaDto>>(res);

			return Ok(response);
		}

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaNotaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [Route("[action]")]
        public IActionResult GetCuentaNotaDatos(string cta_id, string usu_id)
        {
            ApiResponse<List<CuentaNotaDto>> response;
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = _cuentasSv.GetCuentaNotaDatos(cta_id, usu_id);

            response = new ApiResponse<List<CuentaNotaDto>>(res);

            return Ok(response);
        }

        [HttpGet]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<CuentaFPDto>))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[Route("[action]")]
		public IActionResult GetFormaDePagoPorCuentaYFP(string cta_id, string fp_id)
		{
			ApiResponse<List<CuentaFPDto>> response;
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			var res = _cuentasSv.GetFormaDePagoPorCuentaYFP(cta_id, fp_id);

			response = new ApiResponse<List<CuentaFPDto>>(res);

			return Ok(response);
		}

		// GET api/<cuentasController>/5
		[HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var cuentas = await _cuentasSv.FindAsync(id);
            var datoDto = _mapper.Map<CuentaDto>(cuentas);
            var response = new ApiResponse<CuentaDto>(datoDto);
            return Ok(response);

        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetProveedorLista()
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            List<ProveedorLista> proveedores = _cuentasSv.GetProveedorLista();
            var lista = _mapper.Map<List<ProveedorListaDto>>(proveedores);

            var response = new ApiResponse<List<ProveedorListaDto>>(lista);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetTiposDeNegocio()
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            List<TipoNegocioDto> tipoNegocio = _tipoNegocioServicio.GetTiposDeNegocio();
            var lista = _mapper.Map<List<TipoNegocioDto>>(tipoNegocio);

            var response = new ApiResponse<List<TipoNegocioDto>>(lista);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetZonaLista()
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            List<ZonaDto> tipoNegocio = _zonaServicio.GetZonaLista();
            var lista = _mapper.Map<List<ZonaDto>>(tipoNegocio);

            var response = new ApiResponse<List<ZonaDto>>(lista);
            return Ok(response);
        }

        [HttpGet]
		[Route("[action]")]
		public IActionResult GetProveedorFamiliaLista(string ctaId)
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<ProveedorFamiliaListaDto> proveedores = _cuentasSv.GetProveedorFamiliaLista(ctaId);
			var lista = _mapper.Map<List<ProveedorFamiliaListaDto>>(proveedores);

			var response = new ApiResponse<List<ProveedorFamiliaListaDto>>(lista);
			return Ok(response);
		}

		// POST api/<cuentasController>
		[HttpPost]
        public async Task<IActionResult> Post(CuentaDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var cta = _mapper.Map<Cuenta>(datoDto);
            var res = await _cuentasSv.AddAsync(cta);

            var response = new ApiResponse<bool>(res);

            return Ok(response);
        }

        // PUT api/<cuentasController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] CuentaDto datoDto)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var cuentas = _mapper.Map<Cuenta>(datoDto);
            cuentas.Cta_Id = id; //garantizo que el id buscado es el que se envia al negocio
            var result = await _cuentasSv.Update(cuentas);
            var response = new ApiResponse<bool>(result);
            return Ok(response);

        }

        // DELETE api/<cuentasController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            var res = await _cuentasSv.Delete(id);
            var response = new ApiResponse<bool>(res);
            return Ok(response);
        }
    }
}
