using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using Newtonsoft.Json;
using System.Security.Claims;

namespace gc.api.core.Servicios.Libros;

public class CertificadosPercepcionServicio(ICertificadosPercepcionDatos datos, IApiUsuarioServicio usuarios)
{
    public const string Permiso = "imp_ap";
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
        if (!TieneAcceso(usuario, perfil, menu)) throw new UnauthorizedAccessException("El perfil no tiene acceso a Certificados de Percepción Emitidos (imp_ap).");
    }
    public async Task GenerarDdjj(CertificadosPercepcionDdjjPedido pedido, ClaimsPrincipal usuario, string perfil, string menu, Stream destino, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu); pedido.Validar();
        using var plazo = CancellationTokenSource.CreateLinkedTokenSource(ct);
        plazo.CancelAfter(TimeSpan.FromMinutes(9));
        if (!(await datos.Impuestos(plazo.Token)).Any(i => i.Id == pedido.Impuesto && i.AgentePer == "S" && i.Soportado))
            throw new ArgumentException("El impuesto seleccionado no está habilitado como agente de percepción.");
        await CertificadosPercepcionDdjjArchivos.Escribir(destino, pedido.Impuesto, datos.Ddjj(pedido, plazo.Token), plazo.Token);
    }
    public async Task<List<PercepcionImpuesto>> Opciones(ClaimsPrincipal usuario, string perfil, string menu, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu);
        return (await datos.Impuestos(ct)).Where(i => i.AgentePer == "S" && i.Soportado).ToList();
    }
    public async Task<CertificadosPercepcionConsulta> Consultar(CertificadosPercepcionFiltro filtro, ClaimsPrincipal usuario, string perfil, string menu, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu); filtro.Validar();
        using var plazo = CancellationTokenSource.CreateLinkedTokenSource(ct);
        plazo.CancelAfter(TimeSpan.FromMinutes(9));
        if (!(await datos.Impuestos(plazo.Token)).Any(i => i.Id == filtro.Impuesto && i.AgentePer == "S" && i.Soportado))
            throw new ArgumentException("El impuesto seleccionado no está habilitado como agente de percepción.");
        var consulta = new CertificadosPercepcionConsulta { Filas = await datos.Consultar(filtro, plazo.Token) };
        _ = consulta.TotalBase; _ = consulta.TotalPercepcion; _ = consulta.TotalLH;
        return consulta;
    }
}
