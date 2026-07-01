
namespace gc.infraestructura.Dtos
{
	public class SaldoResumenDto : Dto
	{
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string ve_id { get; set; } = string.Empty;
		public string ve_nombre { get; set; } = string.Empty;
		public decimal saldo_semana_ant3 { get; set; }
		public decimal saldo_semana_ant2 { get; set; }
		public decimal saldo_semana_ant1 { get; set; }
		public decimal saldo_avecer { get; set; }
		public string hoy_m13 { get; set; } = string.Empty;
		public string hoy_m7 { get; set; } = string.Empty;
		public string hoy_m6 { get; set; } = string.Empty;
		public string hoy_1 { get; set; } = string.Empty;
		public string hoy { get; set; } = string.Empty;
	}
}
