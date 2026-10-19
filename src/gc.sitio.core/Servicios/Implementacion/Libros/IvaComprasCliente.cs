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

public class IvaComprasCliente(IOptions<AppSettings> settings, IOptions<DocsManager> docs)
{
    private static readonly HttpClient Cliente = new(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { Timeout = TimeSpan.FromMinutes(11) };
    private static readonly JsonSerializerSettings Json = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        TypeNameHandling = TypeNameHandling.None
    };

    public async Task<IvaComprasOpciones> Opciones(string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar(settings.Value.RutaBase, "api/iva-compras/opciones", null, token, perfil, menu, ct);
        return JsonConvert.DeserializeObject<IvaComprasOpciones>(await response.Content.ReadAsStringAsync(ct), Json)
            ?? throw new InvalidOperationException("Las listas recibidas no son válidas.");
    }

    public async Task<IvaComprasPagina> Consultar(IvaComprasFiltro filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar(settings.Value.RutaBase, "api/iva-compras/consultar", filtro, token, perfil, menu, ct);
        return JsonConvert.DeserializeObject<IvaComprasPagina>(await response.Content.ReadAsStringAsync(ct), Json)
            ?? throw new InvalidOperationException("La respuesta de IVA no es válida.");
    }

    public async Task<FileStream> Excel(IvaComprasFiltro filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar(docs.Value.ApiReporteUrl, "api/iva-compras/excel", filtro, token, perfil, menu, ct);
        if (response.Content.Headers.ContentType?.MediaType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            throw new InvalidOperationException("La API de reportes no devolvió un Excel válido.");
        var archivo = new FileStream(Path.Combine(Path.GetTempPath(), "geco-iva-compras-sitio-" + Guid.NewGuid().ToString("N") + ".xlsx"),
            FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromMinutes(11));
            await response.Content.CopyToAsync(archivo, timeout.Token);
            archivo.Position = 0;
            if (archivo.Length < 4 || archivo.ReadByte() != 0x50 || archivo.ReadByte() != 0x4b)
                throw new InvalidOperationException("El archivo Excel recibido está vacío o es inválido.");
            archivo.Position = 0;
            return archivo;
        }
        catch { await archivo.DisposeAsync(); throw; }
    }

    public static Uri Url(string? baseUrl, string ruta)
    {
        if (!Uri.TryCreate(baseUrl?.TrimEnd('/') + "/", UriKind.Absolute, out var uri) ||
            (uri.Scheme != "https" && uri.Scheme != "http") || uri.Query.Length > 0 || uri.Fragment.Length > 0 || uri.UserInfo.Length > 0)
            throw new InvalidOperationException("La URL de la API no está configurada correctamente.");
        return new Uri(uri, ruta);
    }

    private static async Task<HttpResponseMessage> Enviar(string? url, string ruta, IvaComprasFiltro? filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(filtro == null ? HttpMethod.Get : HttpMethod.Post, Url(url, ruta));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Geco-Perfil", perfil);
        request.Headers.Add("X-Geco-Menu", menu);
        if (filtro != null) request.Content = new StringContent(JsonConvert.SerializeObject(filtro, Json), Encoding.UTF8, "application/json");
        var response = await Cliente.SendAsync(request, ruta.EndsWith("/excel", StringComparison.Ordinal)
            ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead, ct);
        if (response.IsSuccessStatusCode) return response;
        var status = response.StatusCode;
        string? validacion = null;
        IvaComprasDatoInvalidoException? errorDatos = null;
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
            throw new UnauthorizedAccessException("La sesión no es válida o el perfil no tiene acceso a Detalle IVA Compras.");
        if ((int)status == 429) throw new InvalidOperationException("Hay otras exportaciones en curso. Intente nuevamente en unos instantes.");
        throw new InvalidOperationException("La API no pudo completar la operación. Revise los filtros, la conexión y la versión del SP. No se entregó un archivo parcial.");
    }

    public static IvaComprasDatoInvalidoException? LeerErrorDatos(string contenido)
    {
        try
        {
            var body = JObject.Parse(contenido);
            if (body["codigo"]?.Type != JTokenType.String || body["codigo"]!.Value<string>() != IvaComprasDatoInvalidoException.Codigo ||
                body["campo"]?.Type != JTokenType.String) return null;
            var campo = body["campo"]!.Value<string>();
            // Reconstruir un mensaje propio: no presentar HTML ni errores internos de un proxy/API.
            return campo != null && typeof(IvaComprasDetalleDto).GetProperty(campo) != null
                ? new IvaComprasDatoInvalidoException(campo) : null;
        }
        catch (JsonException) { return null; }
    }
}
