
namespace gc.infraestructura.Dtos.Consultas.ReporteFinanciero
{
	public class FlujoDeIngresoDto : Dto
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string ctaf_desc { get; set; } = string.Empty;
		public decimal ingreso { get; set; } = 0.00M;
		public decimal revision { get; set; } = 0.00M;
		public decimal cartera { get; set; } = 0.00M;
		public decimal alcobro { get; set; } = 0.00M;
		public decimal acreditado { get; set; } = 0.00M;
		public decimal ajustes { get; set; } = 0.00M;
		private string _medio_de_pago;

		public string medio_de_pago
		{
			get { return $"({ctaf_id}) {ctaf_desc}"; }
			set { _medio_de_pago = value; }
		}

	}
}
