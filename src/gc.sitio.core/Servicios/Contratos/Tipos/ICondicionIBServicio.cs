using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ICondicionIBServicio : IServicio<CondicionIBDto>
	{
		List<CondicionIBDto> GetCondicionIBLista(string token);
	}
}
