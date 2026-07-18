using gc.infraestructura.Dtos.SolAuth.Comando;

namespace gc.auth.remoto.Services;

public interface IAuthorizationApiClient
{
    Task<HttpResponseMessage> GetPendingAsync(CancellationToken cancellationToken);
    Task<HttpResponseMessage> GetHistoryAsync(DateTime? from, DateTime? to,
        CancellationToken cancellationToken);
    Task<HttpResponseMessage> LockAsync(Guid requestId, CancellationToken cancellationToken);
    Task<HttpResponseMessage> ResolveAsync(Guid requestId,
        ResolverSolicitudAutorizacionComando command, string idempotencyKey,
        CancellationToken cancellationToken);
}
