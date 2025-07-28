using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Compras.Controllers
{
	public class RelacionarComprobanteSinRPControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public RelacionarComprobanteSinRPControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
		}
	}
}
