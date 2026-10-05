
using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.RemitoExterno
{
	public class RemitoExternoListaRequest : QueryFilters
	{
		public DateTime FechaDesde { get; set; }
		public DateTime FechaHasta { get; set; }
	}
}
