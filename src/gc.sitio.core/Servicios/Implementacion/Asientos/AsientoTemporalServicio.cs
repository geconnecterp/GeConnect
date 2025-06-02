using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.core.Servicios.Contratos.Asientos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion.Asientos
{
    public class AsientoTemporalServicio : Servicio<Dto>, IAsientoTemporalServicio
    {
        private const string RutaAPI = "/api/ApiAsientoTemporal";
        private const string POST_OBTENER_ASIENTOS = "/obtener-asientos";

        private readonly AppSettings _appSettings;
        private const string POST_PASAR_ASIENTOS = "/pasar-asientos-contabilidad";
        private const string GET_OBTENER_ASIENTO_DETALLE = "/obtener-asiento-detalle";

        public AsientoTemporalServicio(IOptions<AppSettings> options, ILogger<AsientoTemporalServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }

        /// <inheritdoc/>
        public async Task<(List<AsientoGridDto>,MetadataGrid)> ObtenerAsientos(QueryAsiento query, string token)
        {
            try
            {
                ApiResponse<List<AsientoGridDto>> apiResponse;
                HelperAPI helper = new();

                // Inicializar cliente y preparar contenido
                HttpClient client = helper.InicializaCliente(query, token, out StringContent contentData);
                HttpResponseMessage response;

                // Construir la URL de la API
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_OBTENER_ASIENTOS}";

                // Enviar solicitud POST
                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde." );
                    }

                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AsientoGridDto>>>(stringData)
                        ?? throw new NegocioException("No se pudo deserializar los datos de asientos temporales.");

                    return (apiResponse.Data ?? [],apiResponse.Meta??new());
                }else
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Hubo un problema en la recepcion de los asientos");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Hubo un problema en la recepcion de los asientos");
                    }
                    else
                    {
                        throw new Exception(error?.Detail);
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new NegocioException("No tiene permisos para acceder a este recurso.");
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Hubo un problema en la recepcion de los asientos");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Hubo un problema en la recepcion de los asientos");
                    }
                    else
                    {
                        throw new Exception(error?.Detail);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                throw new NegocioException("Algo no fue bien al intentar obtener los asientos temporales." );
            }
        }

        /// <inheritdoc/>
        public async Task<RespuestaGenerica<RespuestaDto>> PasarAsientosAContabilidad(AsientoPasaDto asientoPasa, string token)
        {
            try
            {
                ApiResponse<List<RespuestaDto>> apiResponse;
                HelperAPI helper = new();

                // Inicializar cliente y preparar contenido
                HttpClient client = helper.InicializaCliente(asientoPasa, token, out StringContent contentData);
                HttpResponseMessage response;

                // Construir la URL de la API
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_PASAR_ASIENTOS}";

                // Enviar solicitud POST
                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }

                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaDto>>>(stringData)
                        ?? throw new NegocioException("No se pudo deserializar los datos de respuesta.");

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = true,
                        //Mensaje = apiResponse.Data?.resultado_msj ?? "Operación completada con éxito.",
                        ListaEntidad = apiResponse.Data
                    };
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(stringData);
                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = errorResponse?.Data ?? "Error al procesar la solicitud."
                    };
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = "No tiene permisos para acceder a este recurso."
                    };
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Error desconocido al procesar la respuesta de la API.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new RespuestaGenerica<RespuestaDto>
                {
                    Ok = false,
                    Mensaje = "Algo no fue bien al intentar pasar los asientos a contabilidad."
                };
            }
        }

        // AsientoTemporalServicio.cs en gc.sitio.core
        // Añade este método a la clase existente:
        public async Task<RespuestaGenerica<AsientoDetalleDto>> ObtenerAsientoDetalle(string moviId, string token)
        {
            try
            {
                HelperAPI helper = new();

                // Construcción de la URL
                var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_OBTENER_ASIENTO_DETALLE}/{moviId}";

                // Inicializar cliente
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                // Enviar solicitud GET
                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió una respuesta válida. Intente de nuevo más tarde." };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<AsientoDetalleDto>>(stringData)
                        ?? throw new NegocioException("No se pudo deserializar los datos del asiento.");

                    return new RespuestaGenerica<AsientoDetalleDto> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };
                }
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    return new RespuestaGenerica<AsientoDetalleDto> { Ok = false, Mensaje = stringData };
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new RespuestaGenerica<AsientoDetalleDto> { Ok = false, Mensaje = "No tiene permisos para acceder a este recurso." };
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        return new RespuestaGenerica<AsientoDetalleDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        return new RespuestaGenerica<AsientoDetalleDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Error desconocido al procesar la respuesta de la API.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new RespuestaGenerica<AsientoDetalleDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el detalle del asiento." };
            }
        }
    }
}

