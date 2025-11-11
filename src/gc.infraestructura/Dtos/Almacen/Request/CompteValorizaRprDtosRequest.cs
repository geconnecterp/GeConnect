
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class CompteValorizaRprDtosRequest : RequestBase
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
		public bool guarda { get; set; } = false;
		public bool confirma { get; set; } = false;
		public bool dp { get; set; } = false;
		public bool dc { get; set; } = false;
	}
}
