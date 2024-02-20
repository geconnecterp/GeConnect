using gc.api.core.Entidades;
using gc.api.core.Interfaces.Servicios;

namespace gc.api.core.Contratos.Servicios
{
    public interface IServicio<T>:IServicioRd<T>,IServicioWr<T>,IServicioExeSP<T> where T : EntidadBase
    {
    }
}
