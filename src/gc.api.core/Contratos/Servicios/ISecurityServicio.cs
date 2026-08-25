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

        EstadoSeguridadUsuarioDto ObtenerEstadoSeguridad(string usuId);
        OperacionesSeguridadUsuarioDto ObtenerOperacionesSeguridad(string usuId);
        CambioClaveResultadoDto BlanquearClave(string usuarioObjetivo, string usuarioEjecutor,
            string claveTemporal, string? admId, string? ip, Guid operacionId);
        CambioClaveResultadoDto CambiarClaveForzada(string usuId, string claveNueva,
            string? admId, string? ip, Guid operacionId);
        CambioClaveResultadoDto DesbloquearUsuario(string usuarioObjetivo, string usuarioEjecutor,
            string? admId, string? ip, Guid operacionId);
    }
}
