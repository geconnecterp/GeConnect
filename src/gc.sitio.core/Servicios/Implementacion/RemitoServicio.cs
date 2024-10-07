using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Almacen.Tr.Request;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class RemitoServicio : Servicio<RemitoDto>, IRemitoServicio
    {
        private const string RutaAPI = "/api/apiremito";
        private const string RemitosTransferidosLista = "/ObtenerRemitosTransferidosLista";
		private const string RemitosSeteaEstado = "/SetearEstado";
		private const string RemitosVerConteos = "/VerConteos";
		private const string RemitosConfirmarRecepcion = "/ConfirmarRecepcion";
		private readonly AppSettings _appSettings;
        public RemitoServicio(IOptions<AppSettings> options, ILogger<RemitoServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }

		public async Task<List<RespuestaDto>> ConfirmarRecepcion(string remCompte, string usuario, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			RConfirmaRecepcionRequest request = new() { remCompte = remCompte, usuario = usuario };
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RemitosConfirmarRecepcion}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros rem_compte:{remCompte}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaDto>>>(stringData);
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
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

		public async Task<List<RespuestaDto>> SetearEstado(string remCompte, string estado, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new HelperAPI();
            RSetearEstadoRequest request = new() { remCompte = remCompte, estado = estado };
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RemitosSeteaEstado}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros rem_compte:{remCompte}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaDto>>>(stringData);
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<RemitoVerConteoDto>> VerConteos(string remCompte, string token)
		{
			ApiResponse<List<RemitoVerConteoDto>> apiResponse;

			HelperAPI helper = new();

			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RemitosVerConteos}?remCompte={remCompte}";

			response = await client.GetAsync(link);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda remCompte:{remCompte}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RemitoVerConteoDto>>>(stringData);
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
