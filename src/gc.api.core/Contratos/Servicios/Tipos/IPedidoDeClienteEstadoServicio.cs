using gc.api.core.Entidades;
using gc.infraestructura.Dtos;

namespace gc.api.core.Contratos.Servicios.Tipos
{
	public interface IPedidoDeClienteEstadoServicio : IServicio<PedidoDeClienteEstado>
	{
		List<PedidoDeClienteEstadoDto> GetPedidoDeClienteEstados();
	}
}
