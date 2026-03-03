using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Pedidos;

namespace gc.api.core.Contratos.Servicios
{
	public interface IApiPedidoServicio
	{
		List<PedidoListDto> ObtenerListaPedidos(PedidoRequest req);
		List<PedidoDto> ObtenerPedido(string pc_compte);
		List<PedidoProductoDto> ObtenerDetalleDePedido(string pc_compte);
		RespuestaDto ConfirmarPedido(ConfirmarPedidoRequest request);
	}
}
