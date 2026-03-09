using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Productos.Pedidos
{
	public class ConfirmarPedidoRequest : RequestBase
	{
		public string abm { get; set; } = string.Empty;
		public string pc_compte { get; set; } = string.Empty;
		public string pc_obs { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public char pc_cf { get; set; }
		public string json_prod { get; set; } = string.Empty;
		public DateTime pc_fecha { get; set; }
		public DateTime pc_entrega { get; set; }
	}
}
