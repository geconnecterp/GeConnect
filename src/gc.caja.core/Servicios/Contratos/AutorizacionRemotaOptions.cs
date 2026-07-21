namespace gc.caja.core.Autorizaciones;

/// <summary>
/// Configuración propia de la aplicación que consume autorizaciones remotas.
/// Cada aplicación debe declarar un código de origen estable.
/// </summary>
public sealed class AutorizacionRemotaOptions
{
    public const string Seccion = "AutorizacionRemota";

    public string CodigoModuloOrigen { get; set; } = string.Empty;
    public string RutaApi { get; set; } = "api/SolicitudesAutorizacion";
    public int TimeoutHttpSegundos { get; set; } = 30;
    public int IntervaloConsultaMilisegundos { get; set; } = 2000;
}
