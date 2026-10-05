using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using gc.infraestructura.Dtos.Cajas;

namespace gc.caja.Models.Administracion;

public record ResultadoGeneral(bool Ok, string Mensaje, bool Incierto = false, string? Proceso = null);
public record ConsultaPuestos(bool Ok, string Mensaje, List<CajaPVAbiertosDto> Puestos);

// Contrato exclusivo de Caja: una respuesta vacía nunca significa éxito ni ausencia de puestos.
public class CajaGeneralServicio(HttpClient client, ILogger<CajaGeneralServicio> logger)
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private static JsonElement Datos(JsonElement root) => root.EnumerateObject()
        .First(p => p.Name.Equals("data", StringComparison.OrdinalIgnoreCase)).Value;
    private static HttpRequestMessage Solicitud(HttpMethod metodo, string ruta, string token)
    {
        var request = new HttpRequestMessage(metodo, "api/apicaja/" + ruta);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
    public async Task<ConsultaPuestos> Consultar(string sucursal, string token)
    {
        try
        {
            using var request = Solicitud(HttpMethod.Get, "ObtenerPVAbiertos?admId=" + Uri.EscapeDataString(sucursal), token);
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var data = Datos(json.RootElement);
            if (data.ValueKind != JsonValueKind.Array) throw new JsonException("Falta la lista de puestos.");
            var puestos = data.Deserialize<List<CajaPVAbiertosDto>>(Json)!;
            if (puestos.Any(p => p == null || string.IsNullOrWhiteSpace(p.caja_id)))
                throw new JsonException("Lista de puestos incompleta.");
            return new(true, puestos.Count == 0 ? "No hay puestos abiertos en esta sucursal." : "Debe cerrar los puestos abiertos antes del cierre general.", puestos);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error consultando puestos abiertos de sucursal {Sucursal}", sucursal);
            return new(false, "No se pudo verificar si hay puestos abiertos. Actualice la consulta; el cierre general permanece bloqueado.", []);
        }
    }
    public async Task<ResultadoGeneral> Ejecutar(bool apertura, string usuario, string sucursal, string token)
    {
        if (!apertura)
        {
            var consulta = await Consultar(sucursal, token);
            if (!consulta.Ok || consulta.Puestos.Count != 0) return new(false, consulta.Mensaje);
        }
        try
        {
            using var request = Solicitud(HttpMethod.Post, apertura ? "HabilitarCajaGral" : "CierreCajaGral", token);
            request.Content = JsonContent.Create(new { usu_id = usuario, adm_id = sucursal });
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var data = Datos(json.RootElement);
            if (!data.TryGetProperty("resultado", out var codigo) || !codigo.TryGetInt16(out var valor))
                throw new JsonException("Falta resultado explícito.");
            string mensaje = data.TryGetProperty("resultado_msj", out var msj) ? msj.GetString() ?? "" : "";
            string? proceso = data.TryGetProperty("resultado_id", out var id) ? id.ToString() : null;
            if (valor != 0) return new(false, string.IsNullOrWhiteSpace(mensaje) ? "La operación fue rechazada por el servidor." : mensaje);
            if (apertura && string.IsNullOrWhiteSpace(proceso))
                return new(false, "El servidor informó éxito sin número de proceso. Verifique la habilitación antes de repetirla.", true);
            return new(true, apertura ? "Las cajas de la sucursal se habilitaron correctamente. Número de proceso de cobro: " + proceso :
                "Se realizó correctamente el cierre general de cajas de la sucursal.", false, proceso);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Resultado incierto de operación general {Apertura} en sucursal {Sucursal}", apertura, sucursal);
            return new(false, "No se pudo confirmar el resultado. Verifique el estado de la sucursal antes de repetir la operación.", true);
        }
    }
}
