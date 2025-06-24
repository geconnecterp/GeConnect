using System.IdentityModel.Tokens.Jwt;

namespace gc.sitio.Models.Middleware
{
    public class SessionExpirationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionExpirationMiddleware> _logger;

        public SessionExpirationMiddleware(RequestDelegate next, ILogger<SessionExpirationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Rutas públicas que no requieren verificación de sesión
            if (context.Request.Path.StartsWithSegments("/seguridad/Token/Login") ||
                context.Request.Path.StartsWithSegments("/css") ||
                context.Request.Path.StartsWithSegments("/js") ||
                context.Request.Path.StartsWithSegments("/lib") ||
                context.Request.Path.StartsWithSegments("/fonts") ||
                context.Request.Path.StartsWithSegments("/images"))
            {
                await _next(context);
                return;
            }

            // Si la solicitud es AJAX/fetch, necesitamos manejarla diferente
            bool isAjaxRequest = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                                context.Request.Headers.Accept.ToString().Contains("application/json");

            // Verificar autenticación
            if (!context.User.Identity.IsAuthenticated)
            {
                if (isAjaxRequest)
                {
                    // Para solicitudes AJAX, devolver un código de estado especial
                    context.Response.StatusCode = 440; // Login Timeout (no estándar pero usado comúnmente)
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":true,\"auth\":false,\"msg\":\"Su sesión ha expirado. Por favor inicie sesión nuevamente.\"}");
                    return;
                }
                else
                {
                    // Redireccionar al login para solicitudes normales
                    context.Response.Redirect("/seguridad/Token/Login");
                    return;
                }
            }

            // Verificar si el token JWT está vigente
            string etiqueta = context.Session.GetString("Etiqueta") ?? string.Empty;
            if (!string.IsNullOrEmpty(etiqueta))
            {
                string token = context.Request.Cookies[etiqueta] ?? string.Empty;

                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var tokenS = handler.ReadToken(token) as JwtSecurityToken;

                        if (tokenS == null || tokenS.ValidTo < DateTime.UtcNow)
                        {
                            // Token expirado
                            _logger.LogInformation("Token JWT expirado para el usuario");

                            if (isAjaxRequest)
                            {
                                context.Response.StatusCode = 440;
                                context.Response.ContentType = "application/json";
                                await context.Response.WriteAsync("{\"error\":true,\"auth\":false,\"msg\":\"Su sesión ha expirado. Por favor inicie sesión nuevamente.\"}");
                                return;
                            }
                            else
                            {
                                context.Response.Redirect("/seguridad/Token/Login");
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al verificar el token JWT");

                        if (isAjaxRequest)
                        {
                            context.Response.StatusCode = 440;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync("{\"error\":true,\"auth\":false,\"msg\":\"Error de autenticación. Por favor inicie sesión nuevamente.\"}");
                            return;
                        }
                        else
                        {
                            context.Response.Redirect("/seguridad/Token/Login");
                            return;
                        }
                    }
                }
                else if (isAjaxRequest)
                {
                    // No hay token en las cookies
                    context.Response.StatusCode = 440;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":true,\"auth\":false,\"msg\":\"Su sesión ha expirado. Por favor inicie sesión nuevamente.\"}");
                    return;
                }
                else
                {
                    context.Response.Redirect("/seguridad/Token/Login");
                    return;
                }
            }

            // Verificar la sesión de IdleTimeout
            bool sessionActive = true;
            if (context.Session != null)
            {
                try
                {
                    // Intentar acceder a la sesión para verificar si está activa
                    _ = context.Session.Keys;
                }
                catch
                {
                    sessionActive = false;
                }
            }
            else
            {
                sessionActive = false;
            }

            if (!sessionActive)
            {
                if (isAjaxRequest)
                {
                    context.Response.StatusCode = 440;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\":true,\"auth\":false,\"msg\":\"Su sesión ha expirado. Por favor inicie sesión nuevamente.\"}");
                    return;
                }
                else
                {
                    context.Response.Redirect("/seguridad/Token/Login");
                    return;
                }
            }

            // Continuar con la solicitud si todo está bien
            await _next(context);
        }
    }

    // Extensión para facilitar el registro del middleware
    public static class SessionExpirationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionExpirationCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionExpirationMiddleware>();
        }
    }
}
