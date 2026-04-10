using gc.infraestructura.Dtos.Tipos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IPedidoInternoEstadoServicio : IServicio<PedidoInternoEstadoDto>
	{
		List<PedidoInternoEstadoDto> GetPedidoInternoEstados(string token);
	}
}
