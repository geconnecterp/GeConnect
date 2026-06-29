
namespace gc.infraestructura.Dtos
{
	public class MovimientoListaDto : Dto
	{
		public string origen { get; set; } = string.Empty;
		public string op_compte { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public DateTime op_fecha { get; set; }
		public DateTime? cc_fecha_vto { get; set; }
		public string ctag_motivo { get; set; } = string.Empty;
		public string ctag_id { get; set; } = string.Empty;
		public string ctag_denominacion { get; set; } = string.Empty;
		public decimal cc_importe { get; set; }
		public string cm_nombre { get; set; } = string.Empty;
		public string cm_cuit { get; set; } = string.Empty;
		public int signo { get; set; }
	}
}
