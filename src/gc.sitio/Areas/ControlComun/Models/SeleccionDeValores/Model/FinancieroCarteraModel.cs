namespace gc.sitio.Areas.ControlComun.Models.SeleccionDeValores.Model
{
	public class FinancieroCarteraModel
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string fc_compte { get; set; } = string.Empty;
		public int fc_item { get; set; }
		public string? fc_dato1_valor { get; set; }
		public string? fc_dato2_valor { get; set; }
		public string? fc_dato3_valor { get; set; }
		public DateTime? fc_fecha_valor { get; set; }
		public decimal fc_importe { get; set; } = 0.00M;
		public DateTime? fc_fecha { get; set; }
		public char propio { get; set; }
		public string? cta_id { get; set; }
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string? ins_dato1_desc { get; set; }
		public string? ins_dato2_desc { get; set; }
		public string? ins_dato3_desc { get; set; }
		public string? fc_concepto { get; set; }
		public int? tra_conciliada_nro { get; set; }
		public string? ins_id { get; set; }
		public string tcf_id { get; set; } = string.Empty;
		public string concepto_valor { get; set; } = string.Empty;
		public string ctaf_denominacion { get; set; } = string.Empty;
		public string ban_razon_social { get; set; } = string.Empty;
	}
}
