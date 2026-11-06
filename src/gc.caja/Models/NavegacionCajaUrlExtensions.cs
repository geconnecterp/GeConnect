using Microsoft.AspNetCore.Mvc;

namespace gc.caja.Models;

public static class NavegacionCajaUrlExtensions
{
    // La portada y el menú comparten Home; todos los retornos operativos usan esta ruta.
    public static string MenuPrincipalCaja(this IUrlHelper url) =>
        url.Action("Index", "Home", new { area = "", menu = true })
        ?? throw new InvalidOperationException("No se pudo generar la ruta al menú principal de Caja.");
}
