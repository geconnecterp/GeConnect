using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;

namespace gc.api.core.Contratos.Servicios
{
    public interface IABMProductoServicio : IServicio<Producto>
    {
        List<ABMProductoSearchDto> Buscar(QueryFilters filtro);
    }
}
