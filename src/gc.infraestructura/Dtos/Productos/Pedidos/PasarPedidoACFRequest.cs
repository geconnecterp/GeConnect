using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Productos.Pedidos
{
	public class PasarPedidoACFRequest : RequestBase
	{
		public string pc_compte { get; set; } = string.Empty;
	}
}
