using gc.auth.remoto.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;

namespace gc.auth.remoto.Middleware;

public sealed class RemoteSessionExpirationMiddleware
{
    private readonly RequestDelegate _next;

    public RemoteSessionExpirationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsPublicRequest(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var token = context.Session.GetString(AuthenticationSession.JwtToken);
        var authenticated = context.User.Identity?.IsAuthenticated == true;

        if (!authenticated || !IsActive(token))
        {
            context.Session.Clear();
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (IsAjax(context.Request))
            {
                context.Response.StatusCode = 440;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "La sesión finalizó. Debe autenticarse nuevamente."
                });
                return;
            }

            var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
            var loginPath = $"{context.Request.PathBase}/Seguridad/Token/Login";
            context.Response.Redirect($"{loginPath}?returnUrl={Uri.EscapeDataString(returnUrl)}");
            return;
        }

        await _next(context);
    }

    private static bool IsActive(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.ValidTo > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsPublicRequest(PathString path) =>
        path.StartsWithSegments("/Seguridad/Token/Login", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/favicon.ico", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/Home/Error", StringComparison.OrdinalIgnoreCase);

    private static bool IsAjax(HttpRequest request) =>
        string.Equals(request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
