namespace gc.sitio.Helpers;

public static class EnlacePublicoUrl
{
    public static string ResolverBase(string? publicBaseUrl, string fallback, bool legacyEnabled = true)
    {
        if (string.IsNullOrWhiteSpace(publicBaseUrl))
        {
            if (!legacyEnabled) throw new ArgumentException("Configurar la dirección pública antes de desactivar la descarga heredada.");
            return fallback.TrimEnd('/');
        }
        if (!Uri.TryCreate(publicBaseUrl.Trim(), UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps || string.IsNullOrEmpty(uri.Host)
            || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment))
            throw new ArgumentException("La dirección pública de documentos debe ser HTTPS, sin credenciales, parámetros ni fragmentos.");
        return uri.AbsoluteUri.TrimEnd('/');
    }
}
