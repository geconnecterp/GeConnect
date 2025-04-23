
namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class CompteValorizaRprDtosRequest
	{
		public string cta_id { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
	}

	public class CompteValorizaRequest : CompteValorizaRprDtosRequest
	{
		public string json_dtos { get; set; } = string.Empty;
		public string json_detalle { get; set; } = string.Empty;
	}
}
