using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Libros;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

namespace gc.sitio.core.Servicios.Implementacion.Libros;

public class IvaComprasCierreCliente(IOptions<AppSettings> settings)
{
    private static readonly HttpClient Cliente = new(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { Timeout = TimeSpan.FromMinutes(11) };
    private static readonly JsonSerializerSettings Json = new() { TypeNameHandling = TypeNameHandling.None, FloatParseHandling = FloatParseHandling.Decimal };
    private static CancellationTokenSource Presupuesto(CancellationToken ct)
    {
        var source = CancellationTokenSource.CreateLinkedTokenSource(ct);
        source.CancelAfter(TimeSpan.FromMinutes(11));
        return source;
    }
    public async Task Acceso(string token, string perfil, string menu, CancellationToken ct)
    {
        using var tiempo = Presupuesto(ct);
        using var response = await Enviar("acceso", null, token, perfil, menu, tiempo.Token);
    }
    public async Task<IvaComprasCierreEstado> Estado(string token, string perfil, string menu, CancellationToken ct)
    {
        using var tiempo = Presupuesto(ct);
        using var response = await Enviar("estado", null, token, perfil, menu, tiempo.Token);
        var estado = JsonConvert.DeserializeObject<IvaComprasCierreEstado>(await response.Content.ReadAsStringAsync(tiempo.Token), Json)
            ?? throw new InvalidOperationException("La API no devolvió un historial válido.");
        estado.Validar();
        return estado;
    }
    public async Task<IvaComprasCierreRespuesta> Cerrar(IvaComprasCierrePedido pedido, string token, string perfil, string menu, CancellationToken ct)
    {
        pedido.Validar();
        using var tiempo = Presupuesto(ct);
        using var response = await Enviar("cerrar", pedido, token, perfil, menu, tiempo.Token);
        var respuesta = JsonConvert.DeserializeObject<IvaComprasCierreRespuesta>(await response.Content.ReadAsStringAsync(tiempo.Token), Json)
            ?? throw new InvalidOperationException("No se recibió el resultado del cierre.");
        if (string.IsNullOrWhiteSpace(respuesta.resultado_msj)) throw new InvalidOperationException("El cierre no devolvió un mensaje válido.");
        return respuesta;
    }

    public async Task<FileStream> Rg3685(IvaComprasCierreDescargaPedido pedido, string token, string perfil, string menu, CancellationToken ct)
    {
        pedido.Validar();
        using var tiempo = Presupuesto(ct);
        using var response = await Enviar("rg3685", pedido, token, perfil, menu, tiempo.Token);
        if (response.Content.Headers.ContentType?.MediaType != "application/zip")
            throw new InvalidOperationException("No se recibió un ZIP válido.");
        var file = new FileStream(Path.Combine(Path.GetTempPath(), "geco-iva-compras-sitio-" + Guid.NewGuid().ToString("N") + ".zip"),
            FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
        try
        {
            await response.Content.CopyToAsync(file, tiempo.Token);
            file.Position = 0;
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read, leaveOpen: true))
            {
                var prefix = "IVA_Compras_3685_" + IvaComprasCierrePeriodo.Texto(pedido.Periodo);
                var nombres = new[] { prefix + "_Cabecera.txt", prefix + "_Detalle.txt" };
                if (zip.Entries.Count != 2 || !zip.Entries.Select(e => e.FullName).Order().SequenceEqual(nombres.Order()))
                    throw new InvalidOperationException("El ZIP no contiene ambas salidas RG 3685 esperadas.");
            }
            file.Position = 0;
            return file;
        }
        catch { await file.DisposeAsync(); throw; }
    }

    private async Task<HttpResponseMessage> Enviar(string accion, IvaComprasCierrePedido? pedido, string token, string perfil, string menu, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(pedido == null ? HttpMethod.Get : HttpMethod.Post,
            IvaVentasCliente.Url(settings.Value.RutaBase, "api/iva-compras-cierre/" + accion));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Geco-Perfil", perfil); request.Headers.Add("X-Geco-Menu", menu);
        if (pedido != null) request.Content = new StringContent(JsonConvert.SerializeObject(pedido, Json), Encoding.UTF8, "application/json");
        var response = await Cliente.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (response.IsSuccessStatusCode) return response;
        var status = (int)response.StatusCode;
        string mensaje = status is 401 or 403 ? "La sesión venció o el perfil no tiene acceso a Cierre IVA Compras." :
            accion == "cerrar" ? "No se pudo confirmar el resultado del cierre. Actualice el historial antes de volver a intentarlo." : "No se pudo completar la consulta o descarga.";
        try
        {
            if (status is 400 or 403 or 409 or 422 or 429 or 503 or 504 && response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                var json = JObject.Parse(await response.Content.ReadAsStringAsync(ct));
                if (json["mensaje"]?.Type == JTokenType.String) mensaje = json["mensaje"]!.Value<string>()!;
            }
        }
        catch (JsonException) { }
        finally { response.Dispose(); }
        throw new IvaComprasCierreHttpException(status is 401 or 403 or 400 or 409 or 422 or 429 or 503 or 504 ? status : 503, mensaje);
    }
}

public class IvaComprasCierreHttpException(int estado, string mensaje) : InvalidOperationException(mensaje)
{ public int Estado { get; } = estado; }

