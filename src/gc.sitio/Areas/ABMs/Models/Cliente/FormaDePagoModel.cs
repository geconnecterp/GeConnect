namespace gc.sitio.Areas.ABMs.Models
{
	public class FormaDePagoModel
	{
		public string Cta_Id { get; set; } = string.Empty;
		public string Fp_Id { get; set; } = string.Empty;
		public string Fp_Desc { get; set; } = string.Empty;
		public string Fp_Lista { get; set; } = string.Empty;
		public int Fp_Dias { get; set; }
		public string Tcb_Id { get; set; } = string.Empty;
		public string Tcb_Desc { get; set; } = string.Empty;
		public string Tcb_Lista { get; set; } = string.Empty;
		public string Cta_Bco_Cuenta_Nro { get; set; } = string.Empty;
		public string Cta_Bco_Cuenta_Cbu { get; set; } = string.Empty;
		public string Cta_Valores_A_Nombre { get; set; } = string.Empty;
		public string Cta_Obs { get; set; } = string.Empty;
		public string Fp_Deufault { get; set; } = string.Empty;
	}
}
