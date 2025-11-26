using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers
{
	public class NDeCYPIControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public NDeCYPIControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<ProductoNCPIDto> ListaProductoNCPI
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaProductoNCPI");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<ProductoNCPIDto>();
				}
				return JsonConvert.DeserializeObject<List<ProductoNCPIDto>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaProductoNCPI", valor);
			}

		}
	}
}
