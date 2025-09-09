
namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroBcoLibroResumenRequest
	{
		public string ctaf_id { get; set; } = string.Empty;
		public DateTime hasta { get; set; }
	}
}
