using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.SolAuth.Comando;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RemoteAuthorizations.Application.Responses;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace gc.caja.core.Autorizaciones;

public sealed class AutorizacionRemotaServicio : IAutorizacionRemotaServicio
{
    private readonly HttpClient _httpClient;
    private readonly AutorizacionRemotaOptions _options;
    private readonly ILogger<AutorizacionRemotaServicio> _logger;

    public AutorizacionRemotaServicio(
        HttpClient httpClient,
        IOptions<AutorizacionRemotaOptions> options,
        ILogger<AutorizacionRemotaServicio> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Guid> CrearAsync(
        CrearAutorizacionRemotaSolicitud solicitud,
        string idempotencyKey,
        string token,
        CancellationToken cancellationToken = default)
    {
        ValidarCreacion(solicitud, idempotencyKey, token);

        var comando = new CrearSolicitudAutorizacionComando
        {
            CodigoModuloOrigen = _options.CodigoModuloOrigen,
            usu_id = solicitud.UsuarioSolicitante,
            IdSolicitudExterna = solicitud.IdSolicitudExterna,
            DerCodigo = solicitud.DerCodigo,
            TimeoutSegundos = solicitud.TimeoutSegundos,
            ResolucionPorDefecto = new ResolucionPorDefectoComando
            {
                Decision = solicitud.DecisionPorDefecto,
                CodigoResolucion = solicitud.CodigoResolucionPorDefecto,
                Mensaje = solicitud.MensajeResolucionPorDefecto
            },
            Contexto = solicitud.Contexto
        };

        using var request = CrearRequest(HttpMethod.Post, RutaBase(), token);
        request.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
        request.Content = new StringContent(
            JsonConvert.SerializeObject(comando),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var contenido = await LeerContenidoExitosoAsync(response, cancellationToken);
        var resultado = JsonConvert.DeserializeObject<RespuestaDto>(contenido)
            ?? throw new NegocioException("La API no devolvió una respuesta válida al crear la autorización.");

        if (resultado.IdFile == Guid.Empty)
        {
            throw new NegocioException("La API no devolvió el identificador de la autorización creada.");
        }

        _logger.LogInformation(
            "Autorización remota {IdSolicitud} creada para el origen {CodigoModuloOrigen} y el derecho {DerCodigo}.",
            resultado.IdFile,
            _options.CodigoModuloOrigen,
            solicitud.DerCodigo);

        return resultado.IdFile;
    }

    public async Task<SolicitudAutorizacionRespuesta> ObtenerResolucionAsync(
        Guid idSolicitud,
        string token,
        CancellationToken cancellationToken = default)
    {
        if (idSolicitud == Guid.Empty)
        {
            throw new ArgumentException("El identificador de solicitud es requerido.", nameof(idSolicitud));
        }

        ValidarToken(token);

        using var request = CrearRequest(
            HttpMethod.Get,
            $"{RutaBase()}/{idSolicitud:D}/resolucion",
            token);

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var contenido = await LeerContenidoExitosoAsync(response, cancellationToken);
        return JsonConvert.DeserializeObject<SolicitudAutorizacionRespuesta>(contenido)
            ?? throw new NegocioException("La API no devolvió un estado de autorización válido.");
    }

    private string RutaBase()
    {
        var ruta = _options.RutaApi?.Trim().Trim('/');
        if (string.IsNullOrWhiteSpace(ruta))
        {
            throw new InvalidOperationException("Debe configurarse AutorizacionRemota:RutaApi.");
        }

        return ruta;
    }

    private static HttpRequestMessage CrearRequest(HttpMethod metodo, string ruta, string token)
    {
        var request = new HttpRequestMessage(metodo, ruta);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private async Task<string> LeerContenidoExitosoAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var contenido = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return contenido;
        }

        _logger.LogWarning(
            "La API de autorizaciones respondió {StatusCode}. Detalle: {Detalle}",
            (int)response.StatusCode,
            contenido);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
        }

        throw new NegocioException(
            string.IsNullOrWhiteSpace(contenido)
                ? $"La API de autorizaciones respondió {(int)response.StatusCode}."
                : contenido);
    }

    private void ValidarCreacion(
        CrearAutorizacionRemotaSolicitud solicitud,
        string idempotencyKey,
        string token)
    {
        ArgumentNullException.ThrowIfNull(solicitud);
        ValidarToken(token);

        if (string.IsNullOrWhiteSpace(_options.CodigoModuloOrigen))
        {
            throw new InvalidOperationException(
                "Debe configurarse AutorizacionRemota:CodigoModuloOrigen.");
        }

        if (_options.CodigoModuloOrigen.Length > 50)
        {
            throw new InvalidOperationException(
                "AutorizacionRemota:CodigoModuloOrigen admite hasta 50 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(solicitud.UsuarioSolicitante))
        {
            throw new ArgumentException("El usuario solicitante es requerido.", nameof(solicitud));
        }

        if (solicitud.UsuarioSolicitante.Length > 10)
        {
            throw new ArgumentException(
                "El usuario solicitante admite hasta 10 caracteres.",
                nameof(solicitud));
        }

        if (string.IsNullOrWhiteSpace(solicitud.IdSolicitudExterna))
        {
            throw new ArgumentException("El identificador externo es requerido.", nameof(solicitud));
        }

        if (solicitud.IdSolicitudExterna.Length > 100)
        {
            throw new ArgumentException(
                "El identificador externo admite hasta 100 caracteres.",
                nameof(solicitud));
        }

        if (solicitud.DerCodigo is < 1 or > short.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(solicitud), "El derecho es inválido.");
        }

        if (solicitud.TimeoutSegundos <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(solicitud), "El tiempo de espera debe ser mayor que cero.");
        }

        if (solicitud.DecisionPorDefecto is not ("APROBADO" or "RECHAZADO"))
        {
            throw new ArgumentException(
                "La decisión por defecto debe ser APROBADO o RECHAZADO.",
                nameof(solicitud));
        }

        if (string.IsNullOrWhiteSpace(solicitud.CodigoResolucionPorDefecto) ||
            solicitud.CodigoResolucionPorDefecto.Length > 50)
        {
            throw new ArgumentException(
                "El código de resolución por defecto es requerido y admite hasta 50 caracteres.",
                nameof(solicitud));
        }

        if (solicitud.MensajeResolucionPorDefecto?.Length > 500)
        {
            throw new ArgumentException(
                "El mensaje de resolución por defecto admite hasta 500 caracteres.",
                nameof(solicitud));
        }

        if (solicitud.Contexto is null)
        {
            throw new ArgumentException("El contexto es requerido.", nameof(solicitud));
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
        {
            throw new ArgumentException(
                "La clave de idempotencia es requerida y admite hasta 100 caracteres.",
                nameof(idempotencyKey));
        }
    }

    private static void ValidarToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
        }
    }
}
