namespace gc.infraestructura.Dtos.SolAuth.Comando;

public class ResolverSolicitudAutorizacionComando
{
    public string Decision { get; set; } = string.Empty; // "APROBADO" o "RECHAZADO"
    public string CodigoResolucion { get; set; } = string.Empty;
    public string? Mensaje { get; set; }
}
