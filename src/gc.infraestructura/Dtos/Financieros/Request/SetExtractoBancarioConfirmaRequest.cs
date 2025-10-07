
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class SetExtractoBancarioConfirmaRequest
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string json_extracto { get; set; } = string.Empty;
		public string json_eliminados { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
	}
}
