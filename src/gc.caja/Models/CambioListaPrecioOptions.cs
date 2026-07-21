namespace gc.caja.Models;

public sealed class CambioListaPrecioOptions
{
    public const string Seccion = "AutorizacionRemota:CambioListaPrecio";

    public int TimeoutSegundos { get; set; } = 120;
}

public sealed class SolicitarCambioListaPrecioRequest
{
    public string LpId { get; set; } = string.Empty;
}

