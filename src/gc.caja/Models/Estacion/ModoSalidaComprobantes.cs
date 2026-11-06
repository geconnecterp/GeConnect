namespace gc.caja.Models.Estacion;

// Valor funcional común: la vista previa se solicita expresamente en el puesto.
public static class ModoSalidaComprobantes
{
    public static string Resolver(string? modo) =>
        string.Equals(modo?.Trim(), "PANTALLA", StringComparison.OrdinalIgnoreCase)
            ? "PANTALLA" : "IMPRESORA";
}
