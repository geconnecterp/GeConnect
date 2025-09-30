
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class RegistrarFechaDeEntregaRequest
	{
		public string ctaf_id { get; set; } = string.Empty;
		public int che_emision { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
	}
}
