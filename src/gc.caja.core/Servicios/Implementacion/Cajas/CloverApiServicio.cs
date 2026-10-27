using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gc.caja.core.Servicios.Implementacion.Cajas;

public class CloverApiServicio
{
    private readonly HttpClient cliente;
    private readonly string ruta;
    private readonly ILogger<CloverApiServicio> logger;
    public CloverApiServicio(HttpClient cliente, IOptions<AppSettings> options, ILogger<CloverApiServicio> logger) { this.cliente = cliente; this.logger = logger; ruta = (options.Value.RutaBase ?? throw new InvalidOperationException("Falta AppSettings:RutaBase.")).TrimEnd('/') + "/api/ApiPagoFactura/"; }
    protected virtual async Task<T> Enviar<T>(string accion, object dato, string token) where T : class
    {
        var llamada = Guid.NewGuid().ToString("N");
        var reloj = Stopwatch.StartNew();
        using var req = new HttpRequestMessage(HttpMethod.Post, ruta + accion);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        // Un único documento JSON en el cuerpo HTTP; nunca serializar de nuevo este string.
        var cuerpo = JsonConvert.SerializeObject(dato);
        req.Content = new StringContent(cuerpo, Encoding.UTF8, "application/json");
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("[CLOVER][REQUEST] {Accion} | Llamada={Llamada} | POST {Url}\n{Json}",
                accion, llamada, req.RequestUri, CloverJsonDiagnostico.Formatear(cuerpo, token));
        try
        {
            using var respuesta = await cliente.SendAsync(req);
            var texto = await respuesta.Content.ReadAsStringAsync();
            var nivel = respuesta.IsSuccessStatusCode ? LogLevel.Information : LogLevel.Warning;
            if (logger.IsEnabled(nivel))
                logger.Log(nivel, "[CLOVER][RESPONSE] {Accion} | Llamada={Llamada} | HTTP {Estado} | {DuracionMs} ms\n{Json}",
                    accion, llamada, (int)respuesta.StatusCode, reloj.ElapsedMilliseconds, CloverJsonDiagnostico.Formatear(texto, token));
            if (!respuesta.IsSuccessStatusCode) {
                string mensaje;
                try { var json = JObject.Parse(texto); mensaje = (string?)(json["message"] ?? json["Message"] ?? json["mensaje"] ?? json["detail"] ?? json["error"]?[0]?["Detail"] ?? json["error"]?[0]?["detail"]) ?? ""; } catch { mensaje = ""; }
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(mensaje) ? "No se pudo completar la comunicación Clover con la API. Consulte el estado antes de repetir." : mensaje);
            }
            return JsonConvert.DeserializeObject<ApiResponse<T>>(texto)?.Data ?? throw new InvalidOperationException("La API no devolvió datos Clover válidos.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning("[CLOVER][SIN_RESPONSE] {Accion} | Llamada={Llamada} | {DuracionMs} ms | Tipo={Tipo}",
                accion, llamada, reloj.ElapsedMilliseconds, ex.GetType().Name);
            throw; // Conservar el tratamiento de incertidumbre existente; no reintentar el pago.
        }
    }

    public virtual Task<List<CloverOpcionDto>> Opciones(CloverPeticionDto req, string token) => Enviar<List<CloverOpcionDto>>("CloverOpciones", req, token);
    public virtual Task<RespuestaDto> Procesar(CloverPeticionDto req, string token) => Enviar<RespuestaDto>("CloverProcesar", req, token);
}
