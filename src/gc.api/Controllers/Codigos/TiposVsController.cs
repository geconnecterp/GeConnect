using AutoMapper;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
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
		private readonly IRepartidorServicio _repartidorServicio;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ITipoContactoServicio _tipoContactoServicio;
		private readonly ITipoObsServicio _tipoObsServicio;
		private readonly ITipoOpeIvaServicio _tipoOpeIvaServicio;

		public TiposVsController( IMapper mapper, IUriService uriService, ILogger<TiposVsController> logger, ICondicionAfipServicio condicionAfipServicio,
								ICondicionIBServicio condicionIBServicio, IDepartamentoServicio departamentoServicio, IFormaDePagoServicio formaDePagoServicio,
								INaturalezaJuridicaServicio naturalezaJuridicaServicio, IProvinciaServicio provinciaServicio, ITipoCanalServicio tipoCanalServicio,
								ITipoCuentaBcoServicio tipoCuentaBcoServicio, ITiposDocumentoServicio tiposDocumentoServicio, IListaPrecioServicio listaPrecioServicio,
								IVendedorServicio vendedorServicio, IRepartidorServicio repartidorServicio, IFinancieroServicio financieroServicio,
								ITipoContactoServicio tipoContactoServicio, ITipoObsServicio tipoObsServicio, ITipoOpeIvaServicio tipoOpeIvaServicio)
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
			_repartidorServicio = repartidorServicio;
			_financieroServicio = financieroServicio;
			_tipoContactoServicio = tipoContactoServicio;
			_tipoObsServicio = tipoObsServicio;
			_tipoOpeIvaServicio = tipoOpeIvaServicio;
        }

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetCondicionesAfipLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<CondicionAfipDto> condAfip = _condicionAfipServicio.GetCondicionesAfipLista();
			var lista = _mapper.Map<List<CondicionAfipDto>>(condAfip);

			var response = new ApiResponse<List<CondicionAfipDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetCondicionIBLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            List<CondicionIBDto> condIB = _condicionIBServicio.GetCondicionIBLista();
			var lista = _mapper.Map<List<CondicionIBDto>>(condIB);

			var response = new ApiResponse<List<CondicionIBDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetDepartamentoPorProvinciaLista(string prov_id)
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<DepartamentoDto> deptos = _departamentoServicio.GetDepartamentoPorProvinciaLista(prov_id);
			var lista = _mapper.Map<List<DepartamentoDto>>(deptos);

			var response = new ApiResponse<List<DepartamentoDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetFormaDePagoLista(string tipo = "C")
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<FormaDePagoDto> formaPago = _formaDePagoServicio.GetFormaDePagoLista(tipo);
			var lista = _mapper.Map<List<FormaDePagoDto>>(formaPago);

			var response = new ApiResponse<List<FormaDePagoDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetNaturalezaJuridicaLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<NaturalezaJuridicaDto> natJud = _naturalezaJuridicaServicio.GetNaturalezaJuridicaLista();
			var lista = _mapper.Map<List<NaturalezaJuridicaDto>>(natJud);

			var response = new ApiResponse<List<NaturalezaJuridicaDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetProvinciaLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<ProvinciaDto> provincia = _provinciaServicio.GetProvinciaLista();
			var lista = _mapper.Map<List<ProvinciaDto>>(provincia);

			var response = new ApiResponse<List<ProvinciaDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetTipoCanalLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<TipoCanalDto> tipoCanal = _tipoCanalServicio.GetTipoCanalLista();
			var lista = _mapper.Map<List<TipoCanalDto>>(tipoCanal);

			var response = new ApiResponse<List<TipoCanalDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetTipoCuentaBcoLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<TipoCuentaBcoDto> tipoCuentaBco = _tipoCuentaBcoServicio.GetTipoCuentaBcoLista();
			var lista = _mapper.Map<List<TipoCuentaBcoDto>>(tipoCuentaBco);

			var response = new ApiResponse<List<TipoCuentaBcoDto>>(lista);
			return Ok(response);
		}

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetTipoDocumentoLista()
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            List<TipoDocumentoDto> tipoDoc = _tiposDocumentoServicio.GetTipoDocumentoLista();
            var lista = _mapper.Map<List<TipoDocumentoDto>>(tipoDoc);

            var response = new ApiResponse<List<TipoDocumentoDto>>(lista);
            return Ok(response);
        }

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetListaDePreciosLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<ListaPrecioDto> listaPrecio = _listaPrecioServicio.GetListaPrecio();
			var lista = _mapper.Map<List<ListaPrecioDto>>(listaPrecio);

			var response = new ApiResponse<List<ListaPrecioDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetVendedoresLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<VendedorDto> vendedores = _vendedorServicio.GetVendedorLista();
			var lista = _mapper.Map<List<VendedorDto>>(vendedores);

			var response = new ApiResponse<List<VendedorDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetRepartidoresLista()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<RepartidorDto> repartidores = _repartidorServicio.GetRepartidorLista();
			var lista = _mapper.Map<List<RepartidorDto>>(repartidores);

			var response = new ApiResponse<List<RepartidorDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetFinancierosPorTipoCfLista(string tcf_id)
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<FinancieroDto> financiero = _financieroServicio.GetFinancierosPorTipoCfLista(tcf_id);
			var lista = _mapper.Map<List<FinancieroDto>>(financiero);

			var response = new ApiResponse<List<FinancieroDto>>(lista);
			return Ok(response);
		}

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetTipoContactoLista(string tipo = "P")
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<TipoContactoDto> tipoCanal = _tipoContactoServicio.GetTipoContactoLista(tipo);
			var lista = _mapper.Map<List<TipoContactoDto>>(tipoCanal);

			var response = new ApiResponse<List<TipoContactoDto>>(lista);
			return Ok(response);
		}

        [HttpGet]
        [Route("[action]")]
        public IActionResult GetTipoObsLista(string tipo = "C")
        {
            _logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<TipoObsDto> tipoCanal = _tipoObsServicio.GetTiposDeObs(tipo);
            var lista = _mapper.Map<List<TipoObsDto>>(tipoCanal);

            var response = new ApiResponse<List<TipoObsDto>>(lista);
            return Ok(response);
        }

		[HttpGet]
		[Route("[action]")]
		public IActionResult GetTipoOpeIva()
		{
			_logger.LogInformation($"{GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
			List<TipoOpeIvaDto> tipoOpe = _tipoOpeIvaServicio.GetTipoOpeIva();
			var lista = _mapper.Map<List<TipoOpeIvaDto>>(tipoOpe);

			var response = new ApiResponse<List<TipoOpeIvaDto>>(lista);
			return Ok(response);
		}
		//
	}
}
