
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class RegistrarRechazoDeChequeRequest
	{
		public string ctaf_id { get; set; } = string.Empty;
		public int che_emision { get; set; }
		public DateTime fecha_rechazo { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
	}
}
