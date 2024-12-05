using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace gc.sitio.core.Servicios.Implementacion
{
    public  class TipoNegocioServicio : Servicio<TipoNegocioDto>, ITipoNegocioServicio
    {
        private const string RutaAPI = "/api/apicuenta";
        private const string ObtenerTipoDeNegocio = "/GetTiposDeNegocio";
        private readonly AppSettings _appSettings;
        public TipoNegocioServicio(IOptions<AppSettings> options, ILogger<AdministracionServicio> logger) : base(options, logger)
        {
            _appSettings = options.Value;
        }

        public List<TipoNegocioDto> ObtenerTiposDeNegocio(string token)
        {
            try
            {
                ApiResponse<List<TipoNegocioDto>> apiResponse;
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerTipoDeNegocio}";
                response = client.GetAsync(link).GetAwaiter().GetResult();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        _logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
                        return [];
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TipoNegocioDto>>>(stringData);
                    return apiResponse.Data;
                }
                else
                {
                    string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    return [];
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Algo no fue bien. Error interno {ex.Message}");
                return [];
            }
        }
    }
}
