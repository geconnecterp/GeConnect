using RemoteAuthorizations.Application.Responses;

namespace gc.caja.core.Autorizaciones;

public interface IAutorizacionRemotaServicio
{
    Task<Guid> CrearAsync(
        CrearAutorizacionRemotaSolicitud solicitud,
        string idempotencyKey,
        string token,
        CancellationToken cancellationToken = default);

    Task<SolicitudAutorizacionRespuesta> ObtenerResolucionAsync(
        Guid idSolicitud,
        string token,
        CancellationToken cancellationToken = default);
}
