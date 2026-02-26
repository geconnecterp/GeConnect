using gc.infraestructura.Dtos.Productos.Pedidos;

namespace gc.api.core.Contratos.Servicios
{
	public interface IApiPedidoServicio
	{
		List<PedidoListDto> ObtenerListaPedidos(PedidoRequest req);
	}
}
