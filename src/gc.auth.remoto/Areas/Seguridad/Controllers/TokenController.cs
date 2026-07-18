using gc.auth.remoto.Models;
using gc.auth.remoto.Services;
using gc.infraestructura.Dtos.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace gc.auth.remoto.Areas.Seguridad.Controllers;

[Area("Seguridad")]
public sealed class TokenController : Controller
{
    private readonly IAuthenticationApiClient _authenticationApi;
    private readonly ILogger<TokenController> _logger;

    public TokenController(IAuthenticationApiClient authenticationApi, ILogger<TokenController> logger)
    {
        _authenticationApi = authenticationApi;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl, CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true && HasActiveSessionToken())
        {
            return RedirectToLocal(returnUrl);
        }

        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var model = new LoginViewModel { Fecha = DateTime.Now };
        await LoadAdministrationsAsync(model, cancellationToken);
        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl,
        CancellationToken cancellationToken)
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!ModelState.IsValid)
        {
            await LoadAdministrationsAsync(model, cancellationToken);
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        try
        {
            var token = await _authenticationApi.AuthenticateAsync(
                model.UserName.Trim(), model.Password, model.Admid,
                HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            if (jwt.ValidTo <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("El token recibido ya se encuentra vencido.");
            }

            var userId = RequiredClaim(jwt, "user");
            var administration = RequiredClaim(jwt, "AdmId");
            var profilesJson = RequiredClaim(jwt, "perfiles");
            var profiles = JsonSerializer.Deserialize<List<PerfilUserDto>>(profilesJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

            if (profiles.Count == 0)
            {
                throw new UnauthorizedAccessException(
                    "El usuario no tiene perfiles habilitados para operar en el sistema.");
            }

            var selectedProfile = profiles.FirstOrDefault(profile =>
                string.Equals(profile.perfil_default, "S", StringComparison.OrdinalIgnoreCase))
                ?? profiles[0];

            var claims = jwt.Claims.ToList();
            if (!claims.Any(claim => claim.Type == ClaimTypes.Name))
            {
                claims.Add(new Claim(ClaimTypes.Name, userId));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authenticationProperties = new AuthenticationProperties
            {
                AllowRefresh = false,
                ExpiresUtc = new DateTimeOffset(jwt.ValidTo),
                IsPersistent = false
            };

            HttpContext.Session.SetString(AuthenticationSession.JwtToken, token);
            HttpContext.Session.SetString(AuthenticationSession.Administration, administration);
            HttpContext.Session.SetString(AuthenticationSession.UserProfiles,
                JsonSerializer.Serialize(profiles));
            HttpContext.Session.SetString(AuthenticationSession.SelectedProfile,
                JsonSerializer.Serialize(selectedProfile));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, principal, authenticationProperties);

            return RedirectToLocal(returnUrl);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException
                                          or InvalidOperationException
                                          or HttpRequestException
                                          or JsonException
                                          or ArgumentException)
        {
            HttpContext.Session.Clear();
            _logger.LogWarning(exception, "No se pudo autenticar al usuario {UserName}.", model.UserName);
            ModelState.AddModelError(string.Empty, exception.Message);
            model.Password = string.Empty;
            await LoadAdministrationsAsync(model, cancellationToken);
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private async Task LoadAdministrationsAsync(LoginViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            model.Administraciones = await _authenticationApi.GetAdministrationsAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is InvalidOperationException or HttpRequestException)
        {
            _logger.LogError(exception, "No se pudo cargar el catálogo de administraciones.");
            model.Administraciones = [];
            ModelState.AddModelError(string.Empty, exception.Message);
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home", new { area = string.Empty });
    }

    private bool HasActiveSessionToken()
    {
        var token = HttpContext.Session.GetString(AuthenticationSession.JwtToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    private static string RequiredClaim(JwtSecurityToken token, string claimType)
    {
        return token.Claims.FirstOrDefault(claim =>
                   string.Equals(claim.Type, claimType, StringComparison.OrdinalIgnoreCase))?.Value
               ?? throw new InvalidOperationException($"El token no contiene el dato requerido '{claimType}'.");
    }
}
