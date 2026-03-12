using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;

namespace gc.sitio.Areas.Distribuidora.Models.OrdenDeReparto
{
	public class OrdenDeRepartoConsolidarModel
	{
		public OrdenDeRepartoDto OrdenDeReparto { get; set; }
		public GridCoreSmart<PedidoEnOrdenDeRepartoDto> ListaPedidosEnOrdenDeReparto { get; set; }
		public GridCoreSmart<AConsolidarPedidoClienteDetalleDto> ListaDetallesAConsolidar { get; set; }
		public GridCoreSmart<AConsolidarConteosDto> ListaConteosDeLaOrdenDeReparto { get; set; }
		public GridCoreSmart<AConsolidarPedidoClienteDetalleDto> ListaDetalleProductoSeleccionado { get; set; }
	}
}
