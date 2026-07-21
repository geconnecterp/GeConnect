using gc.infraestructura.Enumeraciones;

namespace gc.api.core.Entidades.SAuth;

public class SolicitudAutorizacion : EntidadBase
{
    public Guid Id { get; private set; }
    public string CodigoModuloOrigen { get; private set; }
    public string IdUsuarioSolicitante { get; private set; }
    public int DerCodigo { get; private set; }
    public string DerechoDescripcion { get; private set; } = string.Empty;
    public string IdSolicitudExterna { get; private set; }
    public EstadoAutorizacion Estado { get; private set; }
    public int TimeoutSegundos { get; private set; }
    public DateTime FechaExpiracion { get; private set; }
    public bool PuedeAutorizar { get; set; } // Populated dynamically by GenericDataMapper
    public DecisionAutorizacion DecisionPorDefecto { get; private set; }
    public string CodigoResolucionPorDefecto { get; private set; }
    public string ContextoJson { get; private set; }

    public DateTime FechaSolicitud { get; private set; }
    public DateTime? FechaActualizacion { get; private set; }
    public byte[] RowVersion { get; private set; }

    public ResolucionAutorizacion? Resolucion { get; private set; }

    public string? IdUsuarioBloqueo { get; private set; }
    public DateTime? FechaBloqueo { get; private set; }
    public string IdempotencyKey { get; private set; }
    public SolicitudAutorizacion() { }
    public SolicitudAutorizacion(
        string codigoModuloOrigen,
        string idUsuarioSolicitante,
        int derCodigo,
        string idSolicitudExterna,
        int timeoutSegundos,
        DecisionAutorizacion decisionPorDefecto,
        string codigoResolucionPorDefecto,
        string contextoJson,
        string idempotencyKey)
    {
        Id = Guid.NewGuid();
        CodigoModuloOrigen = codigoModuloOrigen;
        IdUsuarioSolicitante = idUsuarioSolicitante;
        DerCodigo = derCodigo;
        IdSolicitudExterna = idSolicitudExterna;
        Estado = EstadoAutorizacion.PENDIENTE;
        TimeoutSegundos = timeoutSegundos;
        FechaSolicitud = DateTime.UtcNow;
        FechaExpiracion = FechaSolicitud.AddSeconds(timeoutSegundos);
        DecisionPorDefecto = decisionPorDefecto;
        CodigoResolucionPorDefecto = codigoResolucionPorDefecto;
        ContextoJson = contextoJson;
        IdempotencyKey = idempotencyKey;
    }
    public void Bloquear(string idUsuario)
    {
        ValidarUsuarioAutorizador(idUsuario);

        if (Estado != EstadoAutorizacion.PENDIENTE)
        {
            throw new InvalidOperationException($"No se puede tomar una solicitud en estado {Estado}");
        }
        if (!string.IsNullOrWhiteSpace(IdUsuarioBloqueo) && IdUsuarioBloqueo != idUsuario)
        {
            throw new InvalidOperationException($"La solicitud ya está siendo atendida por el usuario {IdUsuarioBloqueo}");
        }
        IdUsuarioBloqueo = idUsuario;
        FechaBloqueo = DateTime.UtcNow;
        Estado = EstadoAutorizacion.EN_PROCESO;
    }
    public void Resolver(
        DecisionAutorizacion decision,
        string codigoResolucion,
        string? mensaje,
        string idUsuarioResolucion,
        bool esResolucionPorDefecto)
    {
        ValidarUsuarioAutorizador(idUsuarioResolucion);

        if (Estado != EstadoAutorizacion.PENDIENTE && Estado != EstadoAutorizacion.EN_PROCESO)
        {
            throw new InvalidOperationException($"No se puede resolver una solicitud en estado {Estado}");
        }
        Estado = EstadoAutorizacion.RESUELTO;
        FechaActualizacion = DateTime.UtcNow;

        Resolucion = new ResolucionAutorizacion(
            Id,
            decision,
            codigoResolucion,
            mensaje,
            idUsuarioResolucion,
            esResolucionPorDefecto);
    }

    public void ResolverAutomaticamentePorPosesionDerecho()
    {
        if (Estado != EstadoAutorizacion.PENDIENTE && Estado != EstadoAutorizacion.EN_PROCESO)
        {
            throw new InvalidOperationException($"No se puede resolver una solicitud en estado {Estado}");
        }

        Estado = EstadoAutorizacion.RESUELTO;
        FechaActualizacion = DateTime.UtcNow;
        IdUsuarioBloqueo = IdUsuarioSolicitante;
        FechaBloqueo = FechaActualizacion;

        Resolucion = new ResolucionAutorizacion(
            Id,
            DecisionAutorizacion.APROBADO,
            "POSESION_DERECHO",
            "Autorizacion automatica por posesion del derecho requerido.",
            IdUsuarioSolicitante,
            false);
    }

    public void Expirar()
    {
        if (Estado != EstadoAutorizacion.PENDIENTE && Estado != EstadoAutorizacion.EN_PROCESO)
        {
            throw new InvalidOperationException($"No se puede vencer una solicitud en estado {Estado}");
        }
        Estado = EstadoAutorizacion.EXPIRADO;
        FechaActualizacion = DateTime.UtcNow;

        Resolucion = new ResolucionAutorizacion(
            Id,
            DecisionPorDefecto,
            CodigoResolucionPorDefecto,
            "Autorización no respondida dentro del tiempo permitido.",
            "SYSTEM",
            true);
    }

    private void ValidarUsuarioAutorizador(string idUsuarioAutorizador)
    {
        if (string.Equals(
            IdUsuarioSolicitante?.Trim(),
            idUsuarioAutorizador?.Trim(),
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "El usuario solicitante no puede autorizar su propia solicitud.");
        }
    }
}
