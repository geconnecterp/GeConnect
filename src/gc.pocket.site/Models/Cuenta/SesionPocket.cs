using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace gc.pocket.site.Models.Cuenta;

public static class SesionPocket
{
    public static async Task Cerrar(HttpContext context)
    {
        var etiqueta = context.Session.GetString("Etiqueta");
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!string.IsNullOrWhiteSpace(etiqueta))
            context.Response.Cookies.Delete(etiqueta, new CookieOptions { Path = "/" });
        context.Session.Clear();
    }
}
