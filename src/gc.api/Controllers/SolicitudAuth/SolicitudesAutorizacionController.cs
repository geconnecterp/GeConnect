using gc.api.core.Contratos.Servicios.SolAuth;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.SolAuth.Comando;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RemoteAuthorizations.Application.Responses;
using System.Security.Claims;

namespace gc.api.Controllers.SolicitudAuth
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class SolicitudesAutorizacionController : ControllerBase
    {
        private readonly ISolicitudAuthServicio _servicio;
        private readonly ILogger<SolicitudesAutorizacionController> _logger;

        public SolicitudesAutorizacionController(ISolicitudAuthServicio servicio, ILogger<SolicitudesAutorizacionController> logger)
        {
            _servicio = servicio;
            _logger = logger;
        }


        [HttpPost]
        public async Task<ActionResult<RespuestaDto>> Crear(
                 [FromBody] CrearSolicitudAutorizacionComando comando,
                 [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
                 CancellationToken cancellationToken)
        {
            if (comando is null)
            {
                return BadRequest("La solicitud de autorizacion es requerida.");
            }

            if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
            {
                return BadRequest("El header Idempotency-Key es requerido y admite hasta 100 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(comando.CodigoModuloOrigen))
            {
                return BadRequest("CodigoModuloOrigen es requerido.");
            }

            comando.usu_id = ObtenerUsuarioAutenticado();
            comando.CodigoModuloOrigen = comando.CodigoModuloOrigen.Trim().ToUpperInvariant();

            try
            {
                _logger.LogInformation(
                    "Creando solicitud de autorizacion remota. Usuario={Usuario}; Modulo={Modulo}; Derecho={DerCodigo}; Externa={IdSolicitudExterna}; IdempotencyKey={IdempotencyKey}",
                    comando.usu_id,
                    comando.CodigoModuloOrigen,
                    comando.DerCodigo,
                    comando.IdSolicitudExterna,
                    idempotencyKey);

                var result = await _servicio.CrearAsync(
                    comando,
                    idempotencyKey,
                    comando.CodigoModuloOrigen,
                    cancellationToken);

                return Ok(new RespuestaDto
                {
                    resultado = 0,
                    resultado_msj = "Solicitud cargada correctamente",
                    IdFile = result.Id,
                    hoy = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "No se pudo crear la solicitud de autorizacion remota. Usuario={Usuario}; Modulo={Modulo}; Derecho={DerCodigo}; Externa={IdSolicitudExterna}; IdempotencyKey={IdempotencyKey}",
                    comando.usu_id,
                    comando.CodigoModuloOrigen,
                    comando.DerCodigo,
                    comando.IdSolicitudExterna,
                    idempotencyKey);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new RespuestaDto
                    {
                        resultado = -1,
                        resultado_msj = "No se pudo crear la solicitud de autorizacion remota.",
                        hoy = DateTime.UtcNow
                    });
            }
        }

        [HttpPost("{idSolicitud:guid}/resolucion")]
        public async Task<ActionResult<ResolucionAutorizacionRespuesta>> Resolver(
            Guid idSolicitud,
            [FromBody] ResolverSolicitudAutorizacionComando comando,
            [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
            CancellationToken cancellationToken)
        {
            string idUsuarioResolucion = ObtenerUsuarioAutenticado();

            var result = await _servicio.ResolverAsync(
                idSolicitud,
                comando,
                idempotencyKey,
                idUsuarioResolucion,
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("{idSolicitud:guid}/bloqueo")]
        public async Task<ActionResult> Bloquear(
            Guid idSolicitud,
            CancellationToken cancellationToken)
        {
            string idUsuario = ObtenerUsuarioAutenticado();

            await _servicio.BloquearAsync(
                idSolicitud,
                idUsuario,
                cancellationToken);

            return Ok();
        }

        [HttpGet("{idSolicitud:guid}/resolucion")]
        public async Task<ActionResult<SolicitudAutorizacionRespuesta>> ObtenerResolucion(
            Guid idSolicitud,
            CancellationToken cancellationToken)
        {
            string idUsuario = ObtenerUsuarioAutenticado();

            var result = await _servicio.ObtenerResolucionAsync(
                idSolicitud,
                idUsuario,
                cancellationToken);

            var esSolicitante = string.Equals(
                result.IdUsuarioSolicitante,
                idUsuario,
                StringComparison.OrdinalIgnoreCase);
            if (!esSolicitante && !result.PuedeAutorizar)
            {
                return Forbid();
            }

            return Ok(result);
        }

        [HttpGet("pendientes")]
        public async Task<ActionResult<IEnumerable<SolicitudAutorizacionRespuesta>>> ObtenerPendientes(
            CancellationToken cancellationToken)
        {
            string idUsuario = ObtenerUsuarioAutenticado();
            var result = await _servicio.ObtenerPendientesAsync(idUsuario, cancellationToken);
            return Ok(result);
        }

        [HttpGet("historico")]
        public async Task<ActionResult<IEnumerable<SolicitudAutorizacionRespuesta>>> ObtenerHistorico(
            [FromServices] Microsoft.Extensions.Configuration.IConfiguration configuration,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            CancellationToken cancellationToken)
        {
            var top = configuration.GetValue<int>("MaxHistoricalResults", 200);
            string idUsuario = ObtenerUsuarioAutenticado();

            var desde = (fechaDesde ?? DateTime.UtcNow).Date;
            var hasta = (fechaHasta ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);

            var result = await _servicio.ObtenerHistoricoAsync(desde, hasta, top, idUsuario, cancellationToken);
            return Ok(result);
        }

        private string ObtenerUsuarioAutenticado()
        {
            var usuario = User.FindFirst("user")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                throw new UnauthorizedAccessException(
                    "El token autenticado no contiene la identidad del usuario.");
            }

            return usuario;
        }
    }
}
