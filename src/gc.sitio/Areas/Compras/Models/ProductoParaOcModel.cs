using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models
{
	public class ProductoParaOcModel
	{
		public GridCoreSmart<ProductoParaOcDto> ListaOC { get; set; }
		public string Total_Costo { get; set; } = string.Empty;
		public string Total_Pallet { get; set; } = string.Empty;
		public decimal Dto1 { get; set; } = 0.0M;
		public decimal Dto2 { get; set; } = 0.0M;
		public decimal Dto3 { get; set; } = 0.0M;
		public decimal Dto4 { get; set; } = 0.0M;
		public decimal Dpa { get; set; } = 0.0M;
		public decimal Flete { get; set; } = 0.0M;
	}
}
