using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.DTOs;

namespace gc.api.core.Interfaces.Servicios
{
    public interface ISecurityServicio:IServicio<Usuario>
    {
        Task<Usuario> GetLoginByCredential(UserLogin login);

        Task<bool> RegistrerUser(RegistroUserDto registro);
    }
}
