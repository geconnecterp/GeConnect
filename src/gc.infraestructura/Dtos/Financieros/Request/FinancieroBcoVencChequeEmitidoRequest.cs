
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class FinancieroBcoVencChequeEmitidoRequest
	{
		public string ctaf_id { get; set; } = string.Empty;
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
	}
}
