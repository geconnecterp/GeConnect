
namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class GetChequeModificarListaRequest
	{
		public string ctaf_id { get; set; } = string.Empty;
		public int che_emision { get; set; }
		public string che_nro { get; set; } = string.Empty;
		public DateTime che_fecha { get; set; }
		public string che_anombre { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
	}
}
