using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;

namespace gc.sitio.Areas.Mstk.Models
{
	public class MovDeStockListaModel
	{
		public GridCoreSmart<MovStkProductoDto> GrillaProductoMovStk { get; set; }
		public int AgrupadoPor { get; set; } = 0;
		public string Leyenda { get; set; } = "Filtros aplicados";
		public string LeyendaTipoMov { get; set; } = string.Empty;
		public string LeyendaDep { get; set; } = string.Empty;
		public string LeyendaBox { get; set; } = string.Empty;
		public string LeyendaProv { get; set; } = string.Empty;
		public string FechaDesde { get; set; } = string.Empty;
		public string FechaHasta { get; set; } = string.Empty;
		public string Producto { get; set; } = string.Empty;
	}
}
