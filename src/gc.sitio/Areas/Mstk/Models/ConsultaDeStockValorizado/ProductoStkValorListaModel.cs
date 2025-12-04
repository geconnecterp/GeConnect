using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;

namespace gc.sitio.Areas.Mstk.Models.ConsultaDeStockValorizado
{
	public class ProductoStkValorListaModel
	{
		public GridCoreSmart<ProductoStkDto> GrillaProductoStkValorizado { get; set; }
		public int AgrupadoPor { get; set; } = 0;
	}
}
