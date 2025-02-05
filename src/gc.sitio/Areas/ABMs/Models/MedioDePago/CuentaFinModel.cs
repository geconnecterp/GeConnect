namespace gc.sitio.Areas.ABMs.Models
{
	public class CuentaFinModel
	{
		public string Ctaf_Id { get; set; } = string.Empty;
		public string Ctaf_Denominacion { get; set; } = string.Empty;
		public string Ctaf_Lista { get; set; } = string.Empty;
		public string Ctaf_Activo { get; set; } = string.Empty;
		public string Ctaf_Estado { get; set; } = string.Empty;
		public string Ctaf_Estado_Des { get; set; } = string.Empty;
		public decimal? Ctaf_Saldo { get; set; } = 0.000M;
		public string Adm_Id { get; set; } = string.Empty;
		public string Tcf_Id { get; set; } = string.Empty;
		public string Tcf_Desc { get; set; } = string.Empty;
		public string? Ins_Id { get; set; }
		public string? Ins_Desc { get; set; }
		public string Ccb_Id { get; set; } = string.Empty;
		public string? Ccb_Id_Diferido { get; set; }
		public string? Ctag_Id { get; set; }
		public string? Mon_Codigo { get; set; }
		public string? Cta_Id { get; set; }
	}
}
