using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion.ABM
{
    public class ABMZonaServicio : Servicio<ABMZonaDto>, IABMZonaServicio
    {
        private const string RutaAPI = "/api/ABMZona";
        //private const string BUSCAR_VENDEDORES = "/ObtenerVendedores";
        //private const string BUSCAR_VENDEDOR = "/ObtenerVendedorPorId";

        private readonly AppSettings _appSettings;
        public ABMZonaServicio(IOptions<AppSettings> options, ILogger<ABMClienteServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }

        public async Task<(List<ABMZonaDto>, MetadataGrid)> ObtenerZonas(QueryFilters filtro, string token)
        {
            try
            {
                ApiResponse<List<ABMZonaDto>>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(filtro, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ABMZonaDto>>>(stringData);
                    if(apiResponse == null)
                    {
                        throw new NegocioException("La deserialización devolvió un valor nulo.");
                    }
                    return (apiResponse.Data ?? [], apiResponse.Meta??new());
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        throw new NegocioException(error.Detail??"Hubo un problema en la recepcion de los datos de la Zona");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true    )
                    {
                        throw new NegocioException(error.Detail ?? "Hubo un problema en la recepcion de los datos de la Zona");
                    }
                    else
                    {
                        throw new Exception(error?.Detail);
                    }
                }
            }
            catch (NegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                throw new Exception("Algo no fue bien al intentar obtener los Perfiles solicitados según el filtro actual.");
            }
        }

        public async Task<RespuestaGenerica<ZonaDto>> ObtenerZonaPorId(string ve_id, string token)
        {
            try
            {
                ApiResponse<ZonaDto> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}?zn_id={ve_id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<ZonaDto>>(stringData)
                        ?? throw new Exception("La deserialización devolvió un valor nulo.");

                    return new RespuestaGenerica<ZonaDto> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error!=null && error.TypeException?.Equals(nameof(NegocioException))==true)
                    {
                        return new RespuestaGenerica<ZonaDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException))==true)
                    {
                        return new RespuestaGenerica<ZonaDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else
                    {
                        if(error != null)
                        {
                            throw new Exception(error.Detail);
                        }
                        else
                        {
                            return new RespuestaGenerica<ZonaDto> { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<ZonaDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los datos del vendedor." };
            }
        }
    }
}
