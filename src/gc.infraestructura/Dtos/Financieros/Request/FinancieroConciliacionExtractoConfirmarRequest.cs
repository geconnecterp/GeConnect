
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class FinancieroConciliacionExtractoConfirmarRequest
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string json_e { get; set; } = string.Empty;
		public string json_s { get; set; } = string.Empty;
	}
}
