namespace Geco.Reportes.Publico.Models;

public sealed record ContextoDescarga(string? Ip, string? UserAgent, string Correlacion);
public sealed record DocumentoPdf(byte[] Contenido, string Nombre);
public sealed record AvisoModel(string Titulo, string Mensaje, string Referencia);

// Nunca contiene errores técnicos, códigos de acceso ni mensajes recibidos del backend.
public sealed class DescargaException(int status, string mensaje) : Exception(mensaje)
{
    public int Status { get; } = status;
    public static DescargaException Enlace() => new(410,
        "El enlace no está disponible. Puede haber vencido o alcanzado su límite de descargas. Solicitá un nuevo enlace a quien te envió el documento.");
    public static DescargaException Servicio() => new(503,
        "No pudimos obtener el documento en este momento. Comunicate con quien te envió el enlace e indicá la referencia de esta pantalla.");
}
