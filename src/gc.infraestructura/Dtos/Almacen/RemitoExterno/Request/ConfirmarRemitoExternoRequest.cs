using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos
{
	public class ConfirmarRemitoExternoRequest : RequestBase
	{
		public char opcion { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string pre_id { get; set; } = string.Empty;
		public string pre_obs { get; set; } = string.Empty;
		public string json { get; set; } = string.Empty;
	}
}
