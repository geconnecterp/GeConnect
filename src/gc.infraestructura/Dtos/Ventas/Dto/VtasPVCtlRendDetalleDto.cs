
namespace gc.infraestructura.Dtos.Ventas
{
	public class VtasPVCtlRendDetalleDto : Dto
	{
		public string ins_id { get; set; } = string.Empty;
		public string ins_desc { get; set; } = string.Empty;
		public string ins_dato1_desc { get; set; } = string.Empty;
		public string ins_dato2_desc { get; set; } = string.Empty;
		public string ins_dato3_desc { get; set; } = string.Empty;
		public string tcf_id { get; set; } = string.Empty;
		public char ins_tiene_vto { get; set; }
		public char ins_detalle { get; set; }
		public string tcf_desc { get; set; } = string.Empty;
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int? caja_nro_cierre { get; set; }
		public int? caja_nro_rend { get; set; }
		public int? rend_item { get; set; }
		public string adm_id { get; set; } = string.Empty;
		public string caja_id { get; set; } = string.Empty;
		public char? rend_tipo { get; set; }
		public DateTime? rend_fecha { get; set; }
		public string rend_dato1_valor { get; set; } = string.Empty;
		public string rend_dato2_valor { get; set; } = string.Empty;
		public string rend_dato3_valor { get; set; } = string.Empty;
		public int? rend_opcion_cuota { get; set; }
		public char? rend_cupon_manual { get; set; }
		public char? rend_ch_dif { get; set; }
		public DateTime? rend_fecha_valor { get; set; }
		public decimal? rend_importe_arq { get; set; }
		public decimal? rend_importe_ok { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public char? rend_estado { get; set; }
		public char? rend_entrega { get; set; }
		public string ent_compte { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string concepto_valor { get; set; } = string.Empty;
		public bool ins_detalle_bool => ins_detalle == 'S';
		public bool pendiente { get; set; } //Viene desde el registro seleccionado de la clase VtasPVCtlRendDto
	}
}
