using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ProductoStkCompensadoListaModel
	{
		public GridCoreSmart<ProductoStkCompensadoDto> GrillaProductoStkComp { get; set; }
		public string Leyenda { get; set; }
		// Opcional: si querés mostrar cada parte por separado
		public string LeyendaProv { get; set; } = string.Empty;
		public string LeyendaRub { get; set; } = string.Empty;
		public string LeyendaEstado { get; set; } = string.Empty;
	}
}
