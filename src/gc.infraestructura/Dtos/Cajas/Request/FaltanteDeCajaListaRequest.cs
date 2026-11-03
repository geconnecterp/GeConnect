
namespace gc.infraestructura.Dtos.Cajas
{
	public class FaltanteDeCajaListaRequest
	{
		public string adm_list { get; set; } = string.Empty;
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
	}
}
