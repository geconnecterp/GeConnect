using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace gc.api.Controllers.Codigos
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class TiposVsController : ControllerBase
    {
        private readonly IMapper _mapper;       
        private readonly IUriService _uriService;
        private readonly ILogger<TiposVsController> _logger;
        private readonly ICondicionAfipServicio _condicionAfipServicio;
        private readonly ICondicionIBServicio _condicionIBServicio;
        private readonly IDepartamentoServicio _departamentoServicio;
        private readonly IFormaDePagoServicio _formaDePagoServicio;
        private readonly INaturalezaJuridicaServicio _naturalezaJuridicaServicio;
        private readonly IProvinciaServicio _provinciaServicio;
        private readonly ITipoCanalServicio _tipoCanalServicio;
        private readonly ITipoCuentaBcoServicio _tipoCuentaBcoServicio;
		private readonly ITiposDocumentoServicio _tiposDocumentoServicio;
		private readonly IListaPrecioServicio _listaPrecioServicio;
		private readonly IVendedorServicio _vendedorServicio;

		public TiposVsController( IMapper mapper, IUriService uriService, ILogger<TiposVsController> logger, ICondicionAfipServicio condicionAfipServicio,
								ICondicionIBServicio condicionIBServicio, IDepartamentoServicio departamentoServicio, IFormaDePagoServicio formaDePagoServicio,
								INaturalezaJuridicaServicio naturalezaJuridicaServicio, IProvinciaServicio provinciaServicio, ITipoCanalServicio tipoCanalServicio,
								ITipoCuentaBcoServicio tipoCuentaBcoServicio, ITiposDocumentoServicio tiposDocumentoServicio, IListaPrecioServicio listaPrecioServicio,
								IVendedorServicio vendedorServicio)
        {
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
            _condicionAfipServicio = condicionAfipServicio;
            _condicionIBServicio = condicionIBServicio;
            _departamentoServicio = departamentoServicio;
            _formaDePagoServicio = formaDePagoServicio;
            _naturalezaJuridicaServicio = naturalezaJuridicaServicio;
            _provinciaServicio = provinciaServicio;
            _tipoCanalServicio = tipoCanalServicio;
            _tipoCuentaBcoServicio = tipoCuentaBcoServicio;
			_tiposDocumentoServicio = tiposDocumentoServicio;
			_listaPrecioServicio = listaPrecioServicio;
			_vendedorServicio = vendedorServicio;
        }

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetCondicionesAfipLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<CondicionAfipDto> tipoNegocio = _condicionAfipServicio.GetCondicionesAfipLista();
			var lista = _mapper.Map<List<CondicionAfipDto>>(tipoNegocio);

			var response = new ApiResponse<List<CondicionAfipDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetCondicionIBLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            List<CondicionIBDto> tipoNegocio = _condicionIBServicio.GetCondicionIBLista();
			var lista = _mapper.Map<List<CondicionIBDto>>(tipoNegocio);

			var response = new ApiResponse<List<CondicionIBDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetDepartamentoPorProvinciaLista(string prov_id)
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<DepartamentoDto> tipoNegocio = _departamentoServicio.GetDepartamentoPorProvinciaLista(prov_id);
			var lista = _mapper.Map<List<DepartamentoDto>>(tipoNegocio);

			var response = new ApiResponse<List<DepartamentoDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetFormaDePagoLista(string tipo = "C")
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<FormaDePagoDto> tipoNegocio = _formaDePagoServicio.GetFormaDePagoLista(tipo);
			var lista = _mapper.Map<List<FormaDePagoDto>>(tipoNegocio);

			var response = new ApiResponse<List<FormaDePagoDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetNaturalezaJuridicaLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<NaturalezaJuridicaDto> tipoNegocio = _naturalezaJuridicaServicio.GetNaturalezaJuridicaLista();
			var lista = _mapper.Map<List<NaturalezaJuridicaDto>>(tipoNegocio);

			var response = new ApiResponse<List<NaturalezaJuridicaDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetProvinciaLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<ProvinciaDto> tipoNegocio = _provinciaServicio.GetProvinciaLista();
			var lista = _mapper.Map<List<ProvinciaDto>>(tipoNegocio);

			var response = new ApiResponse<List<ProvinciaDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetTipoCanalLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<TipoCanalDto> tipoNegocio = _tipoCanalServicio.GetTipoCanalLista();
			var lista = _mapper.Map<List<TipoCanalDto>>(tipoNegocio);

			var response = new ApiResponse<List<TipoCanalDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetTipoCuentaBcoLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<TipoCuentaBcoDto> tipoNegocio = _tipoCuentaBcoServicio.GetTipoCuentaBcoLista();
			var lista = _mapper.Map<List<TipoCuentaBcoDto>>(tipoNegocio);

			var response = new ApiResponse<List<TipoCuentaBcoDto>>(lista);
			return Ok(response);
		}

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetTipoDocumentoLista()
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            List<TipoDocumentoDto> tipoNegocio = _tiposDocumentoServicio.GetTipoDocumentoLista();
            var lista = _mapper.Map<List<TipoDocumentoDto>>(tipoNegocio);

            var response = new ApiResponse<List<TipoDocumentoDto>>(lista);
            return Ok(response);
        }

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetListaDePreciosLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<ListaPrecioDto> tipoNegocio = _listaPrecioServicio.GetListaPrecio();
			var lista = _mapper.Map<List<ListaPrecioDto>>(tipoNegocio);

			var response = new ApiResponse<List<ListaPrecioDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetVendedoresLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<VendedorDto> tipoNegocio = _vendedorServicio.GetVendedorLista();
			var lista = _mapper.Map<List<VendedorDto>>(tipoNegocio);

			var response = new ApiResponse<List<VendedorDto>>(lista);
			return Ok(response);
		}
	}
}
