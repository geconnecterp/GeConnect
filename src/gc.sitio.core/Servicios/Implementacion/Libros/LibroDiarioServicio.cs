using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes;
using gc.sitio.core.Servicios.Contratos.Libros;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion.Libros
{
    public class LibroDiarioServicio: Servicio<Dto>, ILibroDiarioServicio
    {
        private const string RutaAPI = "/api/apildiario";
        private const string POST_OBTENER_ASIENTOS_LIBRO_DIARIO = "/obtener-asientos-ldiario";
        private readonly AppSettings _appSettings;
        public LibroDiarioServicio(IOptions<AppSettings> options, ILogger<LibroDiarioServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }

        // Aquí puedes implementar los métodos específicos del servicio de Libro Mayor
        // Ejemplo: ObtenerLibroMayor, CrearLibroMayor, etc.
        public async Task<(List<AsientoDetalleLDDto>, MetadataGrid)> ObtenerAsientosLibroDiario(LDiarioRequest query, string token)
        {
            try
            {
                ApiResponse<List<AsientoDetalleLDDto>> apiResponse;
                HelperAPI helper = new();

                // Inicializar cliente y preparar contenido
                HttpClient client = helper.InicializaCliente(query, token, out StringContent contentData);
                HttpResponseMessage response;

                // Construir la URL de la API
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_OBTENER_ASIENTOS_LIBRO_DIARIO}";

                // Enviar solicitud POST
                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }

                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AsientoDetalleLDDto>>>(stringData)
                        ?? throw new NegocioException("No se pudo deserializar el libro mayor.");

                    return (apiResponse.Data ?? [], apiResponse.Meta ?? new());
                }
                else
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<string>(stringData);
                    if (error != null)
                    {
                        throw new NegocioException(error ?? "Hubo un problema en la recepcion del libro mayor");
                    }
                    else
                    {
                        throw new Exception("Hubo un problema en la recepcion del libro mayor");
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
                        throw new NegocioException(error.Detail ?? "Hubo un problema en la recepcion del libro mayor");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Hubo un problema en la recepcion del libro mayor");
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
                throw new NegocioException("Algo no fue bien al intentar obtener los asientos temporales.");
            }
        }
    }
}
