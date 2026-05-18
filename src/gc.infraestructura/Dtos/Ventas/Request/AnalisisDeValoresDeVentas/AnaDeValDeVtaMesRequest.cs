
namespace gc.infraestructura.Dtos.Ventas.Request
{
	public class AnaDeValDeVtaMesRequest
	{
		public string adm_list { get; set; } = string.Empty;
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
	}
}
