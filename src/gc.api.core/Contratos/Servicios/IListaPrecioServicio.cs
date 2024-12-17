using gc.api.core.Entidades;
using gc.infraestructura.Dtos;

namespace gc.api.core.Contratos.Servicios
{
    public interface IListaPrecioServicio : IServicio<ListaPrecio>
    {
        List<ListaPrecioDto> GetListaPrecio();
    }
}
