namespace gc.pocket.site.Models.Cuenta;

// Después de Authentication: usa claims del ticket protegido, nunca del formulario.
public sealed class AccesoCuentaMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }
        var destino = AccesoCuentaPocket.Redireccion(context.User, context.Request.Path);
        if (destino == null) { await next(context); return; }
        var url = $"{context.Request.PathBase}{destino}";
        context.Response.Headers.CacheControl = "no-store";
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
            context.Request.Headers.Accept.ToString().Contains("application/json"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { ok = false, error = true, warn = true,
                cambioClave = true, msg = "Debe actualizar su contraseña antes de continuar.", redirect = url });
        }
        else context.Response.Redirect(url);
    }
}
