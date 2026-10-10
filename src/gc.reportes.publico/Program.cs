using System.Net;
using System.Threading.RateLimiting;
using Geco.Reportes.Publico.Configuration;
using Geco.Reportes.Publico.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

var app = Program.CrearAplicacion(args);
app.Run();

public partial class Program
{
    public static WebApplication CrearAplicacion(string[] args, Action<WebApplicationBuilder>? configurarHost = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(Program).Assembly.GetName().Name
        });
        builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);
        builder.Services.AddControllersWithViews();
        builder.Services.AddOptions<ReportesPublicosOptions>()
            .Bind(builder.Configuration.GetSection(ReportesPublicosOptions.Section))
            .ValidateDataAnnotations()
            .Validate(o => ReportesPublicosOptions.EsUrlHttps(o.ApiBaseUrl), "Configurar ReportesPublicos:ApiBaseUrl con una URL HTTPS sin credenciales, query ni fragmento.")
            .Validate(o => o.ProxiesValidos(), "ProxiesConfiables debe contener únicamente direcciones IP explícitas.")
            .ValidateOnStart();
        builder.Services.AddHttpClient<IReportePublicoService, ReportePublicoService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<ReportesPublicosOptions>>().Value;
                client.BaseAddress = new Uri(options.ApiBaseUrl.TrimEnd('/') + "/");
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false,
                UseProxy = false,
                ConnectTimeout = TimeSpan.FromSeconds(10),
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            })
            .RemoveAllLoggers(); // Los logs HTTP predeterminados incluyen el código secreto de la URL.

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            // Conservar la confianza local de IIS y agregar solo proxies explícitos, nunca confiar en todos.
            foreach (var address in builder.Configuration.GetSection("ReportesPublicos:ProxiesConfiables").Get<string[]>() ?? [])
                if (IPAddress.TryParse(address, out var ip)) options.KnownProxies.Add(ip);
        });
        builder.Services.AddRateLimiter(options =>
        {
            var config = builder.Configuration.GetSection(ReportesPublicosOptions.Section).Get<ReportesPublicosOptions>() ?? new();
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                PartitionedRateLimiter.Create<HttpContext, string>(_ => RateLimitPartition.GetConcurrencyLimiter("global",
                    _ => new ConcurrencyLimiterOptions { PermitLimit = Math.Clamp(config.MaxConcurrentes, 1, 16), QueueLimit = 0 })),
                PartitionedRateLimiter.Create<HttpContext, string>(_ => RateLimitPartition.GetFixedWindowLimiter("global",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = Math.Clamp(config.SolicitudesPorMinutoGlobal, 1, 1000), Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })),
                PartitionedRateLimiter.Create<HttpContext, string>(ctx => RateLimitPartition.GetFixedWindowLimiter(
                    ctx.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = Math.Clamp(config.SolicitudesPorMinutoPorIp, 1, 300), Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })));
            options.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                context.HttpContext.Response.Headers.RetryAfter = "60";
                await context.HttpContext.Response.WriteAsync("Se alcanzó el límite de solicitudes. Esperá un minuto antes de volver a intentar.", ct);
            };
        });

        configurarHost?.Invoke(builder);
        var app = builder.Build();
        app.UseForwardedHeaders();
        app.Use(async (context, next) =>
        {
            context.Response.Headers.CacheControl = "no-store, private";
            context.Response.Headers.Pragma = "no-cache";
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow, noarchive";
            context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; style-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            if (!app.Environment.IsDevelopment() && !context.Request.IsHttps)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Utilizá el enlace HTTPS original.");
                return;
            }
            if (context.Request.IsHttps) context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000";
            try { await next(context); }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { context.Abort(); }
            catch (Exception ex)
            {
                // No registrar ex/Message: pueden incluir payload, URL o código de descarga.
                app.Logger.LogError("Error del portal. Referencia {Referencia}, categoría {Categoria}", context.TraceIdentifier, ex.GetType().Name);
                if (context.Response.HasStarted) { context.Abort(); return; }
                context.Response.StatusCode = 503;
                await context.Response.WriteAsync("El documento no está disponible en este momento.");
            }
        });
        app.UseStaticFiles();
        app.UseRouting();
        app.UseRateLimiter();
        app.MapControllers(); // Sin rutas convencionales, swagger, SQL, sesión ni generación pública.
        return app;
    }
}
