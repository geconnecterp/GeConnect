
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class FinancieroConciliacionExtractoConfirmarRequest : RequestBase
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string json_e { get; set; } = string.Empty;
		public string json_s { get; set; } = string.Empty;
	}
}
