using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Pedidos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IPedidoServicio
	{
		Task<RespuestaGenerica<PedidoListDto>> BuscarPedidos(QueryFilters filtro, string token);
		Task<RespuestaGenerica<PedidoDto>> ObtenerPedido(string pcCompte, string token);
		Task<RespuestaGenerica<PedidoProductoDto>> ObtenerDetalleDePedido(string pcCompte, string token);
		Task<RespuestaGenerica<RespuestaDto>> ConfirmarPedido(ConfirmarPedidoRequest req, string token);
	}
}
