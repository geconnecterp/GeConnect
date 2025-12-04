using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ProductoStkListaModel
	{
		public GridCoreSmart<ProductoStkDto> GrillaProductoStk { get; set; }
		public int AgrupadoPor { get; set; } = 0;
	}
}
