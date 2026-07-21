using gc.infraestructura.Dtos.Administracion;

namespace gc.auth.remoto.Services;

public interface IAuthenticationApiClient
{
    Task<IReadOnlyList<AdministracionLoginDto>> GetAdministrationsAsync(CancellationToken cancellationToken);
    Task<string> AuthenticateAsync(string userName, string password, string administrationId,
        string? clientIp, CancellationToken cancellationToken);
}
