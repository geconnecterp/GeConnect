using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Administracion;
using System.Net.Http.Json;
using System.Text.Json;

namespace gc.auth.remoto.Services;

public sealed class AuthenticationApiClient : IAuthenticationApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationApiClient> _logger;

    public AuthenticationApiClient(HttpClient httpClient, ILogger<AuthenticationApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AdministracionLoginDto>> GetAdministrationsAsync(
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            "api/administracion/GetAdministraciones4Login", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("La API devolvió {StatusCode} al consultar administraciones.",
                (int)response.StatusCode);
            throw new InvalidOperationException("No se pudieron obtener las administraciones disponibles.");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<AdministracionLoginDto>>>(
            JsonOptions, cancellationToken);

        return result?.Data ?? [];
    }

    public async Task<string> AuthenticateAsync(string userName, string password,
        string administrationId, string? clientIp, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/apitoken")
        {
            Content = JsonContent.Create(new
            {
                UserName = userName,
                Password = password,
                Admid = administrationId
            })
        };

        if (!string.IsNullOrWhiteSpace(clientIp))
        {
            request.Headers.TryAddWithoutValidation("X-ClientUsr", clientIp);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("La API rechazó un intento de autenticación con estado {StatusCode}.",
                (int)response.StatusCode);
            throw new UnauthorizedAccessException("El usuario, la contraseña o la administración no son correctos.");
        }

        var result = await response.Content.ReadFromJsonAsync<AutenticacionDto>(JsonOptions, cancellationToken);
        if (string.IsNullOrWhiteSpace(result?.Token))
        {
            throw new InvalidOperationException("La API no devolvió un token de autenticación válido.");
        }

        return result.Token;
    }
}
