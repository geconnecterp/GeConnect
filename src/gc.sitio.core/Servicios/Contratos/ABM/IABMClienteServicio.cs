using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;

namespace gc.sitio.core.Servicios.Contratos.ABM
{
    public interface IABMClienteServicio : IServicio<ABMClienteSearchDto>
    {
        Task<(List<ABMClienteSearchDto>, MetadataGrid)> BuscarClientes(QueryFilters filters, string token);
    }
}
