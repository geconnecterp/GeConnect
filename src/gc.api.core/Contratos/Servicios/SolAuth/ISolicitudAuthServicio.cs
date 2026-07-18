using gc.infraestructura.Dtos.SolAuth.Comando;
using RemoteAuthorizations.Application.Responses;

namespace gc.api.core.Contratos.Servicios.SolAuth
{
    public interface ISolicitudAuthServicio
    {
        Task<SolicitudAutorizacionRespuesta> CrearAsync(
         CrearSolicitudAutorizacionComando comando,
         string idempotencyKey,
         string codigoModuloOrigen,
         CancellationToken cancellationToken = default);

        Task<ResolucionAutorizacionRespuesta> ResolverAsync(
            Guid idSolicitud,
            ResolverSolicitudAutorizacionComando comando,
            string idempotencyKey,
            string idUsuarioResolucion,
            CancellationToken cancellationToken = default);

        Task BloquearAsync(
            Guid idSolicitud,
            string idUsuario,
            CancellationToken cancellationToken = default);

        Task<SolicitudAutorizacionRespuesta> ObtenerResolucionAsync(
            Guid idSolicitud,
            string idUsuario,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<SolicitudAutorizacionRespuesta>> ObtenerPendientesAsync(
            string idUsuario,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<SolicitudAutorizacionRespuesta>> ObtenerHistoricoAsync(
            DateTime fechaDesde,
            DateTime fechaHasta,
            int top,
            string idUsuario,
            CancellationToken cancellationToken = default);

        Task ExpirarSolicitudesPendientesAsync(CancellationToken cancellationToken = default);
    }
}
