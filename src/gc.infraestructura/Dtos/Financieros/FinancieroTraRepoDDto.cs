
namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroTraRepoDDto : Dto
	{
		public string tra_compte { get; set; } = string.Empty;
		public string ttra_id { get; set; } = string.Empty;
		public string ttra_desc { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public DateTime tra_fecha { get; set; }
		public DateTime tra_fecha_movi { get; set; }
		public string tra_concepto { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public char tra_anulada { get; set; }
		public DateTime? tra_anulada_fecha { get; set; }
		public int tra_item { get; set; }
		public string ctaf_id { get; set; } = string.Empty;
		public string ctaf_denominacion { get; set; } = string.Empty;
		public string fc_dia_movi { get; set; } = string.Empty;
		public string fc_compte { get; set; } = string.Empty;
		public int? fc_item { get; set; }
		public string tco_id { get; set; } = string.Empty;
		public string fc_dato1_valor { get; set; } = string.Empty;
		public string fc_dato2_valor { get; set; } = string.Empty;
		public string fc_dato3_valor { get; set; } = string.Empty;
		public DateTime? fc_fecha_valor { get; set; }
		public decimal fc_importe { get; set; } = 0.00M;
		public string fc_cta_id { get; set; } = string.Empty;
		public string fc_concepto { get; set; } = string.Empty;
		public string ins_id { get; set; } = string.Empty;
		public string ins_dato1_desc { get; set; } = string.Empty;
		public string ins_dato2_desc { get; set; } = string.Empty;
		public string ins_dato3_desc { get; set; } = string.Empty;
		public int grupo { get; set; }
		public string concepto { get; set; } = string.Empty;
	}
}
