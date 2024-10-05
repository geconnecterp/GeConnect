using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Productos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class RemitoServicio : Servicio<RemitoDto>, IRemitoServicio
    {
        private const string RutaAPI = "/api/apiremito";
        private const string RemitosTransferidosLista = "/ObtenerRemitosTransferidosLista";
        private readonly AppSettings _appSettings;
        public RemitoServicio(IOptions<AppSettings> options, ILogger<RemitoServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }
        public async Task<List<RemitoTransferidoDto>> ObtenerRemitosTransferidos(string admId, string token)
        {
            ApiResponse<List<RemitoTransferidoDto>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{RemitosTransferidosLista}?admId={admId}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda admId:{admId}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RemitoTransferidoDto>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }
    }
}
