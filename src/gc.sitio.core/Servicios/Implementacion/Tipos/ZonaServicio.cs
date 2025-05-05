using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class ZonaServicio : Servicio<ZonaDto>, IZonaServicio
    {
        private const string RutaAPI = "/api/apicuenta";
        private const string ObtenerZonaLista = "/GetZonaLista";
        private readonly AppSettings _appSettings;

        public ZonaServicio(IOptions<AppSettings> options, ILogger<AdministracionServicio> logger) : base(options, logger)
        {
            _appSettings = options.Value;
        }
        public List<ZonaDto> GetZonaLista(string token)
        {
            try
            {
                ApiResponse<List<ZonaDto>> apiResponse;
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerZonaLista}";
                response = client.GetAsync(link).GetAwaiter().GetResult();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        _logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
                        return [];
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ZonaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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
