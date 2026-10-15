using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Mstk
{
	public class PlanillaElabYFraccionListaRequest : QueryFilters
	{
		public DateTime FechaDesde { get; set; }
		public DateTime FechaHasta { get; set; }
		public bool adm { get; set; }
		public string adm_list { get; set; } = string.Empty;

	}
}
