
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroConciliaDatosRequest : RequestBase
	{
		public string ctaf_id { get; set; } = string.Empty;
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
		public bool concilia { get; set; }
		public bool select_conciliados { get; set; }
	}
}
