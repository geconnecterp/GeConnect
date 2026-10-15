using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;

namespace gc.sitio.Areas.Mstk.Models.ConsultaPlanillaDeElabFraccion
{
	public class PlanillaModel
	{
		public GridCoreSmart<PlanillaElabYFraccionListaDto> Grilla { get; set; }
		public string Leyenda { get; set; } = string.Empty;
	}
}
