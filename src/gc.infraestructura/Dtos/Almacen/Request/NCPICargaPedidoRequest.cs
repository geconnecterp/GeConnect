
namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class NCPICargaPedidoRequest
	{
        public string tipo { get; set; } = string.Empty;
        public string admId { get; set; } = string.Empty;
		public string usuId { get; set; } = string.Empty;
		public string pId { get; set; } = string.Empty;
		public string tipoCarga { get; set; } = string.Empty;
		public int bultos { get; set; } = 0;
    }
}
