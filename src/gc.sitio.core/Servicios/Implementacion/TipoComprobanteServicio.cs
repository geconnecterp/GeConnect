using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.sitio.core.Servicios.Implementacion
{
	public class TipoComprobanteServicio : Servicio<TipoComprobanteDto>, ITipoComprobanteServicio
	{
		private const string RutaAPI = "/api/tipocomprobante";
		private const string ComprobantePorCuentaBuscar = "/GetTipoComprobanteListaPorCuenta";
		private readonly AppSettings _appSettings;

		public TipoComprobanteServicio(IOptions<AppSettings> options, ILogger<AdministracionServicio> logger) : base(options, logger)
		{
			_appSettings = options.Value;
		}

		//Generar metodo para buscar los comprobantes
		//ACAESTAELMETODO
	}
}
