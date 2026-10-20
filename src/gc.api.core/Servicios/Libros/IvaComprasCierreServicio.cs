using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;

namespace gc.api.core.Servicios.Libros;

public class IvaComprasCierreServicio(IIvaComprasCierreDatos datos, IApiUsuarioServicio usuarios, ILogger<IvaComprasCierreServicio> logger)
{
    public const string Permiso = "imp_lc_cierr";
    public bool TieneAcceso(ClaimsPrincipal usuario, string perfil, string menu)
    {
        if (usuario.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(perfil) || string.IsNullOrWhiteSpace(menu) ||
            usuario.FindFirst("cambio_clave_obligatorio")?.Value == "true") return false;
        var actor = usuario.FindFirst("user")?.Value;
        var adm = usuario.FindFirst("AdmId")?.Value.Split('#')[0];
        if (string.IsNullOrWhiteSpace(actor) || actor.Length > 10 || string.IsNullOrWhiteSpace(adm) || adm.Length != 4) return false;
        List<PerfilUserDto>? perfiles;
        try { perfiles = JsonConvert.DeserializeObject<List<PerfilUserDto>>(usuario.FindFirst("perfiles")?.Value ?? "[]", new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None }); }
        catch (JsonException) { return false; }
        return perfiles?.Any(p => p != null && p.perfil_id == perfil) == true &&
            usuarios.ObtenerMenu(perfil, actor, menu, adm).Any(m => string.Equals(m.mnu_item_id, Permiso, StringComparison.OrdinalIgnoreCase));
    }

    private void Autorizar(ClaimsPrincipal usuario, string perfil, string menu)
    {
        if (!TieneAcceso(usuario, perfil, menu)) throw new UnauthorizedAccessException("El perfil no tiene acceso a Cierre IVA Compras.");
    }

    public async Task<IvaComprasCierreEstado> Consultar(ClaimsPrincipal usuario, string perfil, string menu, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu);
        var estado = await datos.Consultar(ct);
        estado.Validar();
        estado.PuedeCerrar = usuario.FindFirst("AdmId")?.Value.Split('#')[0] == "0000";
        return estado;
    }

    public async Task<IvaComprasCierreRespuesta> Cerrar(IvaComprasCierrePedido pedido, ClaimsPrincipal usuario, string perfil, string menu, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu); pedido.Validar();
        var adm = usuario.FindFirst("AdmId")!.Value.Split('#')[0];
        var actor = usuario.FindFirst("user")!.Value;
        if (adm != "0000") throw new UnauthorizedAccessException("El cierre sólo está habilitado desde la sucursal Central (0000).");
        using var scope = logger.BeginScope(new Dictionary<string, object> { ["Actor"] = actor, ["Administracion"] = adm, ["Periodo"] = pedido.Periodo });
        logger.LogInformation("IVA Compras: solicitud de cierre. Actor {Actor}; administración {Administracion}; período {Periodo}. Sin reintentos automáticos.", actor, adm, pedido.Periodo);
        try
        {
            var respuesta = await datos.Cerrar(pedido.Periodo, actor, adm, ct);
            if (respuesta == null || string.IsNullOrWhiteSpace(respuesta.resultado_msj))
                throw new InvalidOperationException("El cierre no devolvió una respuesta válida.");
            logger.LogInformation("IVA Compras: cierre respondió código {Resultado}. Actor {Actor}; administración {Administracion}; período {Periodo}.", respuesta.resultado, actor, adm, pedido.Periodo);
            return respuesta;
        }
        catch (Exception ex)
        {
            logger.LogWarning("IVA Compras: solicitud de cierre interrumpida. Tipo {Tipo}; actor {Actor}; administración {Administracion}; período {Periodo}. Verificar historial antes de repetir.", ex.GetType().Name, actor, adm, pedido.Periodo);
            throw;
        }
    }

    public async Task<IvaComprasCierreConteos> GenerarRg3685(IvaComprasCierreDescargaPedido pedido, ClaimsPrincipal usuario, string perfil, string menu, Stream destino, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu); pedido.Validar();
        if (!await datos.ExisteCierre(pedido.Periodo, pedido.TipoRegistro, ct))
            throw new IvaComprasCierreConflictoException("Seleccione un registro cerrado del historial antes de descargar RG 3685.");
        var conteos = await IvaComprasCierreArchivos.EscribirZip(destino, pedido.Periodo,
            tipo => datos.LineasRg3685(pedido.Periodo, tipo, ct), ct);
        logger.LogInformation("IVA Compras RG 3685: período {Periodo}; C {Cabecera}; D {Detalle}; anchos no nominales C {AnchoC}, D {AnchoD}.",
            pedido.Periodo, conteos.Cabecera, conteos.Detalle, conteos.LongitudesCabecera, conteos.LongitudesDetalle);
        return conteos;
    }
}

