
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class FinancieroExtractoDesconciliaRequest
	{
		public string ctaf_id { get; set; } = string.Empty;
		public int conciliado_nro { get; set; }
	}
}
