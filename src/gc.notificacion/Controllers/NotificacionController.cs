using Microsoft.AspNetCore.Mvc;

namespace gc.notificacion.Controllers
{
    public class NotificacionController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public NotificacionController(ILogger<HomeController>  logger)
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
            _logger.Log(LogLevel.Trace,$"Orden: {ordenId}");
            string contenido = string.Empty;
            using(var reader = new StreamReader(HttpContext.Request.Body))
            {
                contenido = await reader.ReadToEndAsync();
                _logger.Log(LogLevel.Information, contenido);
            }
            //await HttpContext.Response.WriteAsync($"El contenido del Post es: {contenido}");
            return Ok(contenido);
        }
    }
}
