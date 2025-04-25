using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers
{
	public class ValorizacionDeComprobanteControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ILogger _logger;
		public ValorizacionDeComprobanteControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_logger = logger;
		}

		public string CtaIdSelected
		{
			get
			{
				var txt = _context.HttpContext.Session.GetString("CtaIdSelected");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return string.Empty;
				}
				return JsonConvert.DeserializeObject<string>(txt); ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("CtaIdSelected", valor);
			}

		}

		public List<CompteValorizaPendienteListaDto> ComprobantesPendientesDeValorizarLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("ComprobantesPendientesDeValorizarLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<CompteValorizaPendienteListaDto>();
				}
				return JsonConvert.DeserializeObject<List<CompteValorizaPendienteListaDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("ComprobantesPendientesDeValorizarLista", json);
			}
		}

		public List<CompteValorizaDtosListaDto> ComprobantesValorizaDescuentosFinancLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("ComprobantesValorizaDescuentosFinancLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<CompteValorizaDtosListaDto>();
				}
				return JsonConvert.DeserializeObject<List<CompteValorizaDtosListaDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("ComprobantesValorizaDescuentosFinancLista", json);
			}
		}

		public List<CompteValorizaDetalleRprListaDto> ComprobantesValorizaDetalleRprLista
		{
			get
			{
				var json = _context.HttpContext.Session.GetString("ComprobantesValorizaDetalleRprLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return new List<CompteValorizaDetalleRprListaDto>();
				}
				return JsonConvert.DeserializeObject<List<CompteValorizaDetalleRprListaDto>>(json);
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext.Session.SetString("ComprobantesValorizaDetalleRprLista", json);
			}
		}
	}
}
