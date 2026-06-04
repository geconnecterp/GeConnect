using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Ventas.Controllers
{
	public class VentaSorteoCargaControladorBase : ControladorBase
	{
		private readonly AppSettings _setting;

		public VentaSorteoCargaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
		}
		internal RespuestaGenerica<EntidadBase> CrearRespuestaError(string mensaje)
		{
			return new RespuestaGenerica<EntidadBase>
			{
				Mensaje = mensaje,
				Ok = false,
				EsWarn = false,
				EsError = true
			};
		}
		internal RespuestaGenerica<EntidadBase> CrearRespuestaWarning(string mensaje)
		{
			return new RespuestaGenerica<EntidadBase>
			{
				Mensaje = mensaje,
				Ok = false,
				EsWarn = true,
				EsError = false
			};
		}
	}
}
