
namespace gc.infraestructura.Dtos
{
	public class RepoVtaCambioValoresDto : Dto
	{
		public string rb_compte { get; set; } = string.Empty;
		public int rb_nro_valor { get; set; }
		public string ins_id { get; set; } = string.Empty;
		public string rb_dato1_valor { get; set; } = string.Empty;
		public string rb_dato2_valor { get; set; } = string.Empty;
		public string rb_dato3_valor { get; set; } = string.Empty;
		public DateTime? rb_fecha_valor { get; set; }
		public decimal rb_importe { get; set; }
		public char rb_estado { get; set; }
		public decimal rb_rec { get; set; }
		public decimal rb_aux { get; set; }
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
		public int caja_nro_operacion { get; set; }
		public string co_tipo { get; set; } = string.Empty;
		public DateTime co_fecha { get; set; }
		public string ins_desc { get; set; } = string.Empty;
	}
}
