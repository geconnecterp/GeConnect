using gc.infraestructura.Core.Helpers;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace gc.notificacion.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private ILogger<NotifyController> _logger;
        private readonly ClaveSettings _settings;

        public NotifyController(ILogger<NotifyController> logger, IOptions<ClaveSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[Route(template: "[action]/orden/{ordenId}/mp")]
        //public IActionResult Notificar(int ordenId)
        //{
        //    return Ok(ordenId);
        //}

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route(template: "[action]/orden/{orderId}/{tiempo}/mp")]
        public async Task<IActionResult> Notificar(int orderId, long tiempo)
        {
            try
            {
                _logger.LogInformation("=============== NUEVA PETICIÓN ==================");
                _logger.LogInformation($"orderId: {orderId} - Tiempo: {tiempo} - {new DateTime(tiempo)}");

                _logger.LogInformation("Contenido Body Post");
                string contenido = string.Empty;
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    contenido = await reader.ReadToEndAsync();
                    _logger.LogInformation(contenido);
                }

                //se rescatarán los siguientes valores:
                //de QueryString => data.id  identificador del evento
                //del Header => x-signature valor de "ts"
                //                          valor de "v1"
                //              x-request-id su valor.
                string dataId, signa, ts, vs, rqId, qType;
                dataId = signa = ts = vs = rqId = qType = string.Empty;
                string[] arre;

                //se procede a recuperar los datos enviados por MePa. Si alguno de los datos no existe, se procederá a desechar y desestimar el informe.

                try
                {
                    var tipo = HttpContext.Request.Query["id"];

                    switch (tipo)
                    {
                        case "payment":
                            dataId = HttpContext.Request.Query["data.id"];
                            _logger.LogInformation($"data.id: {dataId}");
                            break;
                        default:
                            dataId = HttpContext.Request.Query["id"];
                            _logger.LogInformation($"id: {dataId}");
                            break;
                    }
                }
                catch
                {
                    throw new Exception("No se encontró 'data.id'.");
                }
                try
                {
                    signa = HttpContext.Request.Headers["x-signature"];
                    _logger.LogInformation($"x-signature: {signa}");

                    arre = signa.Split(new char[] { ',' }, StringSplitOptions.None);
                    ts = arre[0].Replace("ts=", "");
                    vs = arre[1].Replace("v1=", "");
                }
                catch { throw new Exception("No se encontró 'x-signature'. "); }

                try
                {
                    rqId = HttpContext.Request.Headers["x-request-id"];
                    _logger.LogInformation($"x-request-id: {rqId}");
                }
                catch { throw new Exception("No se encontró 'x-request-id'. "); }

                string mensaje = $"id:{dataId};request-id:{rqId};ts:{ts};";


                _logger.LogInformation($"Mensaje: {mensaje}");
                var hmac = HelperGen.ObtenerHMACtoHex(mensaje, _settings.Key);

                hmac = hmac.ToLower();
                if (!hmac.Equals(vs))
                {
                    throw new Exception("La validación no es valida ");
                }
                else
                {
                    //invocar a MP para obtener el detalle de la venta

                }




                //await HttpContext.Response.WriteAsync($"El contenido del Post es: {contenido}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invocación WEBHOOKS DESESTIMADA");
                _logger.LogWarning("QueryStrings");
                foreach (var item in HttpContext.Request.Query)
                {
                    _logger.LogWarning($"{item.Key} - {item.Value}");
                }
                _logger.LogWarning("Headers");

                foreach (var item in HttpContext.Request.Headers)
                {
                    _logger.LogWarning($"{item.Key} - {item.Value}");
                }

                return Ok();
            }
        }
    }
}
