using System.Security.Claims;

namespace gc.caja.Models.Cuenta;

public static class AccesoCuentaCaja
{
    public const string Inicio = "/Seguridad/Cuenta/Index";
    public const string Obligatoria = "/Seguridad/Cuenta/ClaveObligatoria";
    public static bool Forzada(ClaimsPrincipal user) => Tiene(user, "cambio_clave_obligatorio");
    public static bool Vencida(ClaimsPrincipal user) => Tiene(user, "clave_expirada");
    private static bool Tiene(ClaimsPrincipal user, string claim) =>
        string.Equals(user.FindFirst(claim)?.Value, "true", StringComparison.OrdinalIgnoreCase);

    public static string? Redireccion(ClaimsPrincipal user, string? path)
    {
        var ruta = (path ?? "").TrimEnd('/');
        bool Es(string valor) => string.Equals(ruta, valor, StringComparison.OrdinalIgnoreCase);
        if (Es("/Seguridad/Token/Logout")) return null;
        if (Forzada(user))
            return Es(Obligatoria) || Es("/Seguridad/Cuenta/CambiarClaveObligatoria") ? null : Obligatoria;
        if (Vencida(user))
            return Es(Inicio) || Es("/Seguridad/Cuenta") || Es("/Seguridad/Cuenta/CambiarClave") ? null : Inicio;
        return null;
    }
}
