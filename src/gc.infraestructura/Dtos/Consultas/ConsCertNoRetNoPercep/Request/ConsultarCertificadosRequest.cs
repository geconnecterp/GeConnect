
using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep
{
	public class ConsultarCertificadosRequest : BaseFilters
	{
		public string imp_id { get; set; } = string.Empty;
		public string imp_id_texto { get; set; } = string.Empty;
		public bool ret { get; set; }
		public bool per { get; set; }
		public bool no_vencido { get; set; }
		public bool vencido { get; set; }
		public int? Registros { get; set; }
		public int? Pagina { get; set; }
	}
}
