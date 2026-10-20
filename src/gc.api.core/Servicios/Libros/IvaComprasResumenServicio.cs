using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using Newtonsoft.Json;
using System.Security.Claims;

namespace gc.api.core.Servicios.Libros;

public class IvaComprasResumenServicio(IIvaComprasResumenDatos datos, IApiUsuarioServicio usuarios)
{
    public bool TieneAcceso(ClaimsPrincipal usuario, string perfil, string menu)
    {
        if (usuario.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(perfil) || string.IsNullOrWhiteSpace(menu) ||
            usuario.FindFirst("cambio_clave_obligatorio")?.Value == "true") return false;
        var user = usuario.FindFirst("user")?.Value;
        var adm = usuario.FindFirst("AdmId")?.Value.Split('#')[0];
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(adm)) return false;
        List<PerfilUserDto>? perfiles;
        try { perfiles = JsonConvert.DeserializeObject<List<PerfilUserDto>>(usuario.FindFirst("perfiles")?.Value ?? "[]", new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None }); }
        catch (JsonException) { return false; }
        if (perfiles?.Any(p => p.perfil_id == perfil) != true) return false;
        return usuarios.ObtenerMenu(perfil, user, menu, adm).Any(m => string.Equals(m.mnu_item_id, "imp_lc_res", StringComparison.OrdinalIgnoreCase));
    }
    public async Task<IvaComprasResumenLibro> Consultar(IvaComprasResumenFiltro filtro, CancellationToken ct)
    {
        filtro.Validar();
        var libro = await datos.Consultar(filtro, ct);
        libro.Validar();
        return libro;
    }
}
