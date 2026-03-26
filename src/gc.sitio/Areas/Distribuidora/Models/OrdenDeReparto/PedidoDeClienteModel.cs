using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;

namespace gc.sitio.Areas.Distribuidora.Models.OrdenDeReparto
{
	public class PedidoDeClienteModel
	{
		public GridCoreSmart<PedidoEnOrdenDeRepartoDto> ListaPedidosEnOrdenDeReparto { get; set; }
	}
}
