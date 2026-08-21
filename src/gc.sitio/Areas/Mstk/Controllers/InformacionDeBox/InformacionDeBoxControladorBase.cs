using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers.InformacionDeBox
{
	public class InformacionDeBoxControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public InformacionDeBoxControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}
	}
}
