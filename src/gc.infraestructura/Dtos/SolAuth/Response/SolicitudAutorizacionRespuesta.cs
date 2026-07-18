using System;

namespace RemoteAuthorizations.Application.Responses;

public class SolicitudAutorizacionRespuesta
{
    public Guid Id { get; set; }
    public string IdSolicitudExterna { get; set; } = string.Empty;
    public int DerCodigo { get; set; }
    public string DerechoDescripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string IdUsuarioSolicitante { get; set; } = string.Empty;
    public string CodigoModuloOrigen { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; }
    public int TimeoutSegundos { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public string ContextoJson { get; set; } = string.Empty;
    
    // Si ya está bloqueada/tomada por alguien
    public string? IdUsuarioBloqueo { get; set; }
    public DateTime? FechaBloqueo { get; set; }

    // Indica si el usuario actual tiene permisos para autorizar esto
    public bool PuedeAutorizar { get; set; }

    //public string? IdSolicitudExterna { get; set; }
    public ResolucionAutorizacionRespuesta? Resolucion { get; set; }
}
