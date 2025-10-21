using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Financieros;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	public class ProyeccionDeGastosControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ProyeccionDeGastosControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}

		public List<GastoProyListaDto> ListaProyeccionDeGasto
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaProyeccionDeGasto");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<GastoProyListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaProyeccionDeGasto", json);
			}
		}
	}
}
