using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Mstk;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Mstk.Controllers.ConsultaDeMovDeStock
{
	public class ConsultaDeMovDeStockControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ConsultaDeMovDeStockControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<MovStkProductoDto> ListaProductoMovStk
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaProductoMovStk");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<MovStkProductoDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaProductoMovStk", json);
			}
		}

		public MetadataGrid MetadataMovStockProd
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataMovStockProd");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt);
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataMovStockProd", valor);
			}

		}
	}
}
