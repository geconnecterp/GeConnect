using gc.caja.core.Autorizaciones;
using RemoteAuthorizations.Application.Responses;

namespace gc.caja.Models;

public sealed class AutorizacionRemotaSesion
{
    public string ClaveOperacion { get; init; } = string.Empty;
    public Guid IdSolicitud { get; init; }
    public string IdSolicitudExterna { get; init; } = string.Empty;
    public string IdempotencyKey { get; init; } = string.Empty;
    public string UsuarioSolicitante { get; init; } = string.Empty;
    public string CodigoModuloOrigen { get; init; } = string.Empty;
    public int DerCodigo { get; init; }
    public string ContextoSha256 { get; init; } = string.Empty;
    public DateTime FechaRegistroUtc { get; init; }
}

public sealed class AutorizacionRemotaConsultaResultado
{
    public bool Vigente { get; init; }
    public string Estado { get; init; } = string.Empty;
    public SolicitudAutorizacionRespuesta? Solicitud { get; init; }

    public static AutorizacionRemotaConsultaResultado Reemplazada() => new()
    {
        Vigente = false,
        Estado = "REEMPLAZADA"
    };
}

public interface IAutorizacionRemotaOrquestador
{
    Task<AutorizacionRemotaSesion> IniciarAsync(
        string claveOperacion,
        CrearAutorizacionRemotaSolicitud solicitud,
        string idempotencyKey,
        string token,
        CancellationToken cancellationToken = default);

    Task<AutorizacionRemotaConsultaResultado> ConsultarAsync(
        string claveOperacion,
        Guid idSolicitud,
        string token,
        CancellationToken cancellationToken = default);

    AutorizacionRemotaSesion? ObtenerVigente(string claveOperacion);

    bool EsVigente(string claveOperacion, Guid idSolicitud);

    void Completar(string claveOperacion, Guid idSolicitud);
}
