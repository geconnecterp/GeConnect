
namespace gc.infraestructura.Dtos.OrdenDePago.Request
{
	public class ConfirmarOrdenDePagoDirectaRequest
	{
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string opt_id { get; set; } = string.Empty;
		public string op_desc { get; set; } = string.Empty;
		public string json_encabezado { get; set; } = string.Empty;
		public string json_concepto { get; set; } = string.Empty;
		public string json_otro { get; set; } = string.Empty;
		public string json_v { get; set; } = string.Empty;
	}
}
