using gc.caja.core.Autorizaciones;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace gc.caja.Models;

public sealed partial class AutorizacionRemotaOrquestador : IAutorizacionRemotaOrquestador
{
    private const string PrefijoSesion = "AutorizacionRemota:Vigente:";

    private readonly IAutorizacionRemotaServicio _servicio;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AutorizacionRemotaOptions _options;
    private readonly ILogger<AutorizacionRemotaOrquestador> _logger;

    public AutorizacionRemotaOrquestador(
        IAutorizacionRemotaServicio servicio,
        IHttpContextAccessor httpContextAccessor,
        IOptions<AutorizacionRemotaOptions> options,
        ILogger<AutorizacionRemotaOrquestador> logger)
    {
        _servicio = servicio;
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AutorizacionRemotaSesion> IniciarAsync(
        string claveOperacion,
        CrearAutorizacionRemotaSolicitud solicitud,
        string idempotencyKey,
        string token,
        CancellationToken cancellationToken = default)
    {
        var claveNormalizada = NormalizarClave(claveOperacion);
        ArgumentNullException.ThrowIfNull(solicitud);

        var idSolicitud = await _servicio.CrearAsync(
            solicitud,
            idempotencyKey,
            token,
            cancellationToken);

        var vigente = new AutorizacionRemotaSesion
        {
            ClaveOperacion = claveNormalizada,
            IdSolicitud = idSolicitud,
            IdSolicitudExterna = solicitud.IdSolicitudExterna,
            IdempotencyKey = idempotencyKey,
            UsuarioSolicitante = solicitud.UsuarioSolicitante,
            CodigoModuloOrigen = _options.CodigoModuloOrigen,
            DerCodigo = solicitud.DerCodigo,
            ContextoSha256 = CalcularHash(solicitud.Contexto),
            FechaRegistroUtc = DateTime.UtcNow
        };

        Sesion.SetString(ClaveSesion(claveNormalizada), JsonConvert.SerializeObject(vigente));

        _logger.LogInformation(
            "La solicitud {IdSolicitud} quedó vigente para la operación {ClaveOperacion}.",
            vigente.IdSolicitud,
            vigente.ClaveOperacion);

        return vigente;
    }

    public async Task<AutorizacionRemotaConsultaResultado> ConsultarAsync(
        string claveOperacion,
        Guid idSolicitud,
        string token,
        CancellationToken cancellationToken = default)
    {
        var vigente = ObtenerVigente(claveOperacion);
        if (vigente is null || vigente.IdSolicitud != idSolicitud)
        {
            return AutorizacionRemotaConsultaResultado.Reemplazada();
        }

        var solicitud = await _servicio.ObtenerResolucionAsync(
            idSolicitud,
            token,
            cancellationToken);

        ValidarIntegridad(vigente, solicitud);

        return new AutorizacionRemotaConsultaResultado
        {
            Vigente = true,
            Estado = solicitud.Estado,
            Solicitud = solicitud
        };
    }

    public AutorizacionRemotaSesion? ObtenerVigente(string claveOperacion)
    {
        var clave = ClaveSesion(NormalizarClave(claveOperacion));
        var json = Sesion.GetString(clave);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonConvert.DeserializeObject<AutorizacionRemotaSesion>(json);
    }

    public bool EsVigente(string claveOperacion, Guid idSolicitud)
    {
        var vigente = ObtenerVigente(claveOperacion);
        return vigente is not null && vigente.IdSolicitud == idSolicitud;
    }

    public void Completar(string claveOperacion, Guid idSolicitud)
    {
        var claveNormalizada = NormalizarClave(claveOperacion);
        var vigente = ObtenerVigente(claveNormalizada);

        if (vigente is null || vigente.IdSolicitud != idSolicitud)
        {
            return;
        }

        Sesion.Remove(ClaveSesion(claveNormalizada));
        _logger.LogInformation(
            "La solicitud vigente {IdSolicitud} fue completada para {ClaveOperacion}.",
            idSolicitud,
            claveNormalizada);
    }

    private ISession Sesion => _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("No existe una sesión HTTP activa.");

    private static string ClaveSesion(string claveOperacion) => PrefijoSesion + claveOperacion;

    private static string NormalizarClave(string claveOperacion)
    {
        var clave = claveOperacion?.Trim().ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(clave) || clave.Length > 50 || !ClaveValidaRegex().IsMatch(clave))
        {
            throw new ArgumentException(
                "La clave de operación admite hasta 50 caracteres: letras, números, punto, guión y guión bajo.",
                nameof(claveOperacion));
        }

        return clave;
    }

    private static string CalcularHash(JToken contexto)
    {
        var json = contexto.ToString(Formatting.None);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
    }

    private static void ValidarIntegridad(
        AutorizacionRemotaSesion vigente,
        RemoteAuthorizations.Application.Responses.SolicitudAutorizacionRespuesta solicitud)
    {
        var contexto = string.IsNullOrWhiteSpace(solicitud.ContextoJson)
            ? new JObject()
            : JToken.Parse(solicitud.ContextoJson);

        var integridadValida =
            solicitud.Id == vigente.IdSolicitud &&
            string.Equals(
                solicitud.IdSolicitudExterna,
                vigente.IdSolicitudExterna,
                StringComparison.Ordinal) &&
            string.Equals(
                solicitud.IdUsuarioSolicitante,
                vigente.UsuarioSolicitante,
                StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                solicitud.CodigoModuloOrigen,
                vigente.CodigoModuloOrigen,
                StringComparison.OrdinalIgnoreCase) &&
            solicitud.DerCodigo == vigente.DerCodigo &&
            string.Equals(
                CalcularHash(contexto),
                vigente.ContextoSha256,
                StringComparison.Ordinal);

        if (!integridadValida)
        {
            throw new InvalidOperationException(
                "La autorización recibida no coincide con la operación vigente.");
        }
    }

    [GeneratedRegex("^[A-Z0-9_.-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex ClaveValidaRegex();
}
