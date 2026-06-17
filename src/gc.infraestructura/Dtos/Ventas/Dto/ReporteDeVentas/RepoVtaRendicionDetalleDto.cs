
namespace gc.infraestructura.Dtos
{
	public class RepoVtaRendicionDetalleDto : Dto
	{
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
		public int caja_nro_rend { get; set; }
		public int rend_item { get; set; }
		public string rend_tipo { get; set; } = string.Empty;
		public DateTime rend_fecha { get; set; }
		public string ins_id { get; set; } = string.Empty;
		public string rend_dato1_valor { get; set; } = string.Empty;
		public string rend_dato2_valor { get; set; } = string.Empty;
		public string rend_dato3_valor { get; set; } = string.Empty;
		public DateTime? rend_fecha_valor { get; set; }
		public decimal rend_importe_arq { get; set; }
		public decimal rend_importe_ok { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public char? rend_estado { get; set; }
		public char? rend_ch_dif { get; set; }
		public string ins_desc { get; set; } = string.Empty;
		public string concepto_valor { get; set; } = string.Empty;
	}
}
