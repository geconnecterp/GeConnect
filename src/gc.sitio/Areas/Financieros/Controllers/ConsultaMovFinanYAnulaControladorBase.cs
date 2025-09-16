using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	public class ConsultaMovFinanYAnulaControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ConsultaMovFinanYAnulaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
		}

		public List<MovimientoFinancieroListaDto> ListaMovimientoFinanciero
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaMovimientoFinanciero");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<MovimientoFinancieroListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaMovimientoFinanciero", json);
			}
		}

		public MetadataGrid MetadataMovimientoFinanciero
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataMovimientoFinanciero");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt);
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataMovimientoFinanciero", valor);
			}

		}
	}
}
