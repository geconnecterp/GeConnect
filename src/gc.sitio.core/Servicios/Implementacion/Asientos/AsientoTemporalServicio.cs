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

        public AsientoTemporalServicio(IOptions<AppSettings> options, ILogger<AsientoTemporalServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }

        /// <inheritdoc/>
        public async Task<RespuestaGenerica<AsientoGridDto>> ObtenerAsientos(QueryAsiento query, string token)
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
                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }

                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AsientoGridDto>>>(stringData)
                        ?? throw new NegocioException("No se pudo deserializar los datos de asientos temporales.");

                    return new RespuestaGenerica<AsientoGridDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };
                }
                if(response.StatusCode == HttpStatusCode.NotFound)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    return new RespuestaGenerica<AsientoGridDto> { Ok = false, Mensaje = stringData };
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new RespuestaGenerica<AsientoGridDto> { Ok = false, Mensaje = "No tiene permisos para acceder a este recurso." };
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        return new RespuestaGenerica<AsientoGridDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        return new RespuestaGenerica<AsientoGridDto> { Ok = false, Mensaje = error.Detail };
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
                return new RespuestaGenerica<AsientoGridDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los asientos temporales." };
            }
        }


    }
}

