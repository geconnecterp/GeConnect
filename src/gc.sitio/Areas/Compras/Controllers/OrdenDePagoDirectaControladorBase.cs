using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers
{
	public class OrdenDePagoDirectaControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ILogger _logger;
		public OrdenDePagoDirectaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_logger = logger;
		}

		public string TipoOPSelected
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("TipoOPSelected");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return string.Empty;
				}
				return JsonConvert.DeserializeObject<string>(txt) ?? string.Empty;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("TipoOPSelected", valor);
			}

		}

		public List<ConceptoFacturadoEnOPDDto> ListaConceptoFacturado
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaConceptoFacturado");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ConceptoFacturadoEnOPDDto>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaConceptoFacturado", valor);
			}

		}

		public List<OtroTributoEnOPDDto> ListaOtrosTributos
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaOtrosTributos");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<OtroTributoEnOPDDto>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaOtrosTributos", valor);
			}

		}

		public List<OrdenDeCompraConceptoDto> ListaTotales
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaTotales");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<OrdenDeCompraConceptoDto>();
				}
				return JsonConvert.DeserializeObject<List<OrdenDeCompraConceptoDto>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaTotales", valor);
			}

		}

		public List<ValoresDesdeOrdenDePagoDirecta> ListaValores
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaValores");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ValoresDesdeOrdenDePagoDirecta>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaValores", valor);
			}
		}

		public List<OrdenDePagoDirectaDto> ListaOrdenDePagoDirecta
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaOrdenDePagoDirecta");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<OrdenDePagoDirectaDto>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaOrdenDePagoDirecta", valor);
			}
		}
	}
}
