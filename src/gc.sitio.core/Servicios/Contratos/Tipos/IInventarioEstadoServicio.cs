using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos.Tipos
{
	public interface IInventarioEstadoServicio : IServicio<InventarioEstadoDto>
	{
		List<InventarioEstadoDto> GetInventarioEstadoLista(string token);
	}
}
