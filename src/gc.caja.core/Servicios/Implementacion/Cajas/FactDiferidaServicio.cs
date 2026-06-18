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
    public class FactDiferidaServicio : Servicio<Dto>, IFactDiferidaServicio
    {
        private const string RutaAPI = "/api/apipagofactura";

        private const string POST_OBTENER_FACT_DIFE = "/ObtenerFacturasPendientes";

        public FactDiferidaServicio(IOptions<AppSettings> options,
           ILogger<FactDiferidaServicio> logger) : base(options, logger)
        {
        }

        /// <summary>
        /// Metodo para obtener las facturas pendientes de pago diferido para un cliente específico, utilizando la información proporcionada en el request. 
        /// Se comunica con una API externa y maneja la respuesta para devolver una lista de facturas pendientes o un mensaje de error si no se encuentran 
        /// o si ocurre algún problema durante la comunicación.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<RespuestaGenerica<FactPendienteResponseDto>> ObtenerFacturasPendientes(FactPendienteRequestDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_OBTENER_FACT_DIFE}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name} - 01 - Error deserializando la respuesta de la API");
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FactPendienteResponseDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name} - 02 - Error deserializando la respuesta de la API");
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }
                    var resp = apiResponse.Data;
                    if (resp != null && resp.Any())
                    {
                        return new RespuestaGenerica<FactPendienteResponseDto>
                        {
                            Ok = true,
                            Mensaje = "OK",
                            ListaEntidad = apiResponse.Data
                        };
                    }

                    else
                    {
                        return new RespuestaGenerica<FactPendienteResponseDto>
                        {
                            Ok = false,
                            EsWarn = true,
                            EsError = false,
                            Mensaje = "No se encontraron facturas pendientes para el cliente.",
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
                return new() { Ok = false, Mensaje = "Hubo un error al intentar Finalizar la compra." };
            }
        }
    }
}
