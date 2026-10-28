using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using Newtonsoft.Json;
using System.Security.Claims;

namespace gc.api.core.Servicios.Libros;

public class CertificadosSufridosServicio(ICertificadosSufridosDatos datos, IApiUsuarioServicio usuarios)
{
    public const string Permiso = "imp_cs";
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
        if (!TieneAcceso(usuario, perfil, menu)) throw new UnauthorizedAccessException("El perfil no tiene acceso a Certificados Sufridos (imp_cs).");
    }
    public async Task<List<SufridoImpuesto>> Opciones(ClaimsPrincipal usuario, string perfil, string menu, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu);
        return (await datos.Impuestos(ct)).Where(i => i.Contribuyente == "S").ToList();
    }
    public async Task<CertificadosSufridosConsulta> Consultar(CertificadosSufridosFiltro filtro, ClaimsPrincipal usuario, string perfil, string menu, CancellationToken ct)
    {
        Autorizar(usuario, perfil, menu); filtro.Validar();
        using var plazo = CancellationTokenSource.CreateLinkedTokenSource(ct);
        plazo.CancelAfter(TimeSpan.FromMinutes(9));
        var impuesto = (await datos.Impuestos(plazo.Token)).FirstOrDefault(i => i.Id == filtro.Impuesto && i.Contribuyente == "S")
            ?? throw new ArgumentException("El impuesto no está habilitado como contribuyente (cont = S).");
        var consulta = new CertificadosSufridosConsulta { AgrupadoPorCuit = filtro.AgrupadoPorCuit, ImpuestoTexto = impuesto.Texto };
        if (filtro.AgrupadoPorCuit)
        {
            consulta.FilasPorCuit = await datos.ConsultarPorCuit(filtro, plazo.Token);
            _ = CertificadosSufridosConsulta.SumarPorCuit(consulta.FilasPorCuit);
            _ = CertificadosSufridosConsulta.CantidadPorCuit(consulta.FilasPorCuit);
            return consulta;
        }
        consulta.Filas = await datos.Consultar(filtro, plazo.Token);
        // Comprobar desbordes antes de devolver datos al cliente.
        _ = CertificadosSufridosConsulta.Sumar(consulta.Filas, f => f.@base);
        _ = CertificadosSufridosConsulta.Sumar(consulta.Filas, f => CertificadosSufridosConsulta.TotalFila(f, filtro.Impuesto == "IB"));
        return consulta;
    }
}
