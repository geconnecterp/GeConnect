using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IRepartidorServicio : IServicio<RepartidorDto>
	{
		List<RepartidorDto> GetRepartidorLista(string token);
	}
}
