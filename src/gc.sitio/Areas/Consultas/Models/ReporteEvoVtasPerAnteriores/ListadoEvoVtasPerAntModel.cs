using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Consultas.Models
{
	public class ListadoEvoVtasPerAntModel
	{
		public GridCoreSmart<ReporteEvoVtasPerAnterioresDto> GrillaProductoEvo { get; set; }
		public int AgrupadoPor { get; set; } = 0;
		// Leyenda final ya armada
		public string Leyenda { get; set; } = string.Empty;

		// Opcional: si querés mostrar cada parte por separado
		public string LeyendaSuc { get; set; } = string.Empty;
		public string LeyendaProv { get; set; } = string.Empty;
		public string LeyendaRub { get; set; } = string.Empty;
		public string LeyendaFechas { get; set; } = string.Empty;
	}
}
