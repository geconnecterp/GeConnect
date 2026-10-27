
namespace gc.infraestructura.Dtos.Almacen
{
	public class ObtenerComptesDeValorizacionRequest
	{
		public string cta_id { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
	}
}
