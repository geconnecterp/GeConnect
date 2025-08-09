
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class ConfirmarTransferenciaRequest
	{
		public string ttra_id { get; set; } = string.Empty;
		public string tra_concepto { get; set; } = string.Empty;
		public DateTime tra_fecha { get; set; }
		public string adm_id { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string json_o { get; set; } = string.Empty;
		public string json_d { get; set; } = string.Empty;
		public string json_encabezado { get; set; } = string.Empty;
		public string json_concepto { get; set; } = string.Empty;
		public string json_otro { get; set; } = string.Empty;
	}
}
