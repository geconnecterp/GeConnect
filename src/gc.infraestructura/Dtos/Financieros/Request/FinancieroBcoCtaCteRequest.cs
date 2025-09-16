
namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroBcoCtaCteRequest
	{
		public DateTime FechaDesde { get; set; }
		public DateTime FechaHasta { get; set; }
		public string ctaf_id { get; set; } = string.Empty;
		public string tipo_filtro { get; set; }
		public string ct_tipo { get; set; } = "%";
	}
}
