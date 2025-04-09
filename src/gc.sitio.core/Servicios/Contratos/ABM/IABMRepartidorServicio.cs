using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos.ABM
{
    public interface IABMRepartidorServicio : IServicio<ABMRepartidorDto>
    {
        Task<(List<ABMRepartidorDto>, MetadataGrid)> ObtenerRepartidores(QueryFilters filters, string token);
        Task<RespuestaGenerica<ABMRepartidorDatoDto>> ObtenerRepartidorPorId(string rp_id, string token);
    }
}
