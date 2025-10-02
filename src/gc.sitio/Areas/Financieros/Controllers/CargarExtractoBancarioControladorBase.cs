using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	public class CargarExtractoBancarioControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public CargarExtractoBancarioControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
		}

		public List<FinancieroDesdeSeleccionDeTipoDto> ListaCuentaBancos
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaCuentaBancos");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<FinancieroDesdeSeleccionDeTipoDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaCuentaBancos", json);
			}
		}
	}
}
