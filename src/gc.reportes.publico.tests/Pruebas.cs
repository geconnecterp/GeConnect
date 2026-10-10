using System.Net;
using System.Text;
using System.Text.Json;
using Geco.Reportes.Publico.Configuration;
using Geco.Reportes.Publico.Models;
using Geco.Reportes.Publico.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Geco.Reportes.Publico.Tests;

internal static class Pruebas
{
    private const string Codigo = "ABcd2345";
    private static readonly ContextoDescarga Contexto = new("192.0.2.10", "Pruebas GECO", "prueba-local");
    private static int aprobadas;
    private static int fallidas;

    public static async Task<int> Main(string[] args)
    {
        string raiz = args.Length == 0 ? Path.GetFullPath("gc.reportes.publico") : Path.GetFullPath(args[0]);
        await Caso("URL pública independiente, subcarpeta y transición sin cambiar PathApp", () =>
        {
            Exigir(gc.sitio.Helpers.EnlacePublicoUrl.ResolverBase("https://docs.example.test/reportes/", "https://interno/gcsitio") == "https://docs.example.test/reportes");
            Exigir(gc.sitio.Helpers.EnlacePublicoUrl.ResolverBase("", "https://interno/gcsitio/") == "https://interno/gcsitio");
            foreach (var mala in new[] { "http://docs.test", "https://user:pass@docs.test", "https://docs.test/?q=1", "https://docs.test/#a", "/relativa" })
            {
                try { gc.sitio.Helpers.EnlacePublicoUrl.ResolverBase(mala, "https://interno"); throw new Exception("Se aceptó URL no segura"); }
                catch (ArgumentException) { }
                Exigir(!ReportesPublicosOptions.EsUrlHttps(mala));
            }
            return Task.CompletedTask;
        });
        await Caso("código inválido rechazado sin llamadas a API", async () =>
        {
            using var api = new ApiFalsa();
            foreach (var codigo in new[] { "", "../abcde", "ABcd2345?", "00000000", "ABCDEFGHijkl" })
                await Falla(() => Servicio(api).DescargarAsync(codigo, Contexto, default), 410);
            Exigir(api.Llamadas.Count == 0);
        });
        await Caso("sin destino público no se pueden crear enlaces al desactivar el legado", () =>
        {
            try { gc.sitio.Helpers.EnlacePublicoUrl.ResolverBase("", "https://interno", false); throw new Exception("Aceptó configuración inconsistente"); }
            catch (ArgumentException) { }
            return Task.CompletedTask;
        });
        await Caso("PDF y auditoría: resolver, generar y confirmar exactamente una vez", async () =>
        {
            using var api = new ApiFalsa();
            var pdf = await Servicio(api).DescargarAsync(Codigo, Contexto, default);
            Exigir(Encoding.ASCII.GetString(pdf.Contenido).StartsWith("%PDF-"));
            Exigir(pdf.Nombre == "Cuenta_prueba.pdf");
            Exigir(api.Llamadas.Count == 3);
            Exigir(api.Llamadas.All(c => c.Url.StartsWith("https://api.test/apireport/api/")));
            Exigir(api.Llamadas[0].Ip == Contexto.Ip && api.Llamadas[0].UserAgent == Contexto.UserAgent);
            using var dto = JsonDocument.Parse(api.Llamadas[1].Body!);
            Exigir(dto.RootElement.GetProperty("Cuenta").GetProperty("CampoFuturo").GetString() == "conservar");
            Exigir(api.Llamadas[2].Body!.Contains("\"accesoId\":17"));
        });
        foreach (var status in new[] { 400, 403, 404, 410 })
            await Caso($"resolución denegada {status}: mensaje público sin detalle interno", async () =>
            {
                using var api = new ApiFalsa { ResolverStatus = status };
                await Falla(() => Servicio(api).DescargarAsync(Codigo, Contexto, default), 410);
                Exigir(api.Llamadas.Count == 1);
            });
        await Caso("API no disponible no se confunde con enlace vencido", async () =>
        {
            using var api = new ApiFalsa { ResolverStatus = 500 };
            await Falla(() => Servicio(api).DescargarAsync(Codigo, Contexto, default), 503);
        });
        await Caso("contrato antiguo bloqueado por defecto", async () =>
        {
            using var api = new ApiFalsa { Antiguo = true };
            await Falla(() => Servicio(api).DescargarAsync(Codigo, Contexto, default), 503);
            Exigir(api.Llamadas.Count == 1);
        });
        await Caso("compatibilidad antigua solo por configuración explícita", async () =>
        {
            using var api = new ApiFalsa { Antiguo = true };
            await Servicio(api, new() { RequiereControlDescargas = false }).DescargarAsync(Codigo, Contexto, default);
            Exigir(api.Llamadas.Count == 2);
        });
        await Caso("sin accesoId válido no se genera PDF", async () =>
        {
            using var api = new ApiFalsa { AccesoId = 0 };
            await Falla(() => Servicio(api).DescargarAsync(Codigo, Contexto, default), 503);
            Exigir(api.Llamadas.Count == 1);
        });
        foreach (var contenido in new[] { "invalido***", Convert.ToBase64String("<html>Error</html>"u8.ToArray()) })
            await Caso("contenido no PDF: registra fallo y nunca confirma", async () =>
            {
                using var api = new ApiFalsa { Base64 = contenido };
                await Falla(() => Servicio(api).DescargarAsync(Codigo, Contexto, default), 503);
                Exigir(api.Llamadas.Last().Url.EndsWith("RegistrarFallo"));
                Exigir(!api.Llamadas.Any(c => c.Url.EndsWith("ConfirmarDescarga")));
            });
        await Caso("límite de tamaño antes de entregar el PDF", async () =>
        {
            using var api = new ApiFalsa { Base64 = Convert.ToBase64String(new byte[2 * 1024 * 1024]) };
            await Falla(() => Servicio(api, new() { MaxPdfMb = 1 }).DescargarAsync(Codigo, Contexto, default), 503);
            Exigir(api.Llamadas.Last().Url.EndsWith("RegistrarFallo"));
        });
        await Caso("respuesta de negocio fallida al generar registra fallo", async () =>
        {
            using var api = new ApiFalsa { Resultado = -1 };
            await Falla(() => Servicio(api).DescargarAsync(Codigo, Contexto, default), 503);
            Exigir(api.Llamadas.Last().Url.EndsWith("RegistrarFallo"));
        });
        foreach (var status in new[] { 500, 200 })
            await Caso($"confirmación rechazada {status}: sin PDF, reintentos ni fallo compensatorio", async () =>
            {
                using var api = new ApiFalsa { ConfirmarStatus = status, EstadoConfirmacion = 5 };
                await Falla(() => Servicio(api).DescargarAsync(Codigo, Contexto, default), 503);
                Exigir(api.Llamadas.Count == 3 && api.Llamadas.Last().Url.EndsWith("ConfirmarDescarga"));
            });
        await Caso("logs sin código, payload ni excepción técnica", async () =>
        {
            using var api = new ApiFalsa { ErrorGenerar = true };
            var log = new LogCapturado<ReportePublicoService>();
            await Falla(() => Servicio(api, log: log).DescargarAsync(Codigo, Contexto, default), 503);
            var text = string.Join("\n", log.Lineas);
            Exigir(!text.Contains(Codigo) && !text.Contains("SECRETO_SIMULADO"));
        });
        await Caso("cancelación no entrega PDF", async () =>
        {
            using var api = new ApiFalsa();
            using var cts = new CancellationTokenSource(); cts.Cancel();
            try { await Servicio(api).DescargarAsync(Codigo, Contexto, cts.Token); throw new Exception("No canceló"); }
            catch (OperationCanceledException) { }
        });
        await Caso("nombre de archivo sin rutas, controles ni caracteres de encabezado", () =>
        {
            var name = ReportePublicoService.NombreSeguro("../Cuenta\r\n\"/prueba");
            Exigir(!name.Contains('/') && !name.Contains('\r') && !name.Contains('"') && name.EndsWith(".pdf"));
            return Task.CompletedTask;
        });

        await Caso("MVC real: PDF, vistas Razor, métodos, headers y superficie mínima", async () =>
        {
            using var api = new ApiFalsa();
            await using var portal = await PortalPrueba.Iniciar(raiz, api);
            var home = await portal.Client.GetAsync("/");
            Exigir(home.StatusCode == HttpStatusCode.OK && (await home.Content.ReadAsStringAsync()).Contains("PORTAL DE DOCUMENTOS"));
            Exigir(home.Headers.CacheControl!.NoStore && home.Headers.GetValues("Referrer-Policy").Single() == "no-referrer");
            var pdf = await portal.Client.GetAsync("/d/" + Codigo);
            Exigir(pdf.StatusCode == HttpStatusCode.OK && pdf.Content.Headers.ContentType!.MediaType == "application/pdf");
            Exigir(pdf.Content.Headers.ContentDisposition!.DispositionType == "attachment");
            Exigir(!pdf.Headers.Contains("Access-Control-Allow-Origin"));
            int antes = api.Llamadas.Count;
            foreach (var route in new[] { "/appsettings.json", "/api/Link/CrearLink", "/api/Reportes/generate", "/seguridad/Token/Login", "/Documentos/Descargar" })
                Exigir((await portal.Client.GetAsync(route)).StatusCode == HttpStatusCode.NotFound);
            Exigir((await portal.Client.PostAsync("/d/" + Codigo, new StringContent("{}"))).StatusCode == HttpStatusCode.MethodNotAllowed);
            Exigir((await portal.Client.GetAsync("/d/" + Codigo + "?reporte=99")).StatusCode == HttpStatusCode.BadRequest);
            using var head = new HttpRequestMessage(HttpMethod.Head, "/d/" + Codigo);
            Exigir((await portal.Client.SendAsync(head)).StatusCode == HttpStatusCode.MethodNotAllowed);
            using var prefetch = new HttpRequestMessage(HttpMethod.Get, "/d/" + Codigo);
            prefetch.Headers.Add("Sec-Purpose", "prefetch");
            Exigir((await portal.Client.SendAsync(prefetch)).StatusCode == HttpStatusCode.NoContent);
            Exigir(api.Llamadas.Count == antes);
            var invalido = await portal.Client.GetAsync("/d/INVALIDO");
            Exigir(invalido.StatusCode == HttpStatusCode.Gone);
            Exigir((await invalido.Content.ReadAsStringAsync()).Contains("Referencia de atención"));
        });
        await Caso("cabeceras X-Geco del visitante no suplantan el contexto", async () =>
        {
            using var api = new ApiFalsa();
            await using var portal = await PortalPrueba.Iniciar(raiz, api);
            using var request = new HttpRequestMessage(HttpMethod.Get, "/d/" + Codigo);
            request.Headers.Add("X-Geco-Client-IP", "203.0.113.99");
            request.Headers.Add("X-Geco-User-Agent", "suplantado");
            request.Headers.UserAgent.ParseAdd("Prueba/1.0");
            Exigir((await portal.Client.SendAsync(request)).StatusCode == HttpStatusCode.OK);
            Exigir(api.Llamadas[0].Ip != "203.0.113.99" && api.Llamadas[0].UserAgent == "Prueba/1.0");
        });
        await Caso("rate limit devuelve 429 sin alcanzar la API", async () =>
        {
            using var api = new ApiFalsa();
            await using var portal = await PortalPrueba.Iniciar(raiz, api, "--ReportesPublicos:SolicitudesPorMinutoPorIp=2");
            await portal.Client.GetAsync("/"); await portal.Client.GetAsync("/");
            Exigir((await portal.Client.GetAsync("/d/" + Codigo)).StatusCode == HttpStatusCode.TooManyRequests);
            Exigir(api.Llamadas.Count == 0);
        });
        await Caso("concurrencia limitada sin iniciar otra resolución", async () =>
        {
            using var api = new ApiFalsa { Bloquear = true };
            await using var portal = await PortalPrueba.Iniciar(raiz, api, "--ReportesPublicos:MaxConcurrentes=1");
            var primera = portal.Client.GetAsync("/d/" + Codigo);
            try
            {
                await api.Entrada.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Exigir((await portal.Client.GetAsync("/d/" + Codigo)).StatusCode == HttpStatusCode.TooManyRequests);
            }
            finally { api.Liberar.TrySetResult(); }
            Exigir((await primera).StatusCode == HttpStatusCode.OK && api.Llamadas.Count == 3);
        });
        await Caso("producción no acepta HTTP ni inicia backend", async () =>
        {
            using var api = new ApiFalsa();
            await using var portal = await PortalPrueba.Iniciar(raiz, api, "--environment=Production");
            Exigir((await portal.Client.GetAsync("/d/" + Codigo)).StatusCode == HttpStatusCode.BadRequest);
            Exigir(api.Llamadas.Count == 0);
        });
        await Caso("configuración incompleta impide iniciar", async () =>
        {
            using var api = new ApiFalsa();
            try
            {
                await using var portal = await PortalPrueba.Iniciar(raiz, api, "--ReportesPublicos:ApiBaseUrl=");
                throw new Exception("Arrancó sin API HTTPS");
            }
            catch (OptionsValidationException) { }
        });
        Console.WriteLine($"RESULTADO: {aprobadas} aprobadas, {fallidas} fallidas. Sin conexiones a API ni base de datos reales.");
        return fallidas == 0 ? 0 : 1;
    }

