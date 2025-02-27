
using gc.infraestructura.Core.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class NCPICargarListaDeProductosRequest : QueryFilters
	{
        public string Tipo { get; set; } = string.Empty;
        public string AdmId { get; set; } = string.Empty;
		public string Filtro { get; set; } = string.Empty;
		public string Id { get; set; } = string.Empty;
	}
}
