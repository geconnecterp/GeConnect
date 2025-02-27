namespace gc.sitio.Areas.ABMs.Models.CuentaDirecta
{
	public class CuentaDirectaAbmValidationModel
	{
		public string ctag_id { get; set; } = string.Empty;
		public string ctag_denominacion { get; set; } = string.Empty;
		public string tcg_id { get; set; } = string.Empty;
		public string tcg_desc { get; set; } = string.Empty;
		public bool ctag_ingreso { get; set; }
		public string ctag_valores_anombre { get; set; } = string.Empty;
		public char ctag_activo { get; set; }
		public string ccb_id { get; set; } = string.Empty;
		public string ccb_desc { get; set; } = string.Empty;
	}
}
