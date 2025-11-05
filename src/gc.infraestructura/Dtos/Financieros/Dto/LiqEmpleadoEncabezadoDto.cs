
namespace gc.infraestructura.Dtos.Financieros
{
	public class LiqEmpleadoEncabezadoDto : Dto
	{
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string cta_emp { get; set; } = string.Empty;
		public string cta_emp_legajo { get; set; } = string.Empty;
		public string cta_emp_ctaf { get; set; } = string.Empty;
		public decimal tope { get; set; } = 0.00M;
		public decimal cv_importe_tot { get; set; } = 0.00M;
		public decimal cv_importe_tot_pend { get; set; } = 0.00M;
		public decimal cv_importe_tot_imputado { get; set; } = 0.00M;
		public decimal porc_imputado_sobre_tope { get; set; } = 0.00M;
		public char atraso { get; set; }
		public char tienetope { get; set; }
		private bool _atraso_bool;

		public bool atraso_bool
		{
			get { return atraso == 'S'; }
			set { _atraso_bool = value; }
		}
		private bool _tienetope_bool;

		public bool tienetope_bool
		{
			get { return tienetope == 'S'; }
			set { _tienetope_bool = value; }
		}

	}
}
