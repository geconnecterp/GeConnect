using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Tipos;

namespace gc.api.core.Contratos.Servicios.Tipos
{
	public interface IPedidoInternoEstadoServicio : IServicio<PedidoInternoEstado>
	{
		List<PedidoInternoEstadoDto> GetPedidoInternoEstados();
	}
}
