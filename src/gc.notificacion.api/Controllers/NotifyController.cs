using gc.infraestructura.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

namespace gc.notificacion.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private ILoggerHelper _logger;

        public NotifyController(ILoggerHelper logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route(template: "[action]/orden/{ordenId}/mp")]
        public IActionResult Notificar(int ordenId)
        {
            return Ok(ordenId);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route(template:"orden/{ordenId}/mp")]
        public async Task<IActionResult> Notificar2(int ordenId)
        {
            _logger.Log(TraceEventType.Information, $"Orden: {ordenId}");
            string contenido = string.Empty;
            using (var reader = new StreamReader(HttpContext.Request.Body))
            {
                contenido = await reader.ReadToEndAsync();
                _logger.Log(TraceEventType.Information, contenido);
            }
            //await HttpContext.Response.WriteAsync($"El contenido del Post es: {contenido}");
            return Ok(contenido);
        }
    }
}
