
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Productos.Pedidos
{
	public class DividePedidoDeClienteRequest : RequestBase
	{
		public string pc_compte { get; set; } = string.Empty;
		public int divide { get; set; }
	}
}
