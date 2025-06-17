using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers
{
	public class OrdenDePagoAProveedorControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ILogger _logger;
		public OrdenDePagoAProveedorControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_logger = logger;
		}

		public string CtaIdSelected
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("CtaIdSelected");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return string.Empty;
				}
				return JsonConvert.DeserializeObject<string>(txt) ?? string.Empty;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("CtaIdSelected", valor);
			}

		}

		public string CtaValoresANombre
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("CtaValoresANombre");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return string.Empty;
				}
				return JsonConvert.DeserializeObject<string>(txt) ?? string.Empty;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("CtaValoresANombre", valor);
			}

		}

		public List<OPValidacionPrevDto> OPValidacionPrevLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("OPValidacionPrevLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<OPValidacionPrevDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("OPValidacionPrevLista", json);
			}
		}

		public List<OPDebitoYCreditoDelProveedorDto> OPDebitoLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("OPDebitoLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<OPDebitoYCreditoDelProveedorDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("OPDebitoLista", json);
			}
		}

		public List<OPDebitoYCreditoDelProveedorDto> OPDebitoOriginalLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("OPDebitoOriginalLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<OPDebitoYCreditoDelProveedorDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("OPDebitoOriginalLista", json);
			}
		}

		public List<OPDebitoYCreditoDelProveedorDto> OPDebitoNuevaLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("OPDebitoNuevaLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<OPDebitoYCreditoDelProveedorDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("OPDebitoNuevaLista", json);
			}
		}

		public List<OPDebitoYCreditoDelProveedorDto> OPCreditoLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("OPCreditoLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<OPDebitoYCreditoDelProveedorDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("OPCreditoLista", json);
			}
		}

		public List<OPDebitoYCreditoDelProveedorDto> OPCreditoOriginalLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("OPCreditoOriginalLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<OPDebitoYCreditoDelProveedorDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("OPCreditoOriginalLista", json);
			}
		}

		public List<OPDebitoYCreditoDelProveedorDto> OPCreditoNuevaLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("OPCreditoNuevaLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<OPDebitoYCreditoDelProveedorDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("OPCreditoNuevaLista", json);
			}
		}

		public List<RetencionesDesdeObligYCredDto> OPRetencionesDesdeObligYCredLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("OPRetencionesDesdeObligYCredLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<RetencionesDesdeObligYCredDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("OPRetencionesDesdeObligYCredLista", json);
			}
		}

		
	}
}
