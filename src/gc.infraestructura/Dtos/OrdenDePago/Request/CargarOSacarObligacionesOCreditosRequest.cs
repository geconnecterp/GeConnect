
namespace gc.infraestructura.Dtos.OrdenDePago.Request
{
	public class CargarOSacarObligacionesOCreditosRequest
	{
		public int cuota { get; set; }
		public string dia_movi { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public char accion { get; set; }
		public string origen { get; set; } = string.Empty;
		public decimal cv_importe { get; set; } = 0.00M;
	}
}
