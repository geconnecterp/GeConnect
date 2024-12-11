using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IProvinciaServicio : IServicio<ProvinciaDto>
	{
		List<ProvinciaDto> GetProvinciaLista(string token);
	}
}
