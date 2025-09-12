
namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroBcoVencChequeEmitidoListaDto : Dto
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string ctaf_denominacion { get; set; } = string.Empty;
		public int che_emision { get; set; }
		public string che_nro { get; set; } = string.Empty;
		public DateTime che_fecha { get; set; }
		public string che_anombre { get; set; } = string.Empty;
		public decimal che_importe { get; set; }
		public char che_estado { get; set; }
		public string che_estado_desc { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public DateTime che_fecha_emi { get; set; }
		public char che_impreso { get; set; }
		public string che_op_tra { get; set; } = string.Empty;
		public string op_compte { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public DateTime? ent_fecha { get; set; }
		public string ent_usu_id { get; set; } = string.Empty;
		public char che_auto { get; set; }
		public char modificado { get; set; }
		public char dif_print { get; set; }
		public char cf_conciliado { get; set; }
		public decimal diferido { get; set; }
	}
}
