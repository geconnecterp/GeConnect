using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Geco.Reportes.Publico.Configuration;
using Geco.Reportes.Publico.Models;
using Microsoft.Extensions.Options;

namespace Geco.Reportes.Publico.Services;

public sealed class ReportePublicoService(
    HttpClient client, IOptions<ReportesPublicosOptions> options,
    ILogger<ReportePublicoService> logger) : IReportePublicoService
{
    // Contrato HTTP independiente: no referencias a GECO, SQL o motores de PDF.
    private readonly ReportesPublicosOptions settings = options.Value;
    private const string Alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
    public static bool CodigoValido(string? codigo) => codigo is { Length: 8 }
        && codigo.All(c => Alfabeto.Contains(c));

    public async Task<DocumentoPdf> DescargarAsync(string codigo, ContextoDescarga contexto, CancellationToken cancellationToken)
    {
        if (!CodigoValido(codigo)) throw DescargaException.Enlace();
        var reloj = Stopwatch.StartNew();
        long accesoId = 0;
        bool confirmacionIniciada = false;
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limite.CancelAfter(TimeSpan.FromSeconds(settings.TimeoutSegundos));
        var ct = limite.Token;
        try
        {
            using var obtener = new HttpRequestMessage(HttpMethod.Get,
                $"api/Link/ObtenerSolicitud?codigo={Uri.EscapeDataString(codigo)}");
            AgregarHeader(obtener, "X-Geco-Client-IP", contexto.Ip, 45);
            AgregarHeader(obtener, "X-Geco-User-Agent", contexto.UserAgent, 500);
            var data = await EnviarAsync(obtener, 2 * 1024 * 1024, true, ct);
            JsonElement solicitud;
            if (Propiedad(data, "solicitud", out solicitud))
            {
                if (!Propiedad(data, "accesoId", out var id) || !id.TryGetInt64(out accesoId) || accesoId <= 0)
                    throw DescargaException.Servicio();
            }
            else
            {
                // Compatibilidad por decisión explícita; nunca degradar auditoría silenciosamente.
                if (settings.RequiereControlDescargas) throw DescargaException.Servicio();
                solicitud = data;
            }
            if (solicitud.ValueKind != JsonValueKind.Object || !Propiedad(solicitud, "reporte", out _))
                throw DescargaException.Servicio();

            using var generar = new HttpRequestMessage(HttpMethod.Post, "api/Reportes/generate")
            {
                // Solo se reenvía el objeto autorizado por la API, nunca parámetros del navegador.
                Content = new StringContent(solicitud.GetRawText(), Encoding.UTF8, "application/json")
            };
            long maxPdf = settings.MaxPdfMb * 1024L * 1024L;
            var pdfData = await EnviarAsync(generar, maxPdf * 4 / 3 + 1024 * 1024, false, ct);
            if (!Propiedad(pdfData, "resultado", out var resultado) || !resultado.TryGetInt32(out var valor) || valor != 0
                || !Propiedad(pdfData, "base64", out var base64) || base64.ValueKind != JsonValueKind.String)
                throw DescargaException.Servicio();
            var texto = base64.GetString()!;
            if (texto.Length > 4 * ((maxPdf + 2) / 3)) throw DescargaException.Servicio();
            var bytes = Convert.FromBase64String(texto);
            if (bytes.LongLength > maxPdf || bytes.Length < 5 || !bytes.AsSpan(0, 5).SequenceEqual("%PDF-"u8))
                throw DescargaException.Servicio();
            ct.ThrowIfCancellationRequested();

            if (accesoId > 0)
            {
                // Si la respuesta se pierde, no sabemos si la API ya contabilizó la descarga.
                confirmacionIniciada = true;
                using var confirmar = new HttpRequestMessage(HttpMethod.Post, "api/Link/ConfirmarDescarga")
                {
                    Content = JsonContent.Create(new { Codigo = codigo, AccesoId = accesoId,
                        Bytes = bytes.LongLength, DuracionMs = Duracion(reloj), ResultadoHttp = 200 })
                };
                var confirmada = await EnviarAsync(confirmar, 64 * 1024, false, ct);
                if (!Propiedad(confirmada, "estado", out var estado) || !estado.TryGetInt32(out var e) || e != 0)
                    throw DescargaException.Servicio();
            }
            logger.LogInformation("PDF preparado. Referencia {Referencia}, acceso {AccesoId}, bytes {Bytes}, duración {DuracionMs}",
                contexto.Correlacion, accesoId, bytes.Length, Duracion(reloj));
            // Contabiliza preparación, como GECO; no prueba recepción física por el cliente.
            var titulo = Propiedad(solicitud, "titulo", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;
            return new DocumentoPdf(bytes, NombreSeguro(titulo));
        }
        catch (Exception ex)
        {
            logger.LogWarning("Descarga no completada. Referencia {Referencia}, acceso {AccesoId}, categoría {Categoria}, confirmación iniciada {Confirmacion}",
                contexto.Correlacion, accesoId, ex.GetType().Name, confirmacionIniciada);
            // No registrar fallo tras una confirmación ambigua: podría alterar una operación confirmada.
            if (accesoId > 0 && !confirmacionIniciada)
                await RegistrarFalloAsync(codigo, accesoId, reloj, contexto.Correlacion);
            if (ex is OperationCanceledException && cancellationToken.IsCancellationRequested) throw;
            if (ex is DescargaException) throw;
            throw DescargaException.Servicio();
        }
    }

    private async Task RegistrarFalloAsync(string codigo, long accesoId, Stopwatch reloj, string correlacion)
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var fallo = new HttpRequestMessage(HttpMethod.Post, "api/Link/RegistrarFallo")
            {
                Content = JsonContent.Create(new { Codigo = codigo, AccesoId = accesoId,
                    DuracionMs = Duracion(reloj), ResultadoHttp = 503, Detalle = "No se pudo preparar el PDF desde el portal público." })
            };
            using var response = await client.SendAsync(fallo, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (!response.IsSuccessStatusCode) throw DescargaException.Servicio();
        }
        catch (Exception)
        {
            logger.LogWarning("No se pudo registrar el fallo. Referencia {Referencia}, acceso {AccesoId}", correlacion, accesoId);
        }
    }

    private async Task<JsonElement> EnviarAsync(HttpRequestMessage request, long maxBytes, bool resolucion, CancellationToken ct)
    {
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode)
        {
            if (resolucion && response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound
                or HttpStatusCode.Gone or HttpStatusCode.Forbidden) throw DescargaException.Enlace();
            throw DescargaException.Servicio();
        }
        if (response.Content.Headers.ContentLength > maxBytes) throw DescargaException.Servicio();
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var buffer = new MemoryStream();
        var chunk = new byte[16384];
        int n;
        while ((n = await stream.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + n > maxBytes) throw DescargaException.Servicio();
            buffer.Write(chunk, 0, n);
        }
        using var json = JsonDocument.Parse(buffer.GetBuffer().AsMemory(0, (int)buffer.Length),
            new JsonDocumentOptions { MaxDepth = 64 });
        if (!Propiedad(json.RootElement, "data", out var data) || data.ValueKind != JsonValueKind.Object)
            throw DescargaException.Servicio();
        return data.Clone();
    }

    private static bool Propiedad(JsonElement obj, string key, out JsonElement value)
    {
        if (obj.ValueKind == JsonValueKind.Object)
            foreach (var p in obj.EnumerateObject())
                if (string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase)) { value = p.Value; return true; }
        value = default;
        return false;
    }

    private static void AgregarHeader(HttpRequestMessage request, string name, string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        var clean = new string(value.Where(c => !char.IsControl(c)).Take(max).ToArray());
        request.Headers.TryAddWithoutValidation(name, clean);
    }

    public static string NombreSeguro(string? titulo)
    {
        var safe = new string((titulo ?? "").Where(c => char.IsLetterOrDigit(c) || c is '-' or '_' or ' ').Take(80).ToArray()).Trim();
        return (safe.Length == 0 ? "documento" : safe.Replace(' ', '_')) + ".pdf";
    }
    private static int Duracion(Stopwatch reloj) => (int)Math.Min(int.MaxValue, reloj.ElapsedMilliseconds);
}
