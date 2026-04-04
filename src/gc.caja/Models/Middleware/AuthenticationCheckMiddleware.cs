namespace gc.caja.Models.Middleware
{
    public class AuthenticationCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ✅ CORREGIDO: Rutas públicas con StringComparison.OrdinalIgnoreCase
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            // Rutas públicas que no requieren autenticación
            if (path.StartsWith("/seguridad/token/login") ||
                path.StartsWith("/css") ||
                path.StartsWith("/js") ||
                path.StartsWith("/lib") ||
                path.StartsWith("/fonts") ||
                path.StartsWith("/images"))
            {
                await _next(context);
                return;
            }

            // Verificar autenticación
            if (!context.User.Identity.IsAuthenticated)
            {
                var loginPath = $"{context.Request.PathBase}/seguridad/token/login";
                context.Response.Redirect(loginPath);
                return;
            }

            // Verificar si tiene el claim AdmId
            var admClaim = context.User.Claims.FirstOrDefault(c => c.Type.Contains("AdmId"));
            if (admClaim == null || string.IsNullOrEmpty(admClaim.Value))
            {
                var loginPath = $"{context.Request.PathBase}/seguridad/token/login";
                context.Response.Redirect(loginPath);
                return;
            }

            await _next(context);
        }
    }
}
