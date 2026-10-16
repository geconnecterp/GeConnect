using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using Newtonsoft.Json;
using System.Security.Claims;

namespace gc.api.core.Servicios.Libros;

public class IvaVentasCierreServicio(IIvaVentasCierreDatos datos, IApiUsuarioServicio usuarios)
{
    public const string Permiso = "imp_lv_cierr";
    public bool TieneAcceso(ClaimsPrincipal usuario, string perfil, string menu)
    {
        if (usuario.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(perfil) || string.IsNullOrWhiteSpace(menu) ||
            usuario.FindFirst("cambio_clave_obligatorio")?.Value == "true") return false;
        var actor = usuario.FindFirst("user")?.Value;
        var adm = usuario.FindFirst("AdmId")?.Value.Split('#')[0];
        if (string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(adm)) return false;
        List<PerfilUserDto>? perfiles;
        try { perfiles = JsonConvert.DeserializeObject<List<PerfilUserDto>>(usuario.FindFirst("perfiles")?.Value ?? "[]", new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None }); }
        catch (JsonException) { return false; }
        return perfiles?.Any(p => p.perfil_id == perfil) == true &&
            usuarios.ObtenerMenu(perfil, actor, menu, adm).Any(m => string.Equals(m.mnu_item_id, Permiso, StringComparison.OrdinalIgnoreCase));
    }
    private void Autorizar(ClaimsPrincipal usuario, string perfil, string menu)
    {
        if (!TieneAcceso(usuario, perfil, menu)) throw new UnauthorizedAccessException("El perfil no tiene acceso a Cierre IVA Ventas.");
    }
    public async Task<IvaVentasCierreEstado> Consultar(ClaimsPrincipal usuario, string perfil, string menu, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu);
        var estado = await datos.Consultar(ct);
        estado.Validar();
        estado.PuedeCerrar = usuario.FindFirst("AdmId")?.Value.Split('#')[0] == "0000";
        return estado;
    }
    public Task<IvaVentasCierreRespuesta> Cerrar(IvaVentasCierrePedido pedido, ClaimsPrincipal usuario, string perfil, string menu, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu); pedido.Validar();
        var adm = usuario.FindFirst("AdmId")!.Value.Split('#')[0];
        var actor = usuario.FindFirst("user")!.Value;
        if (adm != "0000") throw new UnauthorizedAccessException("El cierre sólo está habilitado desde la sucursal Central (0000).");
        if (actor.Length > 10) throw new InvalidOperationException("El identificador de sesión excede el contrato del cierre.");
        return datos.Cerrar(pedido.Periodo, actor, adm, ct);
    }
    public async Task GenerarRg3685(IvaVentasCierrePedido pedido, ClaimsPrincipal usuario, string perfil, string menu, Stream destino, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu); pedido.Validar();
        if (!await datos.ExisteCierre(pedido.Periodo, ct))
            throw new IvaVentasCierreConflictoException("Seleccione un período cerrado del historial antes de descargar RG 3685.");
        await IvaVentasCierreArchivos.EscribirZip(destino, pedido.Periodo,
            tipo => datos.LineasRg3685(pedido.Periodo, tipo, ct), ct);
    }
}
