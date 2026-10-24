using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using Newtonsoft.Json;
using System.Security.Claims;

namespace gc.api.core.Servicios.Libros;

public class CertificadosRetencionServicio(ICertificadosRetencionDatos datos, IApiUsuarioServicio usuarios)
{
    public const string Permiso = "imp_ar";
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
        if (!TieneAcceso(usuario, perfil, menu)) throw new UnauthorizedAccessException("El perfil no tiene acceso a Certificados de Retención Emitidos (imp_ar).");
    }
    public async Task GenerarDdjj(CertificadosRetencionDdjjPedido pedido, ClaimsPrincipal usuario, string perfil, string menu, Stream destino, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu); pedido.Validar();
        using var plazo = CancellationTokenSource.CreateLinkedTokenSource(ct);
        plazo.CancelAfter(TimeSpan.FromMinutes(9));
        if (!(await datos.Impuestos(plazo.Token)).Any(i => i.Id == pedido.Impuesto && i.AgenteRet == "S" && i.Soportado))
            throw new ArgumentException("El impuesto seleccionado no está habilitado como agente de retención.");
        await CertificadosRetencionDdjjArchivos.Escribir(destino, pedido.Impuesto, datos.Ddjj(pedido, plazo.Token), plazo.Token);
    }
    public async Task<List<RetencionImpuesto>> Opciones(ClaimsPrincipal usuario, string perfil, string menu, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu);
        return (await datos.Impuestos(ct)).Where(i => i.AgenteRet == "S").ToList();
    }
    public async Task<CertificadosRetencionConsulta> Consultar(CertificadosRetencionFiltro filtro, ClaimsPrincipal usuario, string perfil, string menu, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu); filtro.Validar();
        if (!(await datos.Impuestos(ct)).Any(i => i.Id == filtro.Impuesto && i.AgenteRet == "S" && i.Soportado))
            throw new ArgumentException("El impuesto seleccionado no está habilitado como agente de retención.");
        var consulta = new CertificadosRetencionConsulta { Filas = await datos.Consultar(filtro, ct) };
        // Evaluar todos los totales antes de entregar la respuesta (incluido un posible overflow).
        _ = consulta.TotalBase; _ = consulta.TotalRetencion; _ = consulta.TotalLH;
        return consulta;
    }
}
