using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Libros;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace gc.sitio.core.Servicios.Implementacion.Libros;

public class CertificadosRetencionCliente(IOptions<AppSettings> options)
{
    private static readonly HttpClient Cliente = new(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { Timeout = TimeSpan.FromMinutes(10) };
    private static readonly JsonSerializerSettings Json = new() { TypeNameHandling = TypeNameHandling.None, FloatParseHandling = FloatParseHandling.Decimal };
    public async Task<List<RetencionImpuesto>> Opciones(string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar("opciones", null, token, perfil, menu, ct);
        return JsonConvert.DeserializeObject<List<RetencionImpuesto>>(await response.Content.ReadAsStringAsync(ct), Json)
            ?? throw new InvalidOperationException("La API no devolvió una lista de impuestos válida.");
    }
    public async Task<CertificadosRetencionConsulta> Consultar(CertificadosRetencionFiltro filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar("consultar", filtro, token, perfil, menu, ct);
        return JsonConvert.DeserializeObject<CertificadosRetencionConsulta>(await response.Content.ReadAsStringAsync(ct), Json)
            ?? throw new InvalidOperationException("La API no devolvió una consulta válida.");
    }
    public async Task<FileStream> Ddjj(CertificadosRetencionDdjjPedido pedido, string token, string perfil, string menu, CancellationToken ct)
    {
        pedido.Validar();
        using var plazo = CancellationTokenSource.CreateLinkedTokenSource(ct);
        plazo.CancelAfter(TimeSpan.FromMinutes(10));
        using var response = await Enviar("ddjj", pedido, token, perfil, menu, plazo.Token);
        if (response.Content.Headers.ContentType?.MediaType != pedido.ContentType.Split(';')[0])
            throw new InvalidOperationException("La API no devolvió el formato de DDJJ esperado.");
        var file = new FileStream(Path.Combine(Path.GetTempPath(), "geco-ret-ddjj-sitio-" + Guid.NewGuid().ToString("N") + ".tmp"),
            FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
        try
        {
            await response.Content.CopyToAsync(file, plazo.Token);
            if (file.Length == 0) throw new InvalidOperationException("La API devolvió una descarga vacía.");
            file.Position = 0;
            if (pedido.Impuesto != "GA" && (file.ReadByte() != 0x50 || file.ReadByte() != 0x4b))
                throw new InvalidOperationException("La API no devolvió un Excel válido.");
            file.Position = 0;
            return file;
        }
        catch { await file.DisposeAsync(); throw; }
    }
    private async Task<HttpResponseMessage> Enviar(string accion, object? filtro, string token, string perfil, string menu, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(filtro == null ? HttpMethod.Get : HttpMethod.Post,
            IvaVentasCliente.Url(options.Value.RutaBase, "api/certificados-retencion-emitidos/" + accion));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Geco-Perfil", perfil); request.Headers.Add("X-Geco-Menu", menu);
        if (filtro != null) request.Content = new StringContent(JsonConvert.SerializeObject(filtro, Json), Encoding.UTF8, "application/json");
        var response = await Cliente.SendAsync(request, accion == "ddjj" ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead, ct);
        if (response.IsSuccessStatusCode) return response;
        var status = (int)response.StatusCode;
        var mensaje = status is 401 or 403 ? "La sesión venció o el perfil no tiene acceso a Certificados de Retención Emitidos (imp_ar)." : "No se pudo completar la consulta o descarga de certificados. No se entregaron datos parciales.";
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
        throw new CertificadosRetencionHttpException(status is 400 or 401 or 403 or 422 or 429 or 503 or 504 ? status : 503, mensaje);
    }
}
public class CertificadosRetencionHttpException(int estado, string mensaje) : InvalidOperationException(mensaje)
{ public int Estado { get; } = estado; }
