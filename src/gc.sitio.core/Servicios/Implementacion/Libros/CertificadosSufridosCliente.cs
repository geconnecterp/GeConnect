using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace gc.sitio.core.Servicios.Implementacion.Libros;

public class CertificadosSufridosCliente(IOptions<AppSettings> options, IOptions<DocsManager> docs)
{
    private static readonly HttpClient Cliente = new(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { Timeout = TimeSpan.FromMinutes(10) };
    private static readonly JsonSerializerSettings Json = new() { TypeNameHandling = TypeNameHandling.None, FloatParseHandling = FloatParseHandling.Decimal };
    public async Task<List<SufridoImpuesto>> Opciones(string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar(options.Value.RutaBase, "opciones", null, token, perfil, menu, ct);
        return JsonConvert.DeserializeObject<List<SufridoImpuesto>>(await response.Content.ReadAsStringAsync(ct), Json)
            ?? throw new InvalidOperationException("La API no devolvió una lista de impuestos válida.");
    }
    public async Task<CertificadosSufridosConsulta> Consultar(CertificadosSufridosFiltro filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar(options.Value.RutaBase, "consultar", filtro, token, perfil, menu, ct);
        return JsonConvert.DeserializeObject<CertificadosSufridosConsulta>(await response.Content.ReadAsStringAsync(ct), Json)
            ?? throw new InvalidOperationException("La API no devolvió una consulta válida.");
    }
    public async Task<byte[]> Pdf(CertificadosSufridosFiltro filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar(docs.Value.ApiReporteUrl, "pdf", filtro, token, perfil, menu, ct);
        if (response.Content.Headers.ContentType?.MediaType != "application/pdf") throw new InvalidOperationException("No se recibió un PDF válido.");
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        if (bytes.Length < 5 || Encoding.ASCII.GetString(bytes, 0, 5) != "%PDF-") throw new InvalidOperationException("El reporte recibido está vacío o no es PDF.");
        return bytes;
    }
    private static async Task<HttpResponseMessage> Enviar(string? baseUrl, string accion, object? filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(filtro == null ? HttpMethod.Get : HttpMethod.Post,
            IvaVentasCliente.Url(baseUrl, "api/certificados-sufridos/" + accion));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Geco-Perfil", perfil); request.Headers.Add("X-Geco-Menu", menu);
        if (filtro != null) request.Content = new StringContent(JsonConvert.SerializeObject(filtro, Json), Encoding.UTF8, "application/json");
        var response = await Cliente.SendAsync(request, ct);
        if (response.IsSuccessStatusCode)
        {
            if (response.Content.Headers.ContentType?.MediaType == (accion == "pdf" ? "application/pdf" : "application/json")) return response;
            response.Dispose(); throw new InvalidOperationException("La API no devolvió el formato esperado.");
        }
        var status = (int)response.StatusCode;
        var mensaje = status is 401 or 403 ? "La sesión venció o el perfil no tiene acceso a Certificados Sufridos (imp_cs)." : "No se pudo completar la consulta o reporte. No se entregaron datos parciales.";
        try
        {
            if (status is 400 or 403 or 422 or 429 or 503 or 504 && response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                var json = JObject.Parse(await response.Content.ReadAsStringAsync(ct));
                if (json["mensaje"]?.Type == JTokenType.String) mensaje = json["mensaje"]!.Value<string>()!;
            }
        }
        catch (JsonException) { }
        finally { response.Dispose(); }
        throw new CertificadosSufridosHttpException(status is 400 or 401 or 403 or 422 or 429 or 503 or 504 ? status : 503, mensaje);
    }
}
public class CertificadosSufridosHttpException(int estado, string mensaje) : InvalidOperationException(mensaje)
{ public int Estado { get; } = estado; }
