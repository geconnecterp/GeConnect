using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.infraestructura.Dtos;

namespace gc.api.core.Interfaces.Servicios
{
    public interface ISecurityServicio:IServicio<Usuario>
    {
        Task<Usuario?> GetLoginByCredential(UserLogin login, bool esUp = false);

        Task<bool> RegistrerUser(Usuario registracion, bool esUp = false);
    }
}
