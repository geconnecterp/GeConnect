namespace gc.sitio.Areas.ABMs.Models
{
	public class BancoAbmValidationModel
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string ban_razon_social { get; set; } = string.Empty;
		public string ban_cuit { get; set; } = string.Empty;
		public char tcb_id { get; set; }
		public string tcb_desc { get; set; } = string.Empty;
		public string? ban_cuenta_nro { get; set; }
		public string? ban_cuenta_cbu { get; set; }
		public string mon_codigo { get; set; } = string.Empty;
		public string mon_desc { get; set; } = string.Empty;
		public int? ban_che_nro { get; set; }
		public int? ban_che_desde { get; set; }
		public int? ban_che_hasta { get; set; }
		public string ccb_id { get; set; } = string.Empty;
		public string ccb_desc { get; set; } = string.Empty;
		public string? ccb_id_diferido { get; set; }
		public string ccb_desc_diferido { get; set; } = string.Empty;
		public string? ctag_id { get; set; }
		public string ctag_denominacion { get; set; } = string.Empty;
	}
}
