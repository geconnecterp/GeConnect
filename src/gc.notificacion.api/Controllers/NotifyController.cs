using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.EntidadesComunes.Options;
using gc.notificacion.api.Modelo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace gc.notificacion.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private ILogger<NotifyController> _logger;
        private readonly ClaveSettings _settings;
        private readonly static string RutaBillOrden = "/api/billeteraOrden";
        private readonly static string ActionOrdenRegistro = "/OrdenNotificado";
        public NotifyController(ILogger<NotifyController> logger, IOptions<ClaveSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route(template: "orden/{orderId}/{hex}/{encrypt}/mp")]
        public async Task<IActionResult> Notificar(string orderId, string hex, string encrypt)
        {
            int errorCodigo = 0;
            try
            {
                //el token debera ser un usuario y clave especial para que se gestione el mismo automaticamente. Levantar de la cache
                //el token, verificar que no esta vencido. Si el mismo esta vencido se debe obtener un nuevo token. almacenarlo y usarlo
                //para invocar la api de servicios
                //por ahora token = ""
                string token = string.Empty;
                string plano = string.Empty;
                string desencrypt = string.Empty;
                string nroRecibo = string.Empty;
                string dataId = string.Empty;
                string qType = string.Empty;
                #region Conversión de datos HexToStr
                try
                {
                    //al recepcionar la notificacion se valida que sea la que hemos mandado
                    //1-orderId viene en hex. Convertirlo en string 
                    plano = HelperGen.ConvierteHexToStr(hex);
                    if(plano.Length>0)
                    {
                        nroRecibo = plano.Split('|')[0];
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en la conversión de Hex a Str");
                    errorCodigo = 1;
                    throw new Exception($"Se produjo un error en la conversión de datos propios. Error Cod:{errorCodigo}");
                }
                #endregion

                #region Desencryptación del texto encryptado
                try
                {
                    //validacion de la clave rsa
                    desencrypt = HelperGen.RSADesencrypFromHEX(encrypt, _settings.PathPrivateKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en la desencryptación de los datos");
                    errorCodigo = 2;
                    throw new Exception($"Se produjo un error en la conversión de datos propios. Error Cod:{errorCodigo}");
                }
                #endregion

                #region Armando Request de OrdenRegistro
                var reg = new OrdenNotificado { Orden_Id = orderId, Orden_Notificada_Ok='N', Orden_Id_Ext = " " };
                #endregion

                #region Validación de los datos recibidos en la notificación. comparacion del numero de recibo y los datos plano y desencrypt
                if (nroRecibo.Equals(orderId[..nroRecibo.Length]) && plano.Equals(desencrypt))
                {
                    _logger.Log(LogLevel.Information, "VERIFICACIÓN COMPROBADA!!!");
                    qType = HttpContext.Request.Query["type"].ToString();
                    if(string.IsNullOrEmpty(qType) )
                    {
                        qType = HttpContext.Request.Query["topic"].ToString();
                    }
                    dataId = HttpContext.Request.Query["data.id"].ToString();

                    switch (qType)
                    {
                        case "stop_delivery_op_wh":
                            //tiene que informar inmediatamente que la tarjeta o cuenta ha sido robada.
                            _logger.LogWarning($"Se recepciono una alerta de posible fraude. OrdenId: {orderId} - DataId: {dataId} ");
                            ActualizarOrdenEnBase(reg, token, _settings.RutaBaseServicios);
                            return Ok();
                        case "merchant_order":
                        case "payment":
                            reg.Orden_Notificada_Ok = 'S';
                            reg.Orden_Id_Ext = dataId;
                            var res = ActualizarOrdenEnBase(reg, token, _settings.RutaBaseServicios);
                            break;
                        default:
                            _logger.LogWarning($"La Notificación no fue ni Payment, ni merchant_order. El tipo de notificación fue: {qType}. OrdenId: {orderId} - DataId: {dataId} ");
                            ActualizarOrdenEnBase(reg, token, _settings.RutaBaseServicios);
                            return Ok();
                    }
                }
                else
                {
                    
                    _logger.Log(LogLevel.Warning, "No se verificó la autenticidad del mensaje");
                    reg.Orden_Notificada_Ok = 'N';
                    var res = ActualizarOrdenEnBase(reg, token, _settings.RutaBaseServicios);
                }
                #endregion
                //2-se toma el dato encryptado y se desencryptará. La misma se deberá comparar con el valor 

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error en la notificación");
            }

            return Ok();
        }

        private (bool, string) ActualizarOrdenEnBase(OrdenNotificado ordenNotifica, string token, string rutaBaseApi)
        {
            ApiResponse<(bool, string)> respuesta = null;
            HttpResponseMessage response;
            HelperAPI helperAPI = new HelperAPI();
            HttpClient client = helperAPI.InicializaCliente(ordenNotifica, token, out StringContent contentData);
            string link = $"{rutaBaseApi}{RutaBillOrden}{ActionOrdenRegistro}/{ordenNotifica.Orden_Id}";
            try
            {
                response = client.PutAsync(link, contentData).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respuesta = ExtraeContentRespuesta(respuesta, response).GetAwaiter().GetResult();

                return (respuesta.Data.Item1, respuesta.Data.Item2);
            }
            else
            {
                respuesta = ExtraeContentRespuesta(respuesta, response).GetAwaiter().GetResult();

                return (false, respuesta.Data.Item2);
            }

        }

        private static async Task<ApiResponse<(bool, string)>> ExtraeContentRespuesta(ApiResponse<(bool, string)> respuesta, HttpResponseMessage response)
        {
            string stringData = await response.Content.ReadAsStringAsync();

            respuesta = JsonSerializer.Deserialize<ApiResponse<(bool, string)>>(stringData);
            return respuesta;
        }

    }
}
