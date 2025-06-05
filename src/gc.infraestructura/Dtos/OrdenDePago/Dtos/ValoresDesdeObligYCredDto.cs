
namespace gc.infraestructura.Dtos.OrdenDePago.Dtos
{
	public class ValoresDesdeObligYCredDto : Dto
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string ctaf_denominacion { get; set; } = string.Empty;
		public string tcf_id { get; set; } = string.Empty;
		public string tipo { get; set; } = string.Empty;
		public char automatico { get; set; }
		public string op_dato1_valor { get; set; } = string.Empty;
		public string op_dato1_desc { get; set; } = string.Empty;
		public string op_dato2_valor { get; set; } = string.Empty;
		public string op_dato2_desc { get; set; } = string.Empty;
		public string op_dato3_valor { get; set; } = string.Empty;
		public string op_dato3_desc { get; set; } = string.Empty;
		public decimal op_importe { get; set; } = 0.00M;
		public DateTime? op_fecha_valor { get; set; }
		public string fc_compte { get; set; } = string.Empty;
		public int fc_item { get; set; }
		public string fc_dia_movi { get; set; } = string.Empty;
		public string fc_cta_id { get; set; } = string.Empty;
		public string fc_anombre { get; set; } = string.Empty;
		public string concepto_valor { get; set; } = string.Empty;
		public int resultado { get; set; }
		public string resultado_msj { get; set; } = string.Empty;
		public int orden { get; set; } = 0;
	}
}
