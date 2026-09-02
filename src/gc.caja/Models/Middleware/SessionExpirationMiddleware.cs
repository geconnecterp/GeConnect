using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace gc.caja.Models.Middleware
{
    public class SessionExpirationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionExpirationMiddleware> _logger;
        private readonly string _pathBase;

        public SessionExpirationMiddleware(
            RequestDelegate next,
            ILogger<SessionExpirationMiddleware> logger,
            IOptions<AppSettings> options)
        {
            _next = next;
            _logger = logger;
            _pathBase = options.Value.PathBase ?? string.Empty;
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

            // ✅ NUEVO: Determinar la URL de login
            var loginPath = $"{context.Request.PathBase}/seguridad/Token/Login";

            // Verificar autenticación
            if (context.User.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("Usuario no autenticado intentando acceder a: {Path}", context.Request.Path);
                await RedirigirALogin(context, isAjaxRequest, loginPath, "Usuario no autenticado.");
                return;
            }

            // ✅ NUEVO: Verificar etiqueta de sesión ANTES de intentar leer el token
            string etiqueta = context.Session.GetString("Etiqueta") ?? string.Empty;
            
            if (string.IsNullOrEmpty(etiqueta))
            {
                _logger.LogWarning("Etiqueta de sesión no encontrada para el usuario autenticado. Path: {Path}", context.Request.Path);
                await RedirigirALogin(context, isAjaxRequest, loginPath, "Sesión inválida. Por favor inicie sesión nuevamente.");
                return;
            }

            // ✅ NUEVO: Verificar que el token exista en las cookies
            string token = context.Request.Cookies[etiqueta] ?? string.Empty;
            
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token no encontrado en cookies. Etiqueta: {Etiqueta}, Path: {Path}", etiqueta, context.Request.Path);
                await RedirigirALogin(context, isAjaxRequest, loginPath, "Su sesión ha expirado. Por favor inicie sesión nuevamente.");
                return;
            }

            // ✅ MEJORADO: Validar token JWT con manejo robusto de errores
            try
            {
                var handler = new JwtSecurityTokenHandler();
                
                // ✅ CRÍTICO: Validar que el token no esté vacío antes de leerlo
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("Token vacío detectado. Etiqueta: {Etiqueta}", etiqueta);
                    await RedirigirALogin(context, isAjaxRequest, loginPath, "Token de sesión inválido.");
                    return;
                }

                // ✅ NUEVO: Validar formato del token antes de intentar leerlo
                if (!handler.CanReadToken(token))
                {
                    _logger.LogWarning("Token con formato inválido. Etiqueta: {Etiqueta}", etiqueta);
                    await RedirigirALogin(context, isAjaxRequest, loginPath, "Token de sesión inválido.");
                    return;
                }

                var tokenS = handler.ReadToken(token) as JwtSecurityToken;

                if (tokenS == null)
                {
                    _logger.LogWarning("No se pudo leer el token JWT. Etiqueta: {Etiqueta}", etiqueta);
                    await RedirigirALogin(context, isAjaxRequest, loginPath, "Token de sesión inválido.");
                    return;
                }

                // Verificar expiración
                if (tokenS.ValidTo < DateTime.UtcNow)
                {
                    _logger.LogInformation("Token JWT expirado. Usuario: {Usuario}, Expiró: {ValidTo}", 
                        tokenS.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value ?? "Desconocido",
                        tokenS.ValidTo);
                    
                    await RedirigirALogin(context, isAjaxRequest, loginPath, "Su sesión ha expirado. Por favor inicie sesión nuevamente.");
                    return;
                }
            }
            catch (ArgumentNullException ex)
            {
                // ✅ NUEVO: Captura específica para tokens nulos
                _logger.LogError(ex, "Error ArgumentNullException al verificar token. Etiqueta: {Etiqueta}, Path: {Path}", etiqueta, context.Request.Path);
                await RedirigirALogin(context, isAjaxRequest, loginPath, "Error de autenticación. Por favor inicie sesión nuevamente.");
                return;
            }
            catch (SecurityTokenException ex)
            {
                // ✅ NUEVO: Captura específica para errores de seguridad del token
                _logger.LogError(ex, "Error de seguridad al verificar token. Etiqueta: {Etiqueta}, Path: {Path}", etiqueta, context.Request.Path);
                await RedirigirALogin(context, isAjaxRequest, loginPath, "Token de sesión inválido. Por favor inicie sesión nuevamente.");
                return;
            }
            catch (Exception ex)
            {
                // ✅ MEJORADO: Captura genérica con más contexto
                _logger.LogError(ex, "Error inesperado al verificar token JWT. Etiqueta: {Etiqueta}, Path: {Path}, Tipo: {ExceptionType}", 
                    etiqueta, context.Request.Path, ex.GetType().Name);
                
                await RedirigirALogin(context, isAjaxRequest, loginPath, "Error de autenticación. Por favor inicie sesión nuevamente.");
                return;
            }

            // ✅ MEJORADO: Verificar la sesión de IdleTimeout con try-catch
            bool sessionActive = true;
            try
            {
                if (context.Session != null)
                {
                    // Intentar acceder a la sesión para verificar si está activa
                    _ = context.Session.Keys.Count();
                }
                else
                {
                    sessionActive = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al verificar estado de sesión. Path: {Path}", context.Request.Path);
                sessionActive = false;
            }

            if (!sessionActive)
            {
                _logger.LogWarning("Sesión inactiva detectada. Path: {Path}", context.Request.Path);
                await RedirigirALogin(context, isAjaxRequest, loginPath, "Su sesión ha expirado por inactividad. Por favor inicie sesión nuevamente.");
                return;
            }

            // Continuar con la solicitud si todo está bien
            await _next(context);
        }

        /// <summary>
        /// ✅ NUEVO: Método centralizado para redirigir al login
        /// </summary>
        private async Task RedirigirALogin(HttpContext context, bool isAjaxRequest, string loginPath, string mensaje)
        {
            if (isAjaxRequest)
            {
                // Para solicitudes AJAX, devolver JSON con código 440 (Login Timeout)
                context.Response.StatusCode = 440;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{\"error\":true,\"auth\":false,\"msg\":\"{mensaje}\"}}");
            }
            else
            {
                // Para solicitudes normales, redirigir al login
                context.Response.Redirect(loginPath);
            }
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
