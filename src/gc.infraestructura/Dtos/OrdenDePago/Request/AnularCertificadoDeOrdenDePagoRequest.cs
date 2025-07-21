
namespace gc.infraestructura.Dtos.OrdenDePago.Request
{
	public class AnularCertificadoDeOrdenDePagoRequest
	{
		public string op_compte { get; set; } = string.Empty;
		public string imp_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
	}
}
