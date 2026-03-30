using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.Dtos.Productos.Pedidos;

namespace gc.sitio.Areas.Distribuidora.Models.OrdenDeReparto
{
	public class PedidoDeClienteEnEdicionModel
	{
		public OrdenDeRepartoDto OrdenDeReparto { get; set; }
		public GridCoreSmart<PedidoProductoDto> ListaProductosDelPedido { get; set; }
		public PedidoDto DatosDelPedido { get; set; }
	}
}
