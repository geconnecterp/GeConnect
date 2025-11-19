using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Consultas.Models
{
	public class CertificadoListaModel
	{
		public GridCoreSmart<CertificadoListaDto> GrillaCertificados { get; set; }
	}
}
