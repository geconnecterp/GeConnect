namespace gc.sitio.Areas.ABMs.Models
{
	public class FormaDePagoModel
	{
		public string cta_id { get; set; } = string.Empty;
		public string fp_id { get; set; } = string.Empty;
		public string fp_desc { get; set; } = string.Empty;
		public string fp_lista { get; set; } = string.Empty;
		public int fp_dias { get; set; }
		public string tcb_id { get; set; } = string.Empty;
		public string tcb_desc { get; set; } = string.Empty;
		public string tcb_lista { get; set; } = string.Empty;
		public string cta_bco_cuenta_nro { get; set; } = string.Empty;
		public string cta_bco_cuenta_cbu { get; set; } = string.Empty;
		public string cta_valores_a_nombre { get; set; } = string.Empty;
		public string cta_obs { get; set; } = string.Empty;
		public string fp_deufault { get; set; } = string.Empty;
	}
}
