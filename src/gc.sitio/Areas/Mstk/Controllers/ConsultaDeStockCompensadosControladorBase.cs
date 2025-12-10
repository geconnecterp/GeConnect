using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Mstk;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Mstk.Controllers
{
	public class ConsultaDeStockCompensadosControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ConsultaDeStockCompensadosControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<ProductoStkCompensadoDto> ListaProductoStkCompensados
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaProductoStkCompensados");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<ProductoStkCompensadoDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaProductoStkCompensados", json);
			}
		}

		public MetadataGrid MetadataStockProdCompensados
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("MetadataStockProdCompensados");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new MetadataGrid();
				}
				return JsonConvert.DeserializeObject<MetadataGrid>(txt);
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("MetadataStockProdCompensados", valor);
			}

		}
	}
}
