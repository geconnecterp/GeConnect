
namespace gc.infraestructura.Dtos.Financieros
{
	public class LiqEmpleadoDetalleDto : Dto
	{
		public string id { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string cta_emp { get; set; } = string.Empty;
		public string cta_emp_legajo { get; set; } = string.Empty;
		public string cta_emp_ctaf { get; set; } = string.Empty;
		public decimal tope { get; set; } = 0.00M;
		public string dia_movi { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public int cm_compte_cuota { get; set; }
		public DateTime cv_fecha_vto { get; set; }
		public decimal cv_importe { get; set; } = 0.00M;
		public decimal cv_importe_imputado { get; set; } = 0.00M;
		public string concepto { get; set; } = string.Empty;
		public string ccb_id { get; set; } = string.Empty;
	}
}
