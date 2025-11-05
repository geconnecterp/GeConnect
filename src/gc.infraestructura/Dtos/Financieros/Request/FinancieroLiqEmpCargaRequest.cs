
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class FinancieroLiqEmpCargaRequest
	{
		public string periodo { get; set; } = string.Empty;
		public string mes { get; set; } = string.Empty;
		public string json_topes { get; set; } = string.Empty;
		public decimal porc_tope { get; set; } = 0.00M;
	}
}
