using System;

namespace gc.api.core.Entidades.SAuth;

public class MensajeBandejaSalida : EntidadBase
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime FechaOcurrencia { get; set; }
    public DateTime? FechaProcesado { get; set; }
    public int Intentos { get; set; }
    public string? Error { get; set; }
}
