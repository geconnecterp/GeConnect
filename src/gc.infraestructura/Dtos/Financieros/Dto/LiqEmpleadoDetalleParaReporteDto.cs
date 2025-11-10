
namespace gc.infraestructura.Dtos.Financieros
{
	public class LiqEmpleadoDetalleParaReporteDto : Dto
	{
		public string le_compte { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string tdoc_id { get; set; } = string.Empty;
		public string cta_documento { get; set; } = string.Empty;
		public string cta_emp_legajo { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string cm_compte_cuota { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public DateTime cv_fecha_vto { get; set; }
		public decimal cv_importe { get; set; } = 0.00M;
		public decimal cv_importe_ori { get; set; } = 0.00M;
		public decimal dto { get; set; } = 0.00M;
		public string le_periodo { get; set; } = string.Empty;
		public DateTime le_fecha { get; set; }
		public string le_concepto { get; set; } = string.Empty;
		public char le_anulada { get; set; }
		public string usu_id { get; set; }
		public string usu_apellidoynombre { get; set; } = string.Empty;
		public string concepto { get; set; } = string.Empty;
	}
}
