using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoInventarioServicio : IServicio<TipoInventarioDto>
	{
		List<TipoInventarioDto> GetTiposEnventario(string token);
	}
}
