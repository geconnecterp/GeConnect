
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class FinancieroLiqEmpleadoConfirmarRequest
	{
		public string periodo { get; set; } = string.Empty;
		public string mes { get; set; } = string.Empty;
		public string concepto { get; set; } = string.Empty;
		public bool actualiza_tope { get; set; }
		public decimal porc_tope { get; set; } = 0.00M;
		public string json_tope { get; set; } = string.Empty;
		public string json_detalle { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
	}
}
