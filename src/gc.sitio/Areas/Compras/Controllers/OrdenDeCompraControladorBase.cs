using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers
{
    public class OrdenDeCompraControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ILogger _logger;
		public OrdenDeCompraControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
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
				return JsonConvert.DeserializeObject<string>(txt); ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("CtaIdSelected", valor);
			}

		}

		public List<ProductoNCPIDto> ListaProductos
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaProductos");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<ProductoNCPIDto>();
				}
				return JsonConvert.DeserializeObject<List<ProductoNCPIDto>>(txt); ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaProductos", valor);
			}

		}

		public List<ProductoParaOcDto> ListaProductosOC
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaProductosOC");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<ProductoParaOcDto>();
				}
				return JsonConvert.DeserializeObject<List<ProductoParaOcDto>>(txt); ;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaProductosOC", valor);
			}

		}
	}
}
