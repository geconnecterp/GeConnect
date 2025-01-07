using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoRetIbServicio : IServicio<TipoRetIngBrDto>
	{
		List<TipoRetIngBrDto> ObtenerTipoRetIb(string token);
	}
}
