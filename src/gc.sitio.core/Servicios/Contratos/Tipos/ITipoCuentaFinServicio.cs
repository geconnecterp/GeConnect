using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoCuentaFinServicio : IServicio<TipoCuentaFinDto>
	{
		List<TipoCuentaFinDto> ObtenerTipoCuentaFin(string token);
		List<TipoCuentaFinDto> GetTipoCuentaFinParaSeleccionDeValores(string app, string token);
	}
}
