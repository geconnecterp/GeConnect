using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Mstk;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Mstk.Controllers
{
	public class ConsultaDeStockControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ConsultaDeStockControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<ProductoStkDto> ListaProductoStk
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaProductoStk");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ProductoStkDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaProductoStk", json);
			}
		}

		public MetadataGrid MetadataStockProd
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataStockProd");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt);
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataStockProd", valor);
			}

		}
	}
}
