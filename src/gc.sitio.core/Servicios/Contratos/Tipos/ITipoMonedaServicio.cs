using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoMonedaServicio : IServicio<TipoMonedaDto>
	{
		List<TipoMonedaDto> ObtenerTipoMoneda(string token);
	}
}
