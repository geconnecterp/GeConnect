using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Billeteras;
using gc.infraestructura.EntidadesComunes.Options;
using gc.notificacion.api.Modelo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Net;

namespace gc.notificacion.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private ILogger<NotifyController> _logger;
        private readonly ClaveSettings _settings;
        private readonly static string RutaBillOrden = "/api/billeteraOrden";
        private readonly static string ActionOrdenNotificado = "/OrdenNotificado";
        public NotifyController(ILogger<NotifyController> logger, IOptions<ClaveSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Test(string id)
        {
            return Ok(id);
        }

        //[HttpPost]
        //[ProducesResponseType((int)HttpStatusCode.OK)]
        //[Route(template:"/ord/{ord}")]
        //public IActionResult TestPost(string ord)
        //{
        //    return Ok(ord);
        //}

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [Route("{orden}/mp")]
        public IActionResult Notificar(string orden)
        {
            int errorCodigo = 0;
            try
            {
                #region Se logea los headers
                //_logger.LogInformation("Headers");

                //foreach (var item in HttpContext.Request.Headers)
                //{
                //    string message = $"{item.Key}-{item.Value}";
                //    _logger.LogWarning(message);
                //}
                #endregion

                #region Obteniendo datos del POST
                _logger.LogInformation("Contenido Body Post");
                string contenido = string.Empty;
                using (var reader = new StreamReader(HttpContext.Request.Body))
                {
                    contenido = reader.ReadToEndAsync().GetAwaiter().GetResult();
                    _logger.LogInformation(contenido);
                }
                #endregion

                //el token debera ser un usuario y clave especial para que se gestione el mismo automaticamente. Levantar de la cache
                //el token, verificar que no esta vencido. Si el mismo esta vencido se debe obtener un nuevo token. almacenarlo y usarlo
                //para invocar la api de servicios
                //por ahora token = ""
                string token = string.Empty;
                string plano = string.Empty;
                string orden_id = string.Empty;
                string desencrypt = string.Empty;
                string nroRecibo = string.Empty;
                string dataId = string.Empty;
                string qType = string.Empty;
                string ord = string.Empty;
                string hex = string.Empty;
                string enc = string.Empty;
                #region Conversión de datos HexToStr
                try
                {
                    _logger.LogInformation("recuperando datos");

                    #region recuperando datos
                    int lg1 = orden.Substring(0, 4).ToInt();
                    int lg2 = orden.Substring(4, 4).ToInt();

                    ord = orden.Substring(8, lg1);
                    hex = orden.Substring(8 + lg1);

                    #endregion

                    //se obtiene el bo_id
                    orden_id = HelperGen.ConvierteHexToStr(ord);
                    _logger.LogInformation("se obtuvo orden_id");

                    #region Se busca de BilleteraOrden el registro de la orden.
                    var billOrden = ObtenerDatosPorBoId(orden_id, "", _settings.RutaBaseServicios).GetAwaiter().GetResult();
                    _logger.LogInformation("Se obtiene el registro de la billetera");

                    enc = billOrden.Bo_Clave;
                    #endregion

                    //al recepcionar la notificacion se valida que sea la que hemos mandado
                    //1-orderId viene en hex. Convertirlo en string 
                    _logger.LogInformation("se va recuperar el valor de hex");

                    plano = HelperGen.ConvierteHexToStr(hex);
                    if (plano.Length > 0)
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
                    desencrypt = HelperGen.RSADesencrypFromHEX(enc, _settings.PathPrivateKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en la desencryptación de los datos");
                    errorCodigo = 2;
                    throw new Exception($"Se produjo un error en la conversión de datos propios. Error Cod:{errorCodigo}");
                }
                #endregion

                #region Armando Request de OrdenRegistro
                var reg = new OrdenNotificado { Orden_Id = orden_id, Orden_Notificada_Ok = 'N', Orden_Id_Ext = " " };
                #endregion

                #region Validación de los datos recibidos en la notificación. comparacion del numero de recibo y los datos plano y desencrypt
                if (nroRecibo.Equals(orden_id[..nroRecibo.Length]) && plano.Equals(desencrypt))
                {
                    _logger.Log(LogLevel.Information, "VERIFICACIÓN COMPROBADA!!!");

                    //debo mirar solos los topic=merchant_order


                    qType = HttpContext.Request.Query["topic"].ToString();
                    dataId = HttpContext.Request.Query["id"].ToString();


                    if (!string.IsNullOrEmpty(qType.Trim()) && !string.IsNullOrEmpty(dataId.Trim()))
                    {
                        switch (qType)
                        {
                            case "stop_delivery_op_wh":
                                //tiene que informar inmediatamente que la tarjeta o cuenta ha sido robada.
                                _logger.LogWarning($"Se recepciono una alerta de posible fraude. OrdenId: {ord} - DataId: {dataId} ");
                                ActualizarOrdenEnBase(reg, token, _settings.RutaBaseServicios);
                                return Ok();
                            case "merchant_order":
                            case "payment":
                                reg.Orden_Notificada_Ok = 'S';
                                reg.Orden_Id_Ext = dataId;
                                _logger.LogInformation($"Se recepcionó el pago {dataId}.");

                                try
                                {
                                    var billetera = ObtenerDatosBilletera("MP");
                                    //obtener datos de la venta 
                                    var resMepa = VerificaInformacionPagoMepa(dataId, billetera.Bill_User_Id, billetera.Bill_Ruta_Base, billetera.Bill_Token);
                                    if (resMepa.Item1)
                                    {
                                        if (qType.Equals("payment"))
                                        {
                                            //solo informaremos para que tenga en cuenta el pago
                                            reg.Status = "payment";
                                        }
                                        else
                                        {
                                            reg.ResponseMepa = resMepa.Item2.Item1; //resguardamos el stringData (Json recepcionado desde MePa)
                                            //son merchant_order
                                            reg.Status = resMepa.Item2.Item2; //obtenemos el status de la operacion de orden merchant_order
                                        }
                                       
                                        var res = ActualizarOrdenEnBase(reg, token, _settings.RutaBaseServicios);
                                        if (reg.Status.Equals("closed"))
                                        {
                                            reg.Status = "ok";    
                                            ActualizarOrdenEnBase(reg, token, _settings.RutaBaseServicios);
                                        }
                                    }
                                }
                                catch (Exception )
                                {
                                    throw;
                                }
                                break;
                            default:
                                _logger.LogWarning($"La Notificación no fue ni Payment, ni merchant_order. El tipo de notificación fue: {qType}. OrdenId: {ord} - DataId: {dataId} ");
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error en la notificación");
            }

            return Ok();
        }

        private BilleteraDto ObtenerDatosBilletera(string bill)
        {
            ApiResponse<BilleteraDto>? response;
            HelperAPI helper = new HelperAPI();
            HttpClient client = helper.InicializaCliente("");
            HttpResponseMessage resp;
            string link = $"{_settings.RutaBaseServicios}/api/billetera/{bill}";
            resp = client.GetAsync(link).GetAwaiter().GetResult();
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                string dataString = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                response = JsonConvert.DeserializeObject<ApiResponse<BilleteraDto>>(dataString);
                if(response == null)
                {
                    throw new NegocioException("Hubo un problema con la deserialización de los datos de la billetera");
                }

                return response.Data;
            }
            else
            {
                throw new Exception("No se pudo obtener los datos base de la billetera.");
            }
        }

        private (bool, (string, string)) VerificaInformacionPagoMepa(string dataId, string userId, string rutaBase, string token)
        {

            HelperAPI helper = new HelperAPI();
            HttpClient client = helper.InicializaCliente(token);
            string link = $"{rutaBase}/merchant_orders/{dataId}";
            HttpResponseMessage response = client.GetAsync(link).GetAwaiter().GetResult();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                //se recepcionan los datos. 
                string dataString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                PagoMepaDto pago = JsonConvert.DeserializeObject<PagoMepaDto>(dataString)
                    ?? throw new NegocioException("Hubo un problema con la deserialización de los datos de MePa") ;


                _logger.LogInformation(JsonConvert.SerializeObject(dataString));
                return (true, (dataString, pago.status));
            }
            return (false, (string.Empty, string.Empty));
        }

        private async Task<BilleteraOrdenDto> ObtenerDatosPorBoId(string bo_id, string token, string rutaBaseApi)
        {
            HttpResponseMessage response;
            HelperAPI helperAPI = new HelperAPI();
            HttpClient client = helperAPI.InicializaCliente(token);
            string link = $"{rutaBaseApi}{RutaBillOrden}/{bo_id}";
            try
            {
                response = await client.GetAsync(link);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al intentar obtener el registro de la orden {bo_id}");
                throw;
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                var respuesta = JsonConvert.DeserializeObject<ApiResponse<BilleteraOrdenDto>>(stringData);
                //respuesta = ExtraeContentRespuesta<ApiResponse<BilleteraOrdenDto>>(response).GetAwaiter().GetResult();
                if(respuesta == null)
                {
                    throw new NegocioException("Hubo un problema con la deserialización de los datos de la billetera");
                }
                return respuesta.Data;
            }
            else
            {
                var error = ExtraeContentRespuesta<ExceptionValidation>(response).GetAwaiter().GetResult();
                _logger.LogError($"{error.Status}-{error.Title}:{error.Detail}");

                return new();
            }
        }

        private (bool, string) ActualizarOrdenEnBase(OrdenNotificado ordenNotifica, string token, string rutaBaseApi)
        {
            _logger.LogInformation($"Por actualizar notificacion-{JsonConvert.SerializeObject(ordenNotifica)}");
            ApiResponse<(bool, string)> respuesta;
            HttpResponseMessage response;
            HelperAPI helperAPI = new();
            HttpClient client = helperAPI.InicializaCliente(ordenNotifica, token, out StringContent contentData);
            string link = $"{rutaBaseApi}{RutaBillOrden}{ActionOrdenNotificado}/{ordenNotifica.Orden_Id}";

            response = client.PutAsync(link, contentData).GetAwaiter().GetResult();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respuesta = ExtraeContentRespuesta<ApiResponse<(bool, string)>>(response).GetAwaiter().GetResult();

                return (respuesta.Data.Item1, respuesta.Data.Item2);
            }
            else
            {
                var error = ExtraeContentRespuesta<ExceptionValidation>(response).GetAwaiter().GetResult();

                return (false, $"{error.Status}-{error.Title}:{error.Detail}");
            }

        }

        private async Task<T> ExtraeContentRespuesta<T>(HttpResponseMessage response)
        {
            string stringData = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(stringData);
            T respuesta = JsonConvert.DeserializeObject<T>(stringData) 
                ?? throw new NegocioException("Hubo un problema al deserializar datos del content");
            return respuesta;
        }
    }
}
