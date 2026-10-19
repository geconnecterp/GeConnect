using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using System.Security.Claims;
using Newtonsoft.Json;

namespace gc.api.core.Servicios.Libros;

public class IvaComprasServicio(IIvaComprasDatos datos, IApiUsuarioServicio usuarios)
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
        return usuarios.ObtenerMenu(perfil, user, menu, adm).Any(m => m.mnu_item_id.Equals("imp_lc_det", StringComparison.OrdinalIgnoreCase));
    }

    public Task<IvaComprasOpciones> Opciones(CancellationToken ct) => datos.Opciones(ct);

    public async Task<IvaComprasPagina> Consultar(IvaComprasFiltro filtro, CancellationToken ct)
    {
        filtro.Validar();
        var filas = await datos.Consultar(filtro, false, ct);
        // El SP no devuelve metadatos si la página está vacía. Reconsultar la primera
        // permite distinguir un conjunto vacío de una página eliminada por cambios concurrentes.
        if (filas.Count == 0 && filtro.Pagina > 1)
        {
            filtro.Pagina = 1;
            filas = await datos.Consultar(filtro, false, ct);
        }
        var total = filas.FirstOrDefault()?.total_registros ?? 0;
        var paginas = (int)Math.Ceiling((double)total / filtro.Registros);
        var inicio = (long)(filtro.Pagina - 1) * filtro.Registros;
        var esperados = (int)Math.Max(0L, Math.Min(filtro.Registros, (long)total - inicio));
        if (total < 0 || filas.Count != esperados ||
            filas.Any(x => x.total_registros != total || x.total_paginas != paginas) ||
            filas.Select(x => (x.tipo, x.compte, x.movi, x.cta_id)).Distinct().Count() != filas.Count)
            throw new InvalidOperationException("Los metadatos de la consulta IVA son inconsistentes.");
        return new() { Filas = filas, Total = total, Pagina = filtro.Pagina, Registros = filtro.Registros,
            Paginas = paginas };
    }

    public async Task<List<IvaComprasDetalleDto>> Todos(IvaComprasFiltro filtro, CancellationToken ct)
    {
        filtro.Validar(exportacion: true);
        var filas = await datos.Consultar(filtro, true, ct);
        ValidarExportacionCompleta(filas);
        return filas;
    }

    public static void ValidarExportacionCompleta(IReadOnlyList<IvaComprasDetalleDto> filas)
    {
        if (filas.Count == 0) throw new ArgumentException("No hay comprobantes para exportar con estos filtros.");
        if (filas.Any(x => x.total_registros != filas.Count || x.total_paginas != 1))
            throw new InvalidOperationException("La cantidad recibida no coincide con el total. No se generó un archivo parcial; vuelva a consultar.");
        if (filas.Select(x => (x.tipo, x.compte, x.movi, x.cta_id)).Distinct().Count() != filas.Count)
            throw new InvalidOperationException("La consulta devolvió comprobantes duplicados. Revise el resultado con el administrador.");
    }
}
