
namespace gc.infraestructura.Dtos.Ventas
{
	public class AnaVtaMesRequest
	{
		public string adm_list { get; set; } = string.Empty;
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
	}
}
