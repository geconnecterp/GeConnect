using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace gc.sitio.core.Servicios.Implementacion.Libros;

public class IvaVentasResumenCliente(IOptions<AppSettings> settings, IOptions<DocsManager> docs)
{
    private static readonly HttpClient Cliente = new(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { Timeout = TimeSpan.FromMinutes(10) };
    public async Task Acceso(string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar(settings.Value.RutaBase, "api/iva-ventas-resumen/acceso", null, token, perfil, menu, ct);
    }
    public async Task<IvaVentasResumenLibro> Consultar(IvaVentasResumenFiltro filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar(settings.Value.RutaBase, "api/iva-ventas-resumen/consultar", filtro, token, perfil, menu, ct);
        var filas = JsonConvert.DeserializeObject<List<IvaVentasResumenFila>>(await response.Content.ReadAsStringAsync(ct),
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, FloatParseHandling = FloatParseHandling.Decimal })
            ?? throw new InvalidOperationException("La API no devolvió un resumen válido.");
        return IvaVentasResumenLibro.Crear(filtro, filas);
    }
    public async Task<byte[]> Excel(IvaVentasResumenFiltro filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar(docs.Value.ApiReporteUrl, "api/iva-ventas-resumen/excel", filtro, token, perfil, menu, ct);
        if (response.Content.Headers.ContentType?.MediaType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            throw new InvalidOperationException("No se recibió un archivo Excel válido.");
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        if (bytes.Length < 4 || bytes[0] != 0x50 || bytes[1] != 0x4b) throw new InvalidOperationException("El Excel recibido está vacío o no es válido.");
        return bytes;
    }
    private static async Task<HttpResponseMessage> Enviar(string? url, string ruta, IvaVentasResumenFiltro? filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(filtro == null ? HttpMethod.Get : HttpMethod.Post, IvaVentasCliente.Url(url, ruta));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Geco-Perfil", perfil);
        request.Headers.Add("X-Geco-Menu", menu);
        if (filtro != null) request.Content = new StringContent(JsonConvert.SerializeObject(filtro), Encoding.UTF8, "application/json");
        var response = await Cliente.SendAsync(request, ct);
        if (response.IsSuccessStatusCode) return response;
        var status = response.StatusCode;
        string? mensaje = null;
        if (status == HttpStatusCode.BadRequest && response.Content.Headers.ContentType?.MediaType == "application/json")
        {
            try { mensaje = JObject.Parse(await response.Content.ReadAsStringAsync(ct))["mensaje"]?.Value<string>(); }
            catch (JsonException) { }
        }
        response.Dispose();
        if (status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            throw new UnauthorizedAccessException("La sesión venció o el perfil no tiene acceso a Consulta Resumen (imp_lv_res).");
        if (!string.IsNullOrWhiteSpace(mensaje)) throw new ArgumentException(mensaje);
        if ((int)status == 429) throw new InvalidOperationException("Hay otros reportes en curso. Intente nuevamente en unos instantes.");
        throw new InvalidOperationException("No se pudo completar el resumen de IVA Ventas. Verifique la conexión y el SP. No se entregan resultados parciales.");
    }
}
