namespace gc.sitio.Areas.Financieros.Models.CargarExtractoBancario
{
	public class JsonExtractoModel
	{
		public string ctaf_id { get; set; } = string.Empty;
		public DateTime ext_fecha { get; set; }
		public string extr_id { get; set; } = string.Empty;
		public string extr_desc { get; set; } = string.Empty;
		public decimal ext_debe { get; set; } = 0.00M;
		public decimal ext_haber { get; set; } = 0.00M;
		public decimal ext_saldo { get; set; } = 0.00M;
		public string ct_tipo { get; set; } = string.Empty;
		public string ct_modo { get; set; } = string.Empty;
		public string abm { get; set; } = string.Empty;
	}
}
