using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

namespace gc.notificacion.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private ILoggerHelper _logger;
        private readonly ClaveSettings _settings;

        public NotifyController(ILoggerHelper logger,IOptions<ClaveSettings> options)
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
        [Route(template: "[action]/orden/mp")]
        public async Task<IActionResult> Notificar()
        {
            try
            {
                //se rescatarán los siguientes valores:
                //de QueryString => data.id  identificador del evento
                //del Header => x-signature valor de "ts"
                //                          valor de "v1"
                //              x-request-id su valor.
                string dataId, signa, ts, vs, rqId, qType;
                dataId = signa = ts = vs = rqId = qType = string.Empty;
                string[] arre;

                //se procede a recuperar los datos enviados por MePa. Si alguno de los datos no existe, se procederá a desechar y desestimar el informe.
                try { dataId = HttpContext.Request.Query["data.id"];
                    _logger.Log(TraceEventType.Information, $"data.id: {dataId}");
                } catch { throw new Exception("No se encontró 'data.id'."); }
                try
                {
                    signa = HttpContext.Request.Headers["x-signature"];
                    _logger.Log(TraceEventType.Information, $"x-signature: {signa}");

                    arre = signa.Split(new char[] { ',' }, StringSplitOptions.None);
                    ts = arre[0].Replace("ts=","");
                    vs = arre[1].Replace("v1=","");
                }
                catch { throw new Exception("No se encontró 'x-signature'. "); }

                try { rqId = HttpContext.Request.Headers["x-request-id"]; 
                    _logger.Log(TraceEventType.Information, $"x-request-id: {signa}");
                }
                catch { throw new Exception("No se encontró 'x-request-id'. "); }

                string mensaje = $"id:{dataId};request-id:{rqId};ts:{ts};";


                _logger.Log(TraceEventType.Information, $"Mensaje: {mensaje}");
                var hmac = HelperGen.ObtenerHMACtoHex(mensaje, _settings.Key);


                if (!hmac.Equals(vs))
                {
                    throw new Exception("La validación no es valida ");
                }
               


                string contenido = string.Empty;
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    contenido = await reader.ReadToEndAsync();
                    _logger.Log(TraceEventType.Information, contenido);
                }
                //await HttpContext.Response.WriteAsync($"El contenido del Post es: {contenido}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Log(TraceEventType.Warning, "Invocación WEBHOOKS DESESTIMADA");
                _logger.Log(ex);
                _logger.Log(TraceEventType.Warning, "QueryStrings");
                foreach (var item in HttpContext.Request.Query)
                {
                    _logger.Log(TraceEventType.Warning, $"{item.Key} - {item.Value}");
                }
                _logger.Log(TraceEventType.Warning, "Headers");

                foreach (var item in HttpContext.Request.Headers)
                {
                    _logger.Log(TraceEventType.Warning, $"{item.Key} - {item.Value}");
                }

                return Conflict();
            }
        }
    }
}
