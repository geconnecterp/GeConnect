using System;

namespace RemoteAuthorizations.Application.Responses;

public class ResolucionAutorizacionRespuesta
{
    public Guid Id { get; set; }
    public Guid IdSolicitud { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string CodigoResolucion { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
    public string IdUsuarioResolucion { get; set; } = string.Empty;
    public bool EsResolucionPorDefecto { get; set; }
    public DateTime FechaResolucion { get; set; }
}
