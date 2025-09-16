
namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroBcoCtaCteDto : Dto
	{
		public string dia_movi { get; set; } = string.Empty;
		public DateTime cf_fecha { get; set; }
		public string cf_concepto { get; set; } = string.Empty;
		public decimal cf_debe { get; set; }
		public decimal cf_haber { get; set; }
		public decimal cf_saldo { get; set; }
		public string ctaf_id { get; set; } = string.Empty;
		public string cf_compte { get; set; } = string.Empty;
		public int cf_item { get; set; }
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public DateTime? cf_fecha_concilia { get; set; }
		public string ct_tipo { get; set; } = string.Empty;
		public char? cf_conciliado { get; set; }
		public int? cf_conciliado_nro { get; set; }
		public int? che_emision { get; set; }
		public DateTime? fecha_cheque { get; set; }
		private bool _strConciliado;

		public bool strConciliado
		{
			get { return cf_conciliado != null && cf_conciliado == 'S'; }
			set { _strConciliado = value; }
		}
	}
}
