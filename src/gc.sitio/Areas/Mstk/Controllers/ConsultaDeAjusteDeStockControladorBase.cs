using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Mstk.Controllers
{
	public class ConsultaDeAjusteDeStockControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ConsultaDeAjusteDeStockControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<AjusteDeStockListaDto> ListaAjustes
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaAjustes");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<AjusteDeStockListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaAjustes", json);
			}
		}

		public MetadataGrid MetadataListaAjustes
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataListaAjustes");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt);
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataListaAjustes", valor);
			}

		}
	}
}
