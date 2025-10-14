
namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroConciliaDatosRequest
	{
		public string ctaf_id { get; set; } = string.Empty;
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
		public bool concilia { get; set; }
		public bool select_conciliados { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
	}
}
