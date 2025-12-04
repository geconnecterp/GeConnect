using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Mstk;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Mstk.Controllers
{
	public class ConsultaDeStockValorizadoControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ConsultaDeStockValorizadoControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<ProductoStkDto> ListaProductoStkValor
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaProductoStkValor");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ProductoStkDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaProductoStkValor", json);
			}
		}

		public MetadataGrid MetadataStockValProd
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataStockValProd");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt);
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataStockValProd", valor);
			}

		}
	}
}
