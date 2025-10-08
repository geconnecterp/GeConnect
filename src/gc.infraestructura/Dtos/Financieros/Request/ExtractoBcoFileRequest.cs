
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class ExtractoBcoFileRequest
	{
		public string ctaf_id { get; set; } = string.Empty;
		public char tipo_file { get; set; }
		public string json_file { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
	}
}
