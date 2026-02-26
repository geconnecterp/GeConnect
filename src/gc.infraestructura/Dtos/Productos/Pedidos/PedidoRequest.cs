
namespace gc.infraestructura.Dtos.Productos.Pedidos
{
	public class PedidoRequest
	{
		public int Registros { get; set; }
		public int Pagina { get; set; }

		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
		public string? cli_list { get; set; }
		public string? pce_list { get; set; }
		public string? ve_list { get; set; }
		public string? rp_list { get; set; }
	}
}
