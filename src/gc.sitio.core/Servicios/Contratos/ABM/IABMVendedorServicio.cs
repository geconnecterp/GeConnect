using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos.ABM
{
    public interface IABMVendedorServicio : IServicio<ABMVendedorDto>
    {
        Task<(List<ABMVendedorDto>, MetadataGrid)> ObtenerVendedores(QueryFilters filters, string token);
        Task<RespuestaGenerica<ABMVendedorDatoDto>> ObtenerVendedorPorId(string ve_id, string token);
    }
}
