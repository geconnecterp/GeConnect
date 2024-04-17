using gc.infraestructura.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace gc.notificacion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        private ILoggerHelper _logger;

        public InfoController(ILoggerHelper  logger)
        {
            _logger = logger;   
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route(template:"[action]/Orden/{ordenId}/ident")]
        public IActionResult Notificar(int ordenId)
        {
            return Ok(ordenId);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route(template: "orden/{ordenId}/mp")]
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
