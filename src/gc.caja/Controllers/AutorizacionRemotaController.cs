using gc.caja.Models;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.caja.Controllers;

[Authorize]
[Route("AutorizacionRemota")]
public sealed class AutorizacionRemotaController : ControladorBaseCaja
{
    private readonly IAutorizacionRemotaOrquestador _orquestador;

    public AutorizacionRemotaController(
        IOptions<AppSettings> options,
        IHttpContextAccessor contexto,
        ILogger<AutorizacionRemotaController> logger,
        IAutorizacionRemotaOrquestador orquestador)
        : base(options, contexto, logger)
    {
        _orquestador = orquestador;
    }

    [HttpGet("Estado/{claveOperacion}/{idSolicitud:guid}")]
    public async Task<IActionResult> Estado(
        string claveOperacion,
        Guid idSolicitud,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _orquestador.ConsultarAsync(
                claveOperacion,
                idSolicitud,
                TokenCookie,
                cancellationToken);

            if (!resultado.Vigente)
            {
                return Ok(new
                {
                    ok = true,
                    vigente = false,
                    estado = resultado.Estado,
                    terminal = true,
                    aprobada = false,
                    mensaje = "La solicitud fue reemplazada por una más reciente."
                });
            }

            var solicitud = resultado.Solicitud!;
            var resolucion = solicitud.Resolucion;
            var terminal = solicitud.Estado is "RESUELTO" or "EXPIRADO";
            if (terminal && resolucion is null)
            {
                _logger?.LogWarning(
                    "La solicitud {IdSolicitud} está en estado {Estado}, pero la API no devolvió su resolución.",
                    solicitud.Id,
                    solicitud.Estado);

                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        ok = false,
                        mensaje = "La resolución todavía no está disponible. Se volverá a consultar."
                    });
            }

            var aprobada = terminal &&
                string.Equals(resolucion?.Decision, "APROBADO", StringComparison.OrdinalIgnoreCase);

            return Ok(new
            {
                ok = true,
                vigente = true,
                idSolicitud = solicitud.Id,
                estado = solicitud.Estado,
                terminal,
                aprobada,
                decision = resolucion?.Decision,
                codigoResolucion = resolucion?.CodigoResolucion,
                mensaje = resolucion?.Mensaje,
                usuarioResolucion = resolucion?.IdUsuarioResolucion,
                fechaResolucion = resolucion?.FechaResolucion,
                fechaExpiracion = solicitud.FechaExpiracion
            });
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { ok = false, mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ok = false, mensaje = ex.Message });
        }
        catch (NegocioException ex)
        {
            _logger?.LogWarning(ex, "No se pudo consultar la autorización {IdSolicitud}.", idSolicitud);
            return StatusCode(StatusCodes.Status502BadGateway, new { ok = false, mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error consultando la autorización {IdSolicitud}.", idSolicitud);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { ok = false, mensaje = "No se pudo consultar la autorización remota." });
        }
    }
}
