using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;

namespace gc.sitio.core.Servicios.Contratos.ABM
{
	public interface IABMProveedorServicio : IServicio<ABMProveedorSearchDto>
	{
		Task<(List<ABMProveedorSearchDto>, MetadataGrid)> BuscarProveedores(QueryFilters filters, string token);
	}
}
