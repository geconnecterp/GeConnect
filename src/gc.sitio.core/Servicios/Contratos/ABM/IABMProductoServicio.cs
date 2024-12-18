using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos.ABM
{
    public interface IABMProductoServicio : IServicio<ProductoListaDto>
    {
        Task<(List<ProductoListaDto>,MetadataGrid)> BuscarProducto(QueryFilters filters, string token);
        Task<ProductoDto> BuscarProducto(string p_id, string token);
    }
}
