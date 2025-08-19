using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	public class PresentacionDeValoresEnCarteraControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public PresentacionDeValoresEnCarteraControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
		}

		public string CtafIdSelected
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("CtafIdSelected");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return string.Empty;
				}
				return JsonConvert.DeserializeObject<string>(txt) ?? string.Empty;
			}
			set
			{
				var txt = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("CtafIdSelected", txt);
			}
		}

		public List<ValoresDesdeObligYCredDto> OPValoresSeleccionados
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("OPValoresSeleccionados");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ValoresDesdeObligYCredDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("OPValoresSeleccionados", json);
			}
		}

		public List<FinancieroCarteraDto> FinancieroCarteraLista
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("FinancieroCarteraLista");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<FinancieroCarteraDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("FinancieroCarteraLista", json);
			}
		}
	}
}
