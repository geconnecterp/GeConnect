using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Seguridad;

namespace gc.api.core.Interfaces.Servicios
{
    public interface ISecurityServicio:IServicio<Usuario>
    {
        Usuario? GetLoginByCredential(UserLogin login, bool esUp = false);

        Task<bool> RegistrerUser(Usuario registracion, bool esUp = false);

        PoliticaClaveDto ObtenerPoliticaClave();

        CambioClaveResultadoDto CambiarClave(string usuId, string claveActual, string claveNueva,
            string? admId, string? ip, Guid operacionId);
    }
}
