using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
namespace gc.api.Core.Interfaces.Servicios
{
    public interface IUsuarioServicio : IServicio<Usuarios>
    {
        Usuarios GetUsuarioByUserName(string userName);
    }
}
