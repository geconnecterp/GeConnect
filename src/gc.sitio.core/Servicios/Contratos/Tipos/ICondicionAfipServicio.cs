using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ICondicionAfipServicio : IServicio<CondicionAfipDto>
	{
		List<CondicionAfipDto> GetCondicionesAfipLista(string token);
	}
}
