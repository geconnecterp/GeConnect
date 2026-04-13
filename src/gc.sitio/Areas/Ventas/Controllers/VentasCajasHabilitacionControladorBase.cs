using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Ventas.Controllers
{
	public class VentasCajasHabilitacionControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public VentasCajasHabilitacionControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}
	}
}
