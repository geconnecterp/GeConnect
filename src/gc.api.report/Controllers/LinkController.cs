using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Codigos
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class LinkController : ControllerBase
    {
        private readonly ILogger<LinkController> _logger;
        private readonly ILinkServicio _linkServicio;
        private readonly IConfiguration _configuration;

        public LinkController(
            ILogger<LinkController> logger,
            ILinkServicio linkServicio,
            IConfiguration configuration)
        {
            _logger = logger;
            _linkServicio = linkServicio;
            _configuration = configuration;
        }

        /// <summary>
        /// ✅ Crea un enlace temporal para compartir reportes
        /// </summary>
        /// <param name="solicitud">Datos de la solicitud de reporte</param>
        /// <param name="clienteId">ID opcional del cliente (Header o parámetro)</param>
        /// <returns>Enlace generado con código único</returns>
        [HttpPost("CrearLink")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CrearLink([FromBody] LinkRequestDto request)
        {

            // Validación de entrada
            if (request == null)
            {
                _logger?.LogWarning("⚠️ Solicitud nula recibida en CrearLink");
                return BadRequest(new
                {
                    success = false,
                    message = "Solicitud de reporte requerida"
                });
            }

            _logger?.LogInformation("📡 Creando link para reporte - Cliente: {ClienteId}", request.ClienteId ?? "N/A");

            // Invocar servicio
            var res = _linkServicio.CrearLink(request.Solicitud, request.Usu_id, request.ClienteId);

            if (res == null)
            {
                _logger?.LogError("❌ Respuesta nula del servicio CrearLinkAsync");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al generar enlace"
                });
            }

            _logger?.LogInformation("✅ Link creado exitosamente - Código: {Codigo}", res.Codigo);

            return Ok(new ApiResponse<ReporteLinkResponseDto>(res));
        }

        /// <summary>
        /// ✅ Obtiene los datos de una solicitud mediante su código único
        /// </summary>
        /// <param name="codigo">Código único del enlace generado</param>
        /// <returns>Datos de la solicitud original</returns>
        [HttpGet("ObtenerSolicitud")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ObtenerSolicitud([FromQuery] string codigo)
        {

            // Validación de entrada
            if (string.IsNullOrWhiteSpace(codigo))
            {
                _logger?.LogWarning("⚠️ Código vacío en ObtenerSolicitud");
                return BadRequest(new
                {
                    success = false,
                    message = "Código de enlace requerido"
                });
            }

            _logger?.LogInformation("📡 Obteniendo solicitud - Código: {Codigo}", codigo);

            // Invocar servicio
            var contexto = new ReporteLinkAccesoContextoDto
            {
                Ip = Request.Headers["X-Geco-Client-IP"].FirstOrDefault()
                    ?? HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["X-Geco-User-Agent"].FirstOrDefault()
                    ?? Request.Headers.UserAgent.FirstOrDefault(),
                Referer = Request.Headers["X-Geco-Referer"].FirstOrDefault()
                    ?? Request.Headers.Referer.FirstOrDefault()
            };

            var res = _linkServicio.ObtenerSolicitud(codigo, contexto);

            _logger?.LogInformation("✅ Solicitud obtenida correctamente - Código: {Codigo}", codigo);

            if (!_configuration.GetValue(
                "Reportes:EnlacesPublicos:ControlDescargasHabilitado",
                false))
            {
                return Ok(new ApiResponse<ReporteSolicitudDto>(res.Solicitud));
            }

            return Ok(new ApiResponse<ReporteLinkAccesoResponseDto>(res));
        }

        [HttpPost("ConfirmarDescarga")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ConfirmarDescarga([FromBody] ReporteLinkDescargaDto descarga)
        {
            var res = _linkServicio.ConfirmarDescarga(descarga);
            return Ok(new ApiResponse<ReporteLinkOperacionResponseDto>(res));
        }

        [HttpPost("RegistrarFallo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult RegistrarFallo([FromBody] ReporteLinkDescargaDto descarga)
        {
            _linkServicio.RegistrarFallo(descarga);
            return Ok(new { success = true });
        }
    }
}
