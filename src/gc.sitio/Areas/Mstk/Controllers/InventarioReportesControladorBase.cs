using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Inventario;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Mstk.Controllers
{
	public class InventarioReportesControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public InventarioReportesControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}
		public List<InventarioListaDto> ListaInventarioEnReporte
		{
			get
			{
				var json = _context.HttpContext?.Session.GetString("ListaInventarioEnReporte");
				if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
				{
					return [];
				}
				return JsonConvert.DeserializeObject<List<InventarioListaDto>>(json) ?? [];
			}
			set
			{
				var json = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("ListaInventarioEnReporte", json);
			}
		}
	}
}
