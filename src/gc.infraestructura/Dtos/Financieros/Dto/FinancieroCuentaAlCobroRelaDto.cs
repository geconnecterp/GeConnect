
namespace gc.infraestructura.Dtos
{
	public class FinancieroCuentaAlCobroRelaDto : Dto
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string ctaf_denominacion { get; set; } = string.Empty;
		public decimal ctaf_saldo { get; set; } = 0.00M;
		public string ins_id { get; set; } = string.Empty;
		public string mon_codigo { get; set; } = string.Empty;
		public string tcf_id { get; set; } = string.Empty;
	}
}
