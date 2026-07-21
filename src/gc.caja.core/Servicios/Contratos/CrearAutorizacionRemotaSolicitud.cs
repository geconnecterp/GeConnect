using Newtonsoft.Json.Linq;

namespace gc.caja.core.Autorizaciones;

/// <summary>
/// Contrato agnóstico de negocio utilizado por cualquier módulo de la aplicación.
/// </summary>
public sealed class CrearAutorizacionRemotaSolicitud
{
    public string UsuarioSolicitante { get; init; } = string.Empty;
    public string IdSolicitudExterna { get; init; } = string.Empty;
    public int DerCodigo { get; init; }
    public int TimeoutSegundos { get; init; }
    public string DecisionPorDefecto { get; init; } = "RECHAZADO";
    public string CodigoResolucionPorDefecto { get; init; } = "TIMEOUT";
    public string? MensajeResolucionPorDefecto { get; init; }
    public JToken Contexto { get; init; } = new JObject();
}
