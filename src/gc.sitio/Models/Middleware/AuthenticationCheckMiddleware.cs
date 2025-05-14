namespace gc.sitio.Models.Middleware
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
            // Si la ruta es para login o áreas públicas, permitimos el acceso
            if (context.Request.Path.StartsWithSegments("/seguridad/Token/Login") ||
                context.Request.Path.StartsWithSegments("/css") ||
                context.Request.Path.StartsWithSegments("/js"))
            {
                await _next(context);
                return;
            }

            // Verificar autenticación
            if (!context.User.Identity.IsAuthenticated)
            {
                context.Response.Redirect("/seguridad/Token/Login");
                return;
            }

            // Verificar si tiene el claim AdmId
            var admClaim = context.User.Claims.FirstOrDefault(c => c.Type.Contains("AdmId"));
            if (admClaim == null || string.IsNullOrEmpty(admClaim.Value))
            {
                context.Response.Redirect("/seguridad/Token/Login");
                return;
            }

            await _next(context);
        }
    }
}
