using gc.api.core.Entidades.Tipos;
using gc.infraestructura.Dtos;

namespace gc.api.core.Contratos.Servicios.Tipos
{
	public interface IInventarioEstadoServicio : IServicio<InventarioEstado>
	{
		List<InventarioEstadoDto> GetInventarioEstadoLista();
	}
}
