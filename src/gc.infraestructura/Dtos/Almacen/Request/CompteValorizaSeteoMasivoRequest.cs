
namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class CompteValorizaSeteoMasivoRequest
	{
		public decimal dto1 { get; set; } = 0.00M;
		public decimal dto2 { get; set; } = 0.00M;
		public decimal dto3 { get; set; } = 0.00M;
		public decimal dto4 { get; set; } = 0.00M;
		public decimal dtodpa { get; set; } = 0.00M;
		public string boni { get; set; } = string.Empty;
		public string[] idsProductos { get; set; } = [];
		public int seccion { get; set; } = 0; //1- Precio // 2- Factura
	}
}
