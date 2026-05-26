using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Reflection;

namespace gc.caja.core.Servicios.Implementacion.Cajas
{
    public class CheckoutServicio : Servicio<Dto>, ICheckoutServicio
    {
        private const string RutaAPI = "/api/apipagofactura";

        private const string POST_OBTENER_VALORES_INS = "/ObtenerValoresIns";
        private const string POST_OBTENER_VALORES_MP = "/ObtenerValoresMP";
        private const string POST_OBTENER_VALORES_NC = "/ObtenerValoresNC";
        private const string POST_OBTENER_VALORES_PENDIENTES = "/ObtenerValoresPendientes";

        // ✅ NUEVAS CONSTANTES - FASE 1: VALORES DIRECTOS
        private const string POST_AGREGAR_VALOR_MANUAL = "/AgregarValorManual";
        private const string POST_FINALIZAR_PAGO = "/ConfirmarOperacionCaja";

        public CheckoutServicio(IOptions<AppSettings> options,
            ILogger<CheckoutServicio> logger) : base(options, logger)
        {
        }


        public async Task<RespuestaGenerica<ValoresInsResDto>> ObtenerValoresIns(ValoresInsReqDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_OBTENER_VALORES_INS}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ValoresInsResDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    var resp = apiResponse.Data;
                    if (!resp.Any())
                    {
                        var def = new ValoresInsResDto
                        {
                            ins_id = "DEF",
                            ins_desc = "Sin instrumentos disponibles",
                            mon_codigo = "DEF",
                            ins_detalle = "N",
                            tcf_id = req.tcf_id,
                            ins_tiene_vto = "N",
                            ins_arqueo = "N",
                            ins_vuelto = "N",
                            ins_vigente = "N",
                            ins_comision = 0,
                            ins_comision_fija = 0,
                            ins_ret_gan = 0,
                            ins_ret_ib = 0,
                            ins_ret_iva = 0
                        };
                        return new() { Ok = true, Mensaje = "Instrumento único.", ListaEntidad = [ def ] };
                    }
                    else
                    {
                        return new RespuestaGenerica<ValoresInsResDto>
                        {
                            Ok = true,
                            Mensaje = "OK",
                            ListaEntidad = apiResponse.Data
                        };
                    }
                }
                else
                {
                    var msg = await ReadApiErrorAsync(response);
                    _logger.LogWarning($"Error API ({response.StatusCode}): {msg}");
                    return new() { Ok = false, Mensaje = msg };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener los datos del producto" };
            }
        }

        public async Task<RespuestaGenerica<ValoresMPResDto>> ObtenerValoresMP(ValoresMPReqDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_OBTENER_VALORES_MP}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ValoresMPResDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    var resp = apiResponse.Data;
                    if (!resp.Any())
                    {
                        return new() { Ok = false, Mensaje = "No se encontraron productos según el criterio." };
                    }
                    else
                    {
                        return new RespuestaGenerica<ValoresMPResDto>
                        {
                            Ok = true,
                            Mensaje = "OK",
                            ListaEntidad = apiResponse.Data
                        };
                    }
                }
                else
                {
                    var msg = await ReadApiErrorAsync(response);
                    _logger.LogWarning($"Error API ({response.StatusCode}): {msg}");
                    return new() { Ok = false, Mensaje = msg };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener los datos del producto" };
            }
        }

        // ✅ FASE 1: VALORES DIRECTOS - Método para obtener valores o instrumentos de nota de crédito, que son a favor del cliente
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req">se recepciona el tipo de operacion (co_tipo) y el id del cliente (cta_id)</param>
        /// <returns></returns>
        public async Task<RespuestaGenerica<ValoresNCResDto>> ObtenerValoresNC(ValoresNCReqDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_OBTENER_VALORES_NC}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ValoresNCResDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    var resp = apiResponse.Data;
                    if (!resp.Any())
                    {
                        return new() { Ok = false, Mensaje = "No se encontraron productos según el criterio." };
                    }
                    else
                    {
                        return new RespuestaGenerica<ValoresNCResDto>
                        {
                            Ok = true,
                            Mensaje = "OK",
                            ListaEntidad = apiResponse.Data
                        };
                    }
                }
                else
                {
                    var msg = await ReadApiErrorAsync(response);
                    _logger.LogWarning($"Error API ({response.StatusCode}): {msg}");
                    return new() { Ok = false, Mensaje = msg };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener los datos del producto" };
            }
        }

        // ✅ FASE 1: VALORES DIRECTOS - Método para obtener valores o instrumentos pendientes (ejemplo post de tarjetas)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req">se recepciona el tipo de operacion (co_tipo) y el id del cliente (cta_id)</param>
        /// <returns></returns>
        public async Task<RespuestaGenerica<ValoresPendientesResDto>> ObtenerValoresPendientes(ValoresPendientesReqDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_OBTENER_VALORES_PENDIENTES}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ValoresPendientesResDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    var resp = apiResponse.Data;
                    if (!resp.Any())
                    {
                        return new() { Ok = false, Mensaje = "No se encontraron productos según el criterio." };
                    }
                    else
                    {
                        return new RespuestaGenerica<ValoresPendientesResDto>
                        {
                            Ok = true,
                            Mensaje = "OK",
                            ListaEntidad = apiResponse.Data
                        };
                    }
                }
                else
                {
                    var msg = await ReadApiErrorAsync(response);
                    _logger.LogWarning($"Error API ({response.StatusCode}): {msg}");
                    return new() { Ok = false, Mensaje = msg };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener los datos del producto" };
            }
        }
    }
}
