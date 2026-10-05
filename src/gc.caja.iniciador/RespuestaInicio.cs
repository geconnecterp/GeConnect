using System.Text.Json;

internal static class RespuestaInicio
{
    public static async Task<string> LeerPaseAsync(HttpResponseMessage respuesta, Uri endpoint)
    {
        var estado = (int)respuesta.StatusCode;
        var ubicacion = endpoint.GetLeftPart(UriPartial.Path);
        if (estado is >= 300 and < 400)
            throw new InvalidOperationException($"El servidor redirigió la identificación del puesto (HTTP {estado}). Endpoint: {ubicacion}. Esta dirección debe responder JSON sin iniciar sesión. Verifique que gc.caja esté actualizado en el servidor y que la URL incluya la ruta de la aplicación, si corresponde.");
        if (!respuesta.IsSuccessStatusCode)
            throw new InvalidOperationException($"El servidor no pudo preparar el puesto (HTTP {estado}). Endpoint: {ubicacion}. Verifique la publicación de gc.caja y su configuración de acceso.");
        var contenido = await respuesta.Content.ReadAsStringAsync();
        var tipo = respuesta.Content.Headers.ContentType?.MediaType;
        if (tipo?.Contains("html", StringComparison.OrdinalIgnoreCase) == true || contenido.TrimStart().StartsWith("<"))
            throw new InvalidOperationException($"El servidor devolvió una página HTML en lugar de JSON. Endpoint: {ubicacion}. Puede ser la página de ingreso o una página de error. Verifique la versión publicada de gc.caja y la ruta de la aplicación.");
        try
        {
            using var datos = JsonDocument.Parse(contenido);
            if (datos.RootElement.ValueKind == JsonValueKind.Object &&
                datos.RootElement.TryGetProperty("pase", out var campo) && campo.ValueKind == JsonValueKind.String)
            {
                var pase = campo.GetString();
                if (pase?.Length == 64 && pase.All(Uri.IsHexDigit)) return pase;
            }
        }
        catch (JsonException)
        {
            throw new InvalidOperationException($"El servidor no devolvió JSON válido. Endpoint: {ubicacion}. Verifique la publicación de gc.caja.");
        }
        throw new InvalidOperationException($"El servidor no devolvió un pase de inicio válido. Endpoint: {ubicacion}. Verifique que el iniciador y gc.caja estén actualizados.");
    }
}
