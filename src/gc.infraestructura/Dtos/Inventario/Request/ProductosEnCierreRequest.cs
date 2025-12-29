
namespace gc.infraestructura.Dtos.Inventario
{
	public class ProductosEnCierreRequest
	{
		public string inv_nro { get; set; } = string.Empty;
		public char tipo { get; set; }
		public string tipo_id { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
	}
}
