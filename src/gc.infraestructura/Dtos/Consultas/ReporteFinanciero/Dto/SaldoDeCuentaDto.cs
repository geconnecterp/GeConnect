
namespace gc.infraestructura.Dtos.Consultas.ReporteFinanciero
{
	public class SaldoDeCuentaDto : Dto
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string tcf_id { get; set; } = string.Empty;
		public string ins_id { get; set; } = string.Empty;
		public string mon_codigo { get; set; } = string.Empty;
		public string ctaf_denominacion { get; set; } = string.Empty;
		public string ctaf_estado { get; set; } = string.Empty;
		public string ccb_id { get; set; } = string.Empty;
		public string ctag_id { get; set; } = string.Empty;
		public string ctaf_obs { get; set; } = string.Empty;
		public DateTime? ctaf_actu_fecha { get; set; }
		public char ctaf_actu { get; set; }
		public decimal cf_saldo { get; set; } = 0.00M;
		public string cta_id { get; set; } = string.Empty;
		public string ins_desc { get; set; } = string.Empty;
		public string tcf_desc { get; set; } = string.Empty;
	}
}
