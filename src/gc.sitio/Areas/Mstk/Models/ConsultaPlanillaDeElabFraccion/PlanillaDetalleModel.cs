using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;

namespace gc.sitio.Areas.Mstk.Models.ConsultaPlanillaDeElabFraccion
{
	public class PlanillaDetalleModel
	{
		public GridCoreSmart<PlanillaElabYFraccionDto> Grilla { get; set; }
		public string Leyenda { get; set; }
	}
}
