using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos.ABM
{
    public interface IABMZonaServicio : IServicio<ABMZonaDto>
    {
        Task<(List<ABMZonaDto>, MetadataGrid)> ObtenerZonas(QueryFilters filters, string token);
        Task<RespuestaGenerica<ZonaDto>> ObtenerZonaPorId(string rp_id, string token);
    }
}