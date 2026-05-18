using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Consultas.Controllers
{
	public class AnalisisDeValoresDeVentasControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;

		public AnalisisDeValoresDeVentasControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;

		}
	}
}
