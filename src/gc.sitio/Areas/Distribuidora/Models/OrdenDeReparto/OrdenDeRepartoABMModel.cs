using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.Dtos.Productos.Pedidos;

namespace gc.sitio.Areas.Distribuidora.Models.OrdenDeReparto
{
	public class OrdenDeRepartoABMModel
	{
		public OrdenDeRepartoDto OrdenDeReparto { get; set; }
		public GridCoreSmart<PedidoEnOrdenDeRepartoDto> ListaPedidosEnOrdenDeReparto { get; set; }
		public GridCoreSmart<PedidoListDto> ListaPedidosPendientes { get; set; }
	}
}
