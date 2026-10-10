using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using gc.infraestructura.Dtos.Cajas;

namespace gc.caja.Models.Administracion;

public record ResultadoGeneral(bool Ok, string Mensaje, bool Incierto = false, string? Proceso = null);

public record ConsultaGeneral(bool Ok, string Mensaje, bool? Habilitada, List<CajaPVAbiertosDto> Puestos);

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

    public async Task<ConsultaGeneral> ConsultarEstado(string caja, string sucursal, string token)
    {
        if (string.IsNullOrWhiteSpace(caja))
            return new(false, "No se identificó el puesto de esta estación para consultar la habilitación de la sucursal.", null, []);
        bool habilitada;
        try
        {
            using var request = Solicitud(HttpMethod.Get, "ObtenerDatosCF?caja_id=" + Uri.EscapeDataString(caja), token);
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var datos = Datos(json.RootElement).Deserialize<CajaDatosDto>(Json);
            if (datos == null || datos.caja_id?.Trim() != caja.Trim() || datos.adm_id?.Trim() != sucursal.Trim())
                throw new JsonException("El puesto no corresponde a la sucursal de la sesión.");
            var valor = datos.caja_habilitadas?.Trim().ToUpperInvariant();
            if (valor != "S" && valor != "N") throw new JsonException("Habilitación general ausente o inválida.");
            habilitada = valor == "S";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error consultando habilitación de sucursal {Sucursal}, puesto {Caja}", sucursal, caja);
            return new(false, "No se pudo determinar la habilitación general de la sucursal. Actualice la consulta; verifique que la API informe caja_habilitadas con S/N.", null, []);
        }
        if (!habilitada) return new(true, "La sucursal no está habilitada. Puede realizar la apertura general de cajas.", false, []);
        var puestos = await Consultar(sucursal, token);
        return new(puestos.Ok, puestos.Mensaje, true, puestos.Puestos);
    }

    public async Task<ResultadoGeneral> Ejecutar(bool apertura, string usuario, string sucursal, string caja, string token)
    {
        var estado = await ConsultarEstado(caja, sucursal, token);
        if (!estado.Ok) return new(false, estado.Mensaje);
        if (apertura && estado.Habilitada != false)
            return new(false, "La sucursal ya está habilitada. No corresponde realizar otra apertura general.");
        if (!apertura && estado.Habilitada != true)
            return new(false, "La sucursal no está habilitada. No corresponde realizar un cierre general.");
        if (!apertura && estado.Puestos.Count != 0) return new(false, estado.Mensaje);
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
