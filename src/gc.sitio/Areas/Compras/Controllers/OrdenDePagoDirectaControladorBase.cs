using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Compras.Controllers
{
	public class OrdenDePagoDirectaControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ILogger _logger;
		public OrdenDePagoDirectaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_logger = logger;
		}
	}
}
