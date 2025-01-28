using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;

namespace gc.sitio.core.Servicios.Contratos.ABM
{
    public interface IABMSectorServicio : IServicio<ABMSectorSearchDto>
    {
        Task<(List<ABMSectorSearchDto>, MetadataGrid)> BuscarSectores(QueryFilters filters, string token);
	}
}
