using gc.auth.remoto.Models;
using gc.auth.remoto.Services;
using gc.infraestructura.Dtos.SolAuth.Comando;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gc.auth.remoto.Controllers;

[Authorize]
[ApiController]
[AutoValidateAntiforgeryToken]
[Route("visor-api/solicitudes")]
public sealed class AuthorizationRequestsController : ControllerBase
{
    private readonly IAuthorizationApiClient _authorizationApi;
    private readonly ILogger<AuthorizationRequestsController> _logger;

    public AuthorizationRequestsController(IAuthorizationApiClient authorizationApi,
        ILogger<AuthorizationRequestsController> logger)
    {
        _authorizationApi = authorizationApi;
        _logger = logger;
    }

    [HttpGet("pendientes")]
    public Task<IActionResult> GetPending(CancellationToken cancellationToken) =>
        ForwardAsync(() => _authorizationApi.GetPendingAsync(cancellationToken), cancellationToken);

    [HttpGet("historico")]
    public Task<IActionResult> GetHistory([FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta, CancellationToken cancellationToken) =>
        ForwardAsync(() => _authorizationApi.GetHistoryAsync(fechaDesde, fechaHasta, cancellationToken),
            cancellationToken);

    [HttpPost("{requestId:guid}/bloqueo")]
    public Task<IActionResult> Lock(Guid requestId, CancellationToken cancellationToken) =>
        ForwardAsync(() => _authorizationApi.LockAsync(requestId, cancellationToken), cancellationToken);

    [HttpPost("{requestId:guid}/resolucion")]
    public Task<IActionResult> Resolve(Guid requestId,
        [FromBody] ResolverSolicitudAutorizacionComando command,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
        {
            return Task.FromResult<IActionResult>(BadRequest(new
            {
                message = "El header Idempotency-Key es requerido y admite hasta 100 caracteres."
            }));
        }

        return ForwardAsync(
            () => _authorizationApi.ResolveAsync(requestId, command, idempotencyKey, cancellationToken),
            cancellationToken);
    }

    private async Task<IActionResult> ForwardAsync(Func<Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await operation();
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            var statusCode = (int)response.StatusCode;
            if (response.Content.Headers.ContentLength is 0)
            {
                return StatusCode(statusCode);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
            return new ContentResult
            {
                Content = content,
                ContentType = contentType,
                StatusCode = statusCode
            };
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogWarning(exception, "La sesión no dispone de credenciales para consultar autorizaciones.");
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Unauthorized(new { message = "La sesión finalizó. Debe autenticarse nuevamente." });
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "No fue posible comunicarse con gc.api.");
            return Problem(
                title: "No fue posible comunicarse con el servicio de autorizaciones.",
                statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
