
namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class CompteCargaConfirmaRequest
	{
		public string cta_id { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string json_encabezado { get; set; } = string.Empty;
		public string json_concepto { get; set; } = string.Empty;
		public string json_otro { get; set; } = string.Empty;
		public string json_relacion { get; set; } = string.Empty;
	}
}
