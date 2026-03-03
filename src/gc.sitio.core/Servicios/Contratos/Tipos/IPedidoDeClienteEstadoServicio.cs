using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IPedidoDeClienteEstadoServicio : IServicio<PedidoDeClienteEstadoDto>
	{
		List<PedidoDeClienteEstadoDto> GetPedidoDeClienteEstados(string token);
	}
}
