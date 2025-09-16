using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class OfertaServicio : Servicio<Dto>, IOfertaServicio
    {
        private const string RutaAPI = "/api/apioferta";

        private const string CONOCER_ESTADO_OFERTA = "/conocer-estado-oferta";
        private const string BUSCAR_CANALES = "/buscar-canales";
        private const string ALTA_OFERTA = "/confirmacion-alta-oferta";

        public OfertaServicio(IOptions<AppSettings> options, ILogger<OfertaServicio> logger) : base(options, logger)
        {
        }

        public async Task<RespuestaGenerica<CanalDto>> BuscarCanales(string token)
        {
            try
            {
                ApiResponse<List<CanalDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_CANALES}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<CanalDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<CanalDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<CanalDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los canales" };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> ConfirmacionAltaOferta(AbmPlusGenDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{ALTA_OFERTA}";

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "No se recibió respuesta válida de la API"
                        };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);

                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "Error deserializando la respuesta de la API"
                        };
                    }

                    if (apiResponse.Data.resultado != 0)
                    {
                        if (apiResponse.Data.resultado > 0)
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsWarn=true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando el alta de la oferta."
                            };
                        }
                        else
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsError = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando el alta de la oferta."
                            };
                        }
                    }
                    else
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = true,
                            Entidad = apiResponse?.Data ?? new RespuestaDto(),
                            Mensaje = "Importación procesada exitosamente"
                        };
                    }
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(errorData);
                    var mensaje = error?.Detail ?? "Error desconocido en la API";

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ConfirmacionAltaOferta");

                return new RespuestaGenerica<RespuestaDto>
                {
                    Ok = false,
                    Mensaje = "Error interno procesando el Alta de la Oferta"
                };
            }
        }

        public async Task<RespuestaGenerica<string>> ConocerEstadoOferta(string p_id, string admId, string lp_id, string token)
        {
            try
            {
                ApiResponse<string> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{CONOCER_ESTADO_OFERTA}?p_id={p_id}&admId={admId}&lp_id={lp_id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<string> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<string> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el estado de la oferta" };
            }
        }
    }
}
