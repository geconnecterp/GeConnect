namespace gc.infraestructura.Helpers;

// Normaliza el contrato de salida del SP. EM es un canal reservado, no un papel imprimible.
public static class FormatoPapelComprobante
{
    public static string Resolver(string? codigo) => codigo?.Trim().ToUpperInvariant() switch
    {
        null or "" => "A4",
        "A4" => "A4",
        "TK" => "TK",
        "EM" => "EM",
        _ => throw new ArgumentException("Formato de comprobante desconocido. Revise ctrl_papel del punto de venta.")
    };
}
