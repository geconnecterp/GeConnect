using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen;

namespace gc.api.core.Contratos.Servicios
{
    public interface ICuentaServicio : IServicio<Cuenta>
    {
        List<ProveedorListaDto> GetProveedorLista();
    }
}
