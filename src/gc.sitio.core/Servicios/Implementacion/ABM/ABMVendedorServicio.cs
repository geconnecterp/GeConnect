using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
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
    public class ABMVendedorServicio : Servicio<ABMVendedorDto>, IABMVendedorServicio
    {
        private const string RutaAPI = "/api/ABMVendedor";
        //private const string BUSCAR_VENDEDORES = "/ObtenerVendedores";
        //private const string BUSCAR_VENDEDOR = "/ObtenerVendedorPorId";

        private readonly AppSettings _appSettings;
        public ABMVendedorServicio(IOptions<AppSettings> options, ILogger<ABMClienteServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }
        public async Task<(List<ABMVendedorDto>, MetadataGrid)> ObtenerVendedores(QueryFilters filtro, string token)
        {
            try
            {
                ApiResponse<List<ABMVendedorDto>>? apiResponse;
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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ABMVendedorDto>>>(stringData);
                    apiResponse = apiResponse ?? throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    return (apiResponse.Data ?? [], apiResponse.Meta ?? new());
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Hubo un problema en la recepcion de los datos del Vendedor");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Hubo un problema en la recepcion de los datos del Vendedor");
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

        public async Task<RespuestaGenerica<ABMVendedorDatoDto>> ObtenerVendedorPorId(string ve_id, string token)
        {
            try
            {
                ApiResponse<ABMVendedorDatoDto> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}?ve_id={ve_id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<ABMVendedorDatoDto>>(stringData)
                        ?? throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");

                    return new RespuestaGenerica<ABMVendedorDatoDto> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Hubo un problema en la recepcion de los datos del Vendedor");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Hubo un problema en la recepcion de los datos del Vendedor");
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

                return new RespuestaGenerica<ABMVendedorDatoDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los datos del vendedor." };
            }
        }
    }
}
