using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion.Asientos
{
    public class AsientoDefinitivoServicio : Servicio<Dto>, IAsientoDefinitivoServicio
    {
        private const string RutaAPI = "/api/AsientoDefinitivo";
        private const string POST_OBTENER_ASIENTOS = "/obtener-asientos";

        private readonly AppSettings _appSettings;
        
        private const string GET_OBTENER_ASIENTO_DETALLE = "/obtener-asiento-detalle";

        public AsientoDefinitivoServicio(IOptions<AppSettings> options, ILogger<AsientoTemporalServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }

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

        public async Task<(List<AsientoGridDto>, MetadataGrid)> ObtenerAsientos(QueryAsiento query, string token)
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
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }

                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AsientoGridDto>>>(stringData)
                        ?? throw new NegocioException("No se pudo deserializar los datos de asientos temporales.");

                    return (apiResponse.Data ?? [], apiResponse.Meta ?? new());
                }
                else
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
                throw new NegocioException("Algo no fue bien al intentar obtener los asientos temporales.");
            }
        }
    }
}
