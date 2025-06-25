using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Libros;
using gc.sitio.core.Servicios.Contratos.Libros;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace gc.sitio.core.Servicios.Implementacion.Libros
{
    public class BalanceGrServicio : Servicio<Dto>, IBalanceGrServicio
    {
        private const string RutaAPI = "/api/apibalancegr";
        
        private readonly AppSettings _appSettings;
        public BalanceGrServicio(IOptions<AppSettings> options, ILogger<LibroMayorServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }

        public async Task<List<BalanseGrDto>> ObtenerBalanceGeneral(int eje_nro, string token)
        {
            try
            {
                ApiResponse<List<BalanseGrDto>> apiResponse;
                HelperAPI helper = new();

                // Inicializar cliente y preparar contenido
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                // Construir la URL de la API
                var link = $"{_appSettings.RutaBase}{RutaAPI}?eje_nro={eje_nro}";

                // Enviar solicitud POST
                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }

                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BalanseGrDto>>>(stringData)
                        ?? throw new NegocioException("No se pudo deserializar el Balance General.");

                    return apiResponse.Data ?? [];
                }
                else
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<string>(stringData);
                    if (error != null)
                    {
                        throw new NegocioException(error ?? "Hubo un problema en la recepcion del Balance General");
                    }
                    else
                    {
                        throw new Exception("Hubo un problema en la recepcion del Balance General");
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
                        throw new NegocioException(error.Detail ?? "Hubo un problema en la recepcion del Balance General");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Hubo un problema en la recepcion del Balance General");
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
                throw new NegocioException("Algo no fue bien al intentar obtener el Balance General.");
            }
        }
    }
}
