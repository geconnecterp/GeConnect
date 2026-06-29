using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.caja.core.Servicios.Implementacion.Cajas
{
    public class CtaCteServicio : Servicio<Dto>, ICtaCteServicio
    {
        private const string RutaAPI = "/api/ApiPagoFactura";

        public const string GET_OBTENER_CTA_CTE = "/ObtenerCtaCte";

        public CtaCteServicio(IOptions<AppSettings> options, ILogger<CtaCteServicio> logger) : base(options, logger)
        {
        }

        public async Task<RespuestaGenerica<CtaCteResponseDto>> ObtenerCtaCte(string cta_id, string adm_id,string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_OBTENER_CTA_CTE}?cta_id={cta_id}&adm_id={adm_id}";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la búsqueda de cuenta" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<CtaCteResponseDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    return new RespuestaGenerica<CtaCteResponseDto>
                    {
                        Ok = true,
                        ListaEntidad = apiResponse.Data,
                        Mensaje = "OK"
                    };
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
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<CtaCteResponseDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al buscar las cuenta corriente del cliente"
                };
            }
        }
    }
}
