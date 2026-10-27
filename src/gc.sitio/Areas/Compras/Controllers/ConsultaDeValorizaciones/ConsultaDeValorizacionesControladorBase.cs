using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Compras.Controllers.ConsultaDeValorizaciones
{
	public class ConsultaDeValorizacionesControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ConsultaDeValorizacionesControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}
	}
}
