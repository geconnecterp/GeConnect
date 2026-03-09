using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.Dtos.Productos.Pedidos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Distribuidora.Models.OrdenDeReparto
{
	public class OrdenDeRepartoABMModel
	{
		public char Accion { get; set; } //A o M
		public OrdenDeRepartoDto OrdenDeReparto { get; set; }
		public GridCoreSmart<PedidoEnOrdenDeRepartoDto> ListaPedidosEnOrdenDeReparto { get; set; }
		public GridCoreSmart<PedidoListDto> ListaPedidosPendientes { get; set; }
		public SelectList ListaRepartidores { get; set; }
		public string RepartidorSeleccionado { get; set; } = string.Empty;
	}
}
