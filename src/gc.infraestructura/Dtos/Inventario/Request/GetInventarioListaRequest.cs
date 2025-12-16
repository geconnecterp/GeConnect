
namespace gc.infraestructura.Dtos.Inventario
{
	public class GetInventarioListaRequest
	{
		public DateTime desde { get; set; }
		public DateTime hasta { get; set; }
		public string adm_id { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string inve_id { get; set; } = "%";
	}
}
