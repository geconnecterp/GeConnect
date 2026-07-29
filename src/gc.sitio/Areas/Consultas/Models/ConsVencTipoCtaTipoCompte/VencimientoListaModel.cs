using gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Consultas.Models
{
	public class VencimientoListaModel
	{
		public GridCoreSmart<VencimientoListaDto> GrillaVencimientos { get; set; }
		// Leyenda final ya armada
		public string Leyenda { get; set; } = string.Empty;

		// Opcional: si querés mostrar cada parte por separado
		public string LeyendaFecVenc { get; set; } = string.Empty;
		public string LeyendaFecGen { get; set; } = string.Empty;
		public string LeyendaCli { get; set; } = string.Empty;
		public string LeyendaProv { get; set; } = string.Empty;
		public string LeyendaTipoC { get; set; } = string.Empty;
	}
}
