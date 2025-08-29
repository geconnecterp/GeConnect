
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class FinancieroChequeDepositadoRequest
	{
		public string ctaf_id { get; set; } = string.Empty;
		public DateTime fechaDesde { get; set; }
		public DateTime fechaHasta { get; set; }
	}
}
