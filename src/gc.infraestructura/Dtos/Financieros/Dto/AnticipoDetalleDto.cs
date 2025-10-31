
namespace gc.infraestructura.Dtos.Financieros
{
	public class AnticipoDetalleDto : Dto
	{
		public string an_compte { get; set; } = string.Empty;
		public string ant_id { get; set; } = string.Empty;
		public string ant_desc { get; set; } = string.Empty;
		public string an_concepto { get; set; } = string.Empty;
		public char an_prov { get; set; }
		public string cta_id_prov { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public char an_anulada { get; set; }
		public DateTime? an_anulada_fecha { get; set; }
		public int an_item { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public int cm_compte_cuota { get; set; }
		public int cm_compte_cuota_tot { get; set; }
		public string tco_id { get; set; } = string.Empty;
		public DateTime cv_fecha_vto { get; set; }
		public decimal cv_importe { get; set; } = 0.00M;
		public string an_tipo { get; set; } = string.Empty;
		public string an_mes { get; set; } = string.Empty;
		public string an_periodo { get; set; } = string.Empty;
		public DateTime an_fecha { get; set; }
		public string cta_denominacion { get; set; } = string.Empty;
		public string cta_emp_legajo { get; set; } = string.Empty;
		public string cv_concepto { get; set; } = string.Empty;
	}
}
