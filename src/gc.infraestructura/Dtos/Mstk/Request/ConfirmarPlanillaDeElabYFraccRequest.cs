
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Mstk.Request
{
	public class ConfirmarPlanillaDeElabYFraccRequest : RequestBase
	{
		public string depoId { get; set; } = string.Empty;
		public string boxId { get; set; } = string.Empty;
		public string nota { get; set; } = string.Empty;
		public string json_elab { get; set; } = string.Empty;
		public string json_mat_prim { get; set; } = string.Empty;
	}
}
