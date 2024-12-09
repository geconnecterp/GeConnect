using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Almacen;

namespace gc.api.core.Contratos.Servicios
{
    public interface IABMProductoServicio : IServicio<Producto>
    {
        List<ProductoListaDto> Buscar(QueryFilters filtro);
    }
}
