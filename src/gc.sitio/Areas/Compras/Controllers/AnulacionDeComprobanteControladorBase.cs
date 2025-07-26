using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.AnulacionDeComprobante;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers
{
	public class AnulacionDeComprobanteControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		//private readonly ILogger _logger;
		public AnulacionDeComprobanteControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			//_logger = logger;
		}

		public List<ComprobanteParaAnularDto> ListaComprobanteParaAnular
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaComprobanteParaAnular");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<ComprobanteParaAnularDto>();
				}
				return JsonConvert.DeserializeObject<List<ComprobanteParaAnularDto>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaComprobanteParaAnular", valor);
			}

		}

		public List<NotaACuentaDto> ListaNotaACuenta
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("ListaNotaACuenta");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return new List<NotaACuentaDto>();
				}
				return JsonConvert.DeserializeObject<List<NotaACuentaDto>>(txt) ?? [];
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaNotaACuenta", valor);
			}
		}
	}
}
