using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers
{
	public class ConsultaDeRecepcionDeProveedoresControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;
		public ConsultaDeRecepcionDeProveedoresControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}
	}
}
