using gc.auth.remoto.Models;
using gc.infraestructura.Dtos.SolAuth.Comando;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace gc.auth.remoto.Services;

public sealed class AuthorizationApiClient : IAuthorizationApiClient
{
    private const string ApiRoute = "api/SolicitudesAutorizacion/";
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<HttpResponseMessage> GetPendingAsync(CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Get, $"{ApiRoute}pendientes", null, null, cancellationToken);

    public Task<HttpResponseMessage> GetHistoryAsync(DateTime? from, DateTime? to,
        CancellationToken cancellationToken)
    {
        var query = new List<string>();
        if (from.HasValue)
        {
            query.Add($"fechaDesde={Uri.EscapeDataString(from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
        }

        if (to.HasValue)
        {
            query.Add($"fechaHasta={Uri.EscapeDataString(to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))}");
        }

        var route = $"{ApiRoute}historico" + (query.Count > 0 ? $"?{string.Join('&', query)}" : string.Empty);
        return SendAsync(HttpMethod.Get, route, null, null, cancellationToken);
    }

    public Task<HttpResponseMessage> LockAsync(Guid requestId, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Post, $"{ApiRoute}{requestId:D}/bloqueo", null, null, cancellationToken);

    public Task<HttpResponseMessage> ResolveAsync(Guid requestId,
        ResolverSolicitudAutorizacionComando command, string idempotencyKey,
        CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Post, $"{ApiRoute}{requestId:D}/resolucion",
            JsonContent.Create(command), idempotencyKey, cancellationToken);

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string route, HttpContent? content,
        string? idempotencyKey, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No existe un contexto HTTP activo.");
        var token = context.Session.GetString(AuthenticationSession.JwtToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException("La sesión no contiene un token de autenticación.");
        }

        using var request = new HttpRequestMessage(method, route) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
        }

        return await _httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }
}
