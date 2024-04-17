using gc.infraestructura.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace gc.notificacion.Controllers
{
    public class NotificacionController : Controller
    {
        private readonly ILoggerHelper _logger;

        public NotificacionController(ILoggerHelper logger)
        {
                _logger = logger;
        }

        [HttpGet]
        //[Route("/orden/{int:ordenId}")]
        public string Notify(int ordenId)
        {
            return ordenId.ToString();
        }

        [HttpPost]
        public async Task<IActionResult> NotifyFromMP(int ordenId)
        {
            _logger.Log(TraceEventType.Information,$"Orden: {ordenId}");
            string contenido = string.Empty;
            using(var reader = new StreamReader(HttpContext.Request.Body))
            {
                contenido = await reader.ReadToEndAsync();
                _logger.Log(TraceEventType.Information, contenido);
            }
            //await HttpContext.Response.WriteAsync($"El contenido del Post es: {contenido}");
            return Ok(contenido);
        }
    }
}
