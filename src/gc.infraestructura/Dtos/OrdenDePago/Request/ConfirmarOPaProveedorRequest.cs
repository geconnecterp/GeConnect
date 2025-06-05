
namespace gc.infraestructura.Dtos.OrdenDePago.Request
{
	public class ConfirmarOPaProveedorRequest
	{
		public string cta_id { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string opt_id { get; set; } = string.Empty;
		public string op_desc { get; set; } = string.Empty;
		public string json_d { get; set; } = string.Empty;
		public string json_h { get; set; } = string.Empty;
		public string json_r { get; set; } = string.Empty;
		public string json_v { get; set; } = string.Empty;
	}
}
