
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class CargaAnticipoEmpleadoRequest
	{
		public string ant_id { get; set; } = string.Empty;
		public string an_concepto { get; set; } = string.Empty;
		public decimal interes { get; set; } = 0.00M;
		public string cta_id { get; set; } = string.Empty;
		public string json_anticipos { get; set; } = string.Empty;
	}
}
