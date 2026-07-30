using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ProductoStkListaModel
	{
		public GridCoreSmart<ProductoStkDto> GrillaProductoStk { get; set; }
		public int AgrupadoPor { get; set; } = 0;
		public string Leyenda { get; set; }
		// Opcional: si querés mostrar cada parte por separado
		public string LeyendaSuc { get; set; } = string.Empty;
		public string LeyendaDep { get; set; } = string.Empty;
		public string LeyendaProv { get; set; } = string.Empty;
		public string LeyendaRub { get; set; } = string.Empty;
		public string LeyendaFam { get; set; } = string.Empty;
		public string LeyendaStock { get; set; } = string.Empty;
		public string LeyendaEstado { get; set; } = string.Empty;
		public string LeyendaCosto { get; set; } = string.Empty;
	}
}
