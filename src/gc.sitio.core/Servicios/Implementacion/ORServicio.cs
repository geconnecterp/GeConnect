

using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenReparto;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using gc.infraestructura.Dtos.Productos.PromoCombo;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class ORServicio : Servicio<Dto>, IORServicio
    {
        private const string RutaAPI = "/api/ApiOR";
        private const string RutaAPI02 = "/api/administracion";

        private const string POST_OBTENER_OR = "/ObtenerOrdenesReparto";
        private const string GET_VALIDA_USU = "/TIValidarUsuario";
        public ORServicio(IOptions<AppSettings> options,ILogger<ORServicio> logger):base(options,logger,RutaAPI)
        {
            
        }
        public async  Task<RespuestaGenerica<OrdenRepartoListDto>> ObtenerOrdenesReparto(ORRequestDto request, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(request, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_OBTENER_OR}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OrdenRepartoListDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    return new RespuestaGenerica<OrdenRepartoListDto>
                    {
                        Ok = true,
                        Mensaje = "OK",
                        ListaEntidad = apiResponse.Data
                        // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
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
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al buscar las Etiquetas" };
            }
        }

        public async Task<RespuestaGenerica<ResponseBaseDto>> ValidarUsuario( string id, string usuId, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI02}{GET_VALIDA_USU}?tipo=OR&id={id}&usu={usuId}";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la validación del Usuario" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ResponseBaseDto>>(stringData)
                        ?? throw new NegocioException("Error al deserializar los datos");

                    if (apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "No se encontraron datos validación del Usuario." };
                    }

                    return new RespuestaGenerica<ResponseBaseDto>
                    {
                        Ok = true,
                        Entidad = apiResponse.Data,
                        Mensaje = "OK"
                    };
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    return new()
                    {
                        Ok = false,
                        Mensaje = "Error al obtener los estados de combos. Si el problema persiste contacte al administrador."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<ResponseBaseDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al validar el usuario"
                };
            }
        }
    }
}
