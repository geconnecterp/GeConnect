using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Libros;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace gc.sitio.core.Servicios.Implementacion.Libros;

public class IvaVentasCierreCliente(IOptions<AppSettings> settings)
{
    private static readonly HttpClient Cliente = new(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { Timeout = TimeSpan.FromMinutes(11) };
    private static readonly JsonSerializerSettings Json = new() { TypeNameHandling = TypeNameHandling.None, FloatParseHandling = FloatParseHandling.Decimal };
    public async Task Acceso(string token, string perfil, string menu, CancellationToken ct)
    { using var response = await Enviar("acceso", null, token, perfil, menu, ct); }
    public async Task<IvaVentasCierreEstado> Estado(string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar("estado", null, token, perfil, menu, ct);
        var estado = JsonConvert.DeserializeObject<IvaVentasCierreEstado>(await response.Content.ReadAsStringAsync(ct), Json)
            ?? throw new InvalidOperationException("La API no devolvió un historial válido.");
        estado.Validar();
        return estado;
    }
    public async Task<IvaVentasCierreRespuesta> Cerrar(IvaVentasCierrePedido pedido, string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar("cerrar", pedido, token, perfil, menu, ct);
        return JsonConvert.DeserializeObject<IvaVentasCierreRespuesta>(await response.Content.ReadAsStringAsync(ct), Json)
            ?? throw new InvalidOperationException("No se recibió el resultado del cierre.");
    }
    public async Task<FileStream> Rg3685(IvaVentasCierrePedido pedido, string token, string perfil, string menu, CancellationToken ct)
    {
        using var response = await Enviar("rg3685", pedido, token, perfil, menu, ct);
        if (response.Content.Headers.ContentType?.MediaType != "application/zip") throw new InvalidOperationException("No se recibió un ZIP válido.");
        var file = new FileStream(Path.Combine(Path.GetTempPath(), "geco-iva-sitio-" + Guid.NewGuid().ToString("N") + ".zip"),
            FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
        try
        {
            await response.Content.CopyToAsync(file, ct);
            file.Position = 0;
            if (file.Length < 4 || file.ReadByte() != 0x50 || file.ReadByte() != 0x4b)
                throw new InvalidOperationException("La descarga no contiene un ZIP válido.");
            file.Position = 0;
            return file;
        }
        catch { await file.DisposeAsync(); throw; }
    }
    private async Task<HttpResponseMessage> Enviar(string accion, IvaVentasCierrePedido? pedido, string token, string perfil, string menu, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(pedido == null ? HttpMethod.Get : HttpMethod.Post,
            IvaVentasCliente.Url(settings.Value.RutaBase, "api/iva-ventas-cierre/" + accion));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Geco-Perfil", perfil); request.Headers.Add("X-Geco-Menu", menu);
        if (pedido != null) request.Content = new StringContent(JsonConvert.SerializeObject(pedido, Json), Encoding.UTF8, "application/json");
        var response = await Cliente.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (response.IsSuccessStatusCode) return response;
        var status = (int)response.StatusCode;
        string mensaje = status is 401 or 403 ? "La sesión venció o el perfil no tiene acceso a Cierre IVA Ventas." :
            accion == "cerrar" ? "No se pudo confirmar el resultado del cierre. Actualice el historial antes de volver a intentarlo." : "No se pudo completar la consulta o descarga.";
        try
        {
            if (status is 400 or 403 or 409 or 429 or 503 or 504 && response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                var json = JObject.Parse(await response.Content.ReadAsStringAsync(ct));
                if (json["mensaje"]?.Type == JTokenType.String) mensaje = json["mensaje"]!.Value<string>()!;
            }
        }
        catch (JsonException) { }
        finally { response.Dispose(); }
        throw new IvaVentasCierreHttpException(status is 401 or 403 or 400 or 409 or 429 or 503 or 504 ? status : 503, mensaje);
    }
}

public class IvaVentasCierreHttpException(int estado, string mensaje) : InvalidOperationException(mensaje)
{ public int Estado { get; } = estado; }
