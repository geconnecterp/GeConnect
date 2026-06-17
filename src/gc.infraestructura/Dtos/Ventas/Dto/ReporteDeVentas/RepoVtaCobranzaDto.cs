
namespace gc.infraestructura.Dtos
{
	public class RepoVtaCobranzaDto : Dto
	{
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
		public string caja_id { get; set; } = string.Empty;
		public decimal co_cobranza { get; set; }
		public string rb_compte { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public int cm_compte_cuota { get; set; }
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public decimal cc_importe { get; set; }
		public decimal cc_importe_ori { get; set; }
		public DateTime? cc_fecha_vto { get; set; }
		public string dia_movi { get; set; } = string.Empty;
		public string ccb_id { get; set; } = string.Empty;
	}
}
