using gc.infraestructura.Enumeraciones;

namespace gc.api.core.Entidades.SAuth;

public class ResolucionAutorizacion :EntidadBase
{
    public Guid Id { get; private set; }
    public Guid IdSolicitud { get; private set; }
    public DecisionAutorizacion Decision { get; private set; }
    public string CodigoResolucion { get; private set; } = string.Empty;
    public string? Mensaje { get; private set; }
    public string IdUsuarioResolucion { get; private set; } = string.Empty;
    public bool EsResolucionPorDefecto { get; private set; }
    public DateTime FechaResolucion { get; private set; }

    public ResolucionAutorizacion() { }

    internal ResolucionAutorizacion(
        Guid idSolicitud,
        DecisionAutorizacion decision,
        string codigoResolucion,
        string? mensaje,
        string idUsuarioResolucion,
        bool esResolucionPorDefecto)
    {
        Id = Guid.NewGuid();
        IdSolicitud = idSolicitud;
        Decision = decision;
        CodigoResolucion = codigoResolucion;
        Mensaje = mensaje;
        IdUsuarioResolucion = idUsuarioResolucion;
        EsResolucionPorDefecto = esResolucionPorDefecto;
        FechaResolucion = DateTime.UtcNow;
    }
}
