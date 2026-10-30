using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using gc.infraestructura.Dtos.Cajas;

namespace gc.caja.Models.Administracion;

public class CustodiaApiServicio(HttpClient client, ILogger<CustodiaApiServicio> logger)
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private static HttpRequestMessage Solicitud(HttpMethod metodo, string ruta, string token)
    {
        var request = new HttpRequestMessage(metodo, "api/caja/custodia/" + ruta);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    public async Task<CustodiaConsultaDto> Consultar(string token)
    {
        using var request = Solicitud(HttpMethod.Get, "consulta", token);
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = json.RootElement.EnumerateObject().First(p => p.Name.Equals("data", StringComparison.OrdinalIgnoreCase)).Value;
        return data.Deserialize<CustodiaConsultaDto>(Json) ?? throw new JsonException("Consulta de custodia vacía.");
    }

    public async Task<CustodiaResultadoDto> Ejecutar(bool entrega, CustodiaOperacionDto operacion, string token)
    {
        try
        {
            using var request = Solicitud(HttpMethod.Post, entrega ? "entregar" : "corregir", token);
            request.Content = JsonContent.Create(operacion);
            using var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
                return new(false, "La sesión no permite realizar esta operación. Ingrese nuevamente.");
            // La API devuelve el resultado explícito también para rechazos de negocio.
            using var documento = JsonDocument.Parse(body);
            if (!documento.RootElement.TryGetProperty("ok", out var confirmado) || confirmado.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                throw new JsonException("El resultado no incluye un indicador explícito de éxito o rechazo.");
            var result = JsonSerializer.Deserialize<CustodiaResultadoDto>(body, Json);
            if (result == null || string.IsNullOrWhiteSpace(result.mensaje)) throw new JsonException("Resultado de custodia incompleto.");
            if (entrega && result.ok && string.IsNullOrWhiteSpace(result.entrega_id))
                throw new JsonException("La entrega fue aceptada sin identificador.");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[CUSTODIA] Respuesta no confirmada. Solicitud={Solicitud}, Entrega={Entrega}", operacion.solicitud, entrega);
            return new(false, "No se pudo confirmar el resultado. Verifique las rendiciones antes de repetir la operación. Referencia: " + operacion.solicitud, incierto: true);
        }
    }

    public async Task<byte[]> Acuse(string entrega, string token)
    {
        using var request = Solicitud(HttpMethod.Get, "acuse?entrega=" + Uri.EscapeDataString(entrega), token);
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentType?.MediaType != "application/pdf") throw new JsonException("El servidor no devolvió el acuse en PDF.");
        return await response.Content.ReadAsByteArrayAsync();
    }
}
