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
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion.Asientos
{
    public class AsientoFrontServicio :Servicio<Dto>, IAsientoFrontServicio
    {
        private const string RutaAPI = "/api/ApiAsiento";
        private const string GET_EJERCICIOS = "/ejercicios";
        private const string GET_TIPOS_ASIENTO = "/tipos";
        private const string GET_USUARIOS_EJERCICIO = "/usuarios-ejercicio";


        private readonly AppSettings _appSettings;

        public AsientoFrontServicio(IOptions<AppSettings> options, ILogger<AsientoFrontServicio> logger):base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }

        /// <inheritdoc/>
        public async Task<RespuestaGenerica<EjercicioDto>> ObtenerEjercicios(string token)
        {
            try
            {
                ApiResponse<List<EjercicioDto>> apiResponse;
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_EJERCICIOS}";
                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }

                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<EjercicioDto>>>(stringData)
                        ?? throw new NegocioException("No se pudo deserializar los datos de ejercicios.");

                    return new RespuestaGenerica<EjercicioDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        return new RespuestaGenerica<EjercicioDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        return new RespuestaGenerica<EjercicioDto> { Ok = false, Mensaje = error.Detail };
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
                return new RespuestaGenerica<EjercicioDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener la lista de ejercicios." };
            }
        }

        /// <inheritdoc/>
        public async Task<RespuestaGenerica<TipoAsientoDto>> ObtenerTiposAsiento(string token)
        {
            try
            {
                ApiResponse<List<TipoAsientoDto>> apiResponse;
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_TIPOS_ASIENTO}";
                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }

                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TipoAsientoDto>>>(stringData)
                        ?? throw new NegocioException("No se pudo deserializar los datos de tipos de asiento.");

                    return new RespuestaGenerica<TipoAsientoDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        return new RespuestaGenerica<TipoAsientoDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        return new RespuestaGenerica<TipoAsientoDto> { Ok = false, Mensaje = error.Detail };
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
                return new RespuestaGenerica<TipoAsientoDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los tipos de asientos." };
            }
        }

        /// <inheritdoc/>
        public async Task<RespuestaGenerica<UsuAsientoDto>> ObtenerUsuariosDeEjercicio(int eje_nro, string token)
        {
            try
            {
                ApiResponse<List<UsuAsientoDto>> apiResponse;
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_USUARIOS_EJERCICIO}/{eje_nro}";
                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }

                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<UsuAsientoDto>>>(stringData)
                        ?? throw new NegocioException("No se pudo deserializar los datos de usuarios del ejercicio.");

                    return new RespuestaGenerica<UsuAsientoDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        return new RespuestaGenerica<UsuAsientoDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        return new RespuestaGenerica<UsuAsientoDto> { Ok = false, Mensaje = error.Detail };
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
                return new RespuestaGenerica<UsuAsientoDto> { Ok = false, Mensaje = $"Algo no fue bien al intentar obtener los usuarios del ejercicio {eje_nro}." };
            }
        }
    }
}
