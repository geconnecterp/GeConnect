
namespace gc.infraestructura.Dtos.Financieros
{
	public class AnticipoDto : Dto
	{
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public int cuotas { get; set; } = 0;
		public decimal importe { get; set; } = 0.00M;
		public decimal intereses { get; set; } = 0.00M;
		public decimal valor_cuota { get; set; } = 0.00M;
		public decimal valor_total { get; set; } = 0.00M;
		public decimal tope { get; set; } = 0.00M;
	}
}
