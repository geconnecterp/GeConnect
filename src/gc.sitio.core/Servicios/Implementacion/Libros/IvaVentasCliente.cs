using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace gc.sitio.core.Servicios.Implementacion.Libros;

public class IvaVentasCliente(IOptions<AppSettings> settings, IOptions<DocsManager> docs)
{
    private static readonly HttpClient Cliente = new(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { Timeout = TimeSpan.FromMinutes(10) };
    private static readonly JsonSerializerSettings Json = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        TypeNameHandling = TypeNameHandling.None
    };

    public async Task<IvaVentasOpciones> Opciones(string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar(settings.Value.RutaBase, "api/iva-ventas/opciones", null, token, perfil, menu, ct);
        return JsonConvert.DeserializeObject<IvaVentasOpciones>(await response.Content.ReadAsStringAsync(ct), Json)
            ?? throw new InvalidOperationException("Las listas recibidas no son válidas.");
    }

    public async Task<IvaVentasPagina> Consultar(IvaVentasFiltro filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar(settings.Value.RutaBase, "api/iva-ventas/consultar", filtro, token, perfil, menu, ct);
        return JsonConvert.DeserializeObject<IvaVentasPagina>(await response.Content.ReadAsStringAsync(ct), Json)
            ?? throw new InvalidOperationException("La respuesta de IVA no es válida.");
    }

    public async Task<byte[]> Excel(IvaVentasFiltro filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar(docs.Value.ApiReporteUrl, "api/iva-ventas/excel", filtro, token, perfil, menu, ct);
        if (response.Content.Headers.ContentType?.MediaType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            throw new InvalidOperationException("La API de reportes no devolvió un Excel válido.");
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        if (bytes.Length < 4 || bytes[0] != 0x50 || bytes[1] != 0x4b) throw new InvalidOperationException("El archivo Excel recibido está vacío o es inválido.");
        return bytes;
    }

    public static Uri Url(string? baseUrl, string ruta)
    {
        if (!Uri.TryCreate(baseUrl?.TrimEnd('/') + "/", UriKind.Absolute, out var uri) ||
            (uri.Scheme != "https" && uri.Scheme != "http") || uri.Query.Length > 0 || uri.Fragment.Length > 0 || uri.UserInfo.Length > 0)
            throw new InvalidOperationException("La URL de la API no está configurada correctamente.");
        return new Uri(uri, ruta);
    }

    private static async Task<HttpResponseMessage> Enviar(string? url, string ruta, IvaVentasFiltro? filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(filtro == null ? HttpMethod.Get : HttpMethod.Post, Url(url, ruta));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Geco-Perfil", perfil);
        request.Headers.Add("X-Geco-Menu", menu);
        if (filtro != null) request.Content = new StringContent(JsonConvert.SerializeObject(filtro, Json), Encoding.UTF8, "application/json");
        var response = await Cliente.SendAsync(request, ct);
        if (response.IsSuccessStatusCode) return response;
        var status = response.StatusCode;
        string? validacion = null;
        IvaVentasDatoInvalidoException? errorDatos = null;
        if (status == HttpStatusCode.UnprocessableEntity && response.Content.Headers.ContentType?.MediaType == "application/json")
            errorDatos = LeerErrorDatos(await response.Content.ReadAsStringAsync(ct));
        if (status == HttpStatusCode.BadRequest && response.Content.Headers.ContentType?.MediaType == "application/json")
        {
            try
            {
                var body = JObject.Parse(await response.Content.ReadAsStringAsync(ct));
                if (body["mensaje"]?.Type == JTokenType.String)
                    validacion = body["mensaje"]!.Value<string>();
            }
            catch (JsonException) { /* No propagar contenido ajeno al contrato. */ }
        }
        response.Dispose();
        if (errorDatos != null) throw errorDatos;
        if (!string.IsNullOrEmpty(validacion)) throw new ArgumentException(validacion);
        if (status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            throw new UnauthorizedAccessException("La sesión no es válida o el perfil no tiene acceso a Detalle IVA Ventas.");
        if ((int)status == 429) throw new InvalidOperationException("Hay otras exportaciones en curso. Intente nuevamente en unos instantes.");
        throw new InvalidOperationException("La API no pudo completar la operación. Revise los filtros, la conexión y la versión del SP. No se entregó un archivo parcial.");
    }

    public static IvaVentasDatoInvalidoException? LeerErrorDatos(string contenido)
    {
        try
        {
            var body = JObject.Parse(contenido);
            if (body["codigo"]?.Type != JTokenType.String || body["codigo"]!.Value<string>() != IvaVentasDatoInvalidoException.Codigo ||
                body["campo"]?.Type != JTokenType.String) return null;
            var campo = body["campo"]!.Value<string>();
            // Reconstruir un mensaje propio: no presentar HTML ni errores internos de un proxy/API.
            return campo != null && typeof(IvaVentasDetalleDto).GetProperty(campo) != null
                ? new IvaVentasDatoInvalidoException(campo) : null;
        }
        catch (JsonException) { return null; }
    }
}
