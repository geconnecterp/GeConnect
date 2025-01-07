using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoGastoServicio : IServicio<TipoGastoDto>
	{
		List<TipoGastoDto> ObtenerTipoGastos(string token);
	}
}