    private static ReportePublicoService Servicio(ApiFalsa api, ReportesPublicosOptions? config = null, ILogger<ReportePublicoService>? log = null) =>
        new(new HttpClient(api, false) { BaseAddress = new Uri("https://api.test/apireport/") },
            Options.Create(config ?? new()), log ?? new LogCapturado<ReportePublicoService>());
    private static void Exigir(bool ok) { if (!ok) throw new Exception("Condición no satisfecha"); }
    private static async Task Falla(Func<Task<DocumentoPdf>> action, int status)
    {
        try { await action(); throw new Exception("La operación debía rechazarse"); }
        catch (DescargaException ex) { Exigir(ex.Status == status && !ex.Message.Contains("SECRETO_SIMULADO")); }
    }
    private static async Task Caso(string nombre, Func<Task> action)
    {
        try { await action(); aprobadas++; Console.WriteLine("PASS " + nombre); }
        catch (Exception ex) { fallidas++; Console.WriteLine("FAIL " + nombre + " | " + ex.GetType().Name + ": " + ex.Message); }
    }
}

internal sealed record Llamada(string Url, string? Body, string? Ip, string? UserAgent);
internal sealed class ApiFalsa : HttpMessageHandler
{
    public List<Llamada> Llamadas { get; } = [];
    public int ResolverStatus = 200, ConfirmarStatus = 200, EstadoConfirmacion, Resultado;
    public long AccesoId = 17;
    public bool Antiguo, ErrorGenerar, Bloquear;
    public TaskCompletionSource Entrada = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Liberar = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public string Base64 = Convert.ToBase64String("%PDF-1.4\n% fixture de transporte\n%%EOF"u8.ToArray());
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var url = request.RequestUri!.AbsoluteUri;
        var body = request.Content == null ? null : await request.Content.ReadAsStringAsync(ct);
        Llamadas.Add(new(url, body, request.Headers.TryGetValues("X-Geco-Client-IP", out var ips) ? ips.First() : null,
            request.Headers.TryGetValues("X-Geco-User-Agent", out var agents) ? agents.First() : null));
        if (url.Contains("ObtenerSolicitud"))
        {
            if (Bloquear) { Entrada.TrySetResult(); await Liberar.Task.WaitAsync(ct); }
            var solicitud = new { Reporte = 1, Titulo = "Cuenta prueba", Cuenta = new { CampoFuturo = "conservar" }, Parametros = new { Cuenta = "PRUEBA" } };
            return Respuesta(ResolverStatus, Antiguo ? (object)new { Data = solicitud } : new { Data = new { Solicitud = solicitud, AccesoId } });
        }
        if (url.EndsWith("generate"))
        {
            if (ErrorGenerar) throw new HttpRequestException("SECRETO_SIMULADO /d/ABcd2345");
            return Respuesta(200, new { data = new { resultado = Resultado, base64 = Base64 } });
        }
        if (url.EndsWith("ConfirmarDescarga")) return Respuesta(ConfirmarStatus, new { data = new { estado = EstadoConfirmacion } });
        if (url.EndsWith("RegistrarFallo")) return Respuesta(200, new { success = true });
        throw new Exception("Ruta de API no permitida en prueba");
    }
    private static HttpResponseMessage Respuesta(int status, object value) => new((HttpStatusCode)status)
    { Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json") };
}

internal sealed class LogCapturado<T> : ILogger<T>
{
    public List<string> Lineas { get; } = [];
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel level) => true;
    public void Log<TState>(LogLevel level, EventId id, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Lineas.Add(formatter(state, exception));
}

internal sealed class PortalPrueba(WebApplication app, HttpClient client) : IAsyncDisposable
{
    public HttpClient Client { get; } = client;
    public static async Task<PortalPrueba> Iniciar(string raiz, ApiFalsa api, params string[] extra)
    {
        string[] args = ["--environment=Development", "--contentRoot=" + raiz,
            "--ReportesPublicos:ApiBaseUrl=https://api.test/apireport/", "--AllowedHosts=127.0.0.1", .. extra];
        var app = global::Program.CrearAplicacion(args, builder =>
        {
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Logging.ClearProviders();
            builder.Services.AddHttpClient<IReportePublicoService, ReportePublicoService>()
                .ConfigurePrimaryHttpMessageHandler(() => api);
        });
        try { await app.StartAsync(); }
        catch { await app.DisposeAsync(); throw; }
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        return new(app, new HttpClient { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(15) });
    }
    public async ValueTask DisposeAsync() { Client.Dispose(); await app.StopAsync(); await app.DisposeAsync(); }
}
