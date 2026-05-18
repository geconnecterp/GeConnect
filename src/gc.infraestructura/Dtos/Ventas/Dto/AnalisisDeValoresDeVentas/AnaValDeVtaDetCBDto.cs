
namespace gc.infraestructura.Dtos.Ventas
{
	public class AnaValDeVtaDetCBDto : Dto
	{
		public string caja_nro_proceso { get; set; } = string.Empty;
		public DateTime caja_habilitacion { get; set; }
		public string rb_compte { get; set; } = string.Empty;
		public string ins_id { get; set; } = string.Empty;
		public string rb_dato1_valor { get; set; } = string.Empty;
		public string rb_dato2_valor { get; set; } = string.Empty;
		public string rb_dato3_valor { get; set; } = string.Empty;
		public DateTime rb_fecha_valor { get; set; }
		public decimal rb_importe { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public int rb_opcion_cuota { get; set; }
		public string tcf_id { get; set; } = string.Empty;
		public string ins_desc { get; set; } = string.Empty;
		public decimal cashback { get; set; }
		public string adm_nombre { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
	}
}
