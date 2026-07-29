using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Consultas.Models
{
	public class CertificadoListaModel
	{
		public GridCoreSmart<CertificadoListaDto> GrillaCertificados { get; set; }
		// Leyenda final ya armada
		public string Leyenda { get; set; } = string.Empty;

		// Opcional: si querés mostrar cada parte por separado
		public string LeyendaImp { get; set; } = string.Empty;
		public string LeyendaCertNoRet { get; set; } = string.Empty;
		public string LeyendaCertNoPer { get; set; } = string.Empty;
		public string LeyendaNoVenc { get; set; } = string.Empty;
		public string LeyendaVenc { get; set; } = string.Empty;
	}
}
