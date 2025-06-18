using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Libros;
using gc.sitio.core.Servicios.Contratos.Libros;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion.Libros
{
    public class BSSServicio : Servicio<Dto>, IBSSServicio
    {
        private const string RutaAPI = "/api/apibsumasaldo";
        private const string POST_OBTENER_ASIENTOS_LIBRO_DIARIO = "/obtener-balance-suma-saldo";
        private readonly AppSettings _appSettings;
        public BSSServicio(IOptions<AppSettings> options, 
            ILogger<LibroDiarioServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }
        public async Task<(List<BSumaSaldoRegDto>, MetadataGrid)> ObtenerBalanceSumaSaldos(BSSRequestDto request, string token)
        {
                    var msg = "Hubo un problema en la recepcion del BSS";
            try
            {
                ApiResponse<List<BSumaSaldoRegDto>> apiResponse;
                HelperAPI helper = new();

                // Inicializar cliente y preparar contenido
                HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
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

                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BSumaSaldoRegDto>>>(stringData)
                        ?? throw new NegocioException("No se pudo deserializar el BSS.");

                    return (apiResponse.Data ?? [], apiResponse.Meta ?? new());
                }
                else
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<string>(stringData);
                    if (error != null)
                    {
                        throw new NegocioException(error ?? msg);
                    }
                    else
                    {
                        throw new Exception(msg);
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
                        throw new NegocioException(error.Detail ?? msg);
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? msg);
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
                throw new NegocioException("Algo no fue bien al intentar obtener el BSS.");
            }
        }
    }
}
