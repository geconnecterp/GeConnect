using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Inventario;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace gc.sitio.core.Servicios.Implementacion
{
	public class InventarioServicio : Servicio<InventarioDto>, IInventarioServicio
	{
		private const string RutaAPI = "/api/apiinventario";
		private const string INV_LISTA = "/ObtenerInventarioLista";
		private readonly AppSettings _appSettings;
		public InventarioServicio(IOptions<AppSettings> options, ILogger logger) : base(options, logger, RutaAPI)
		{
			_appSettings = options.Value;
		}

		public List<InventarioDto> GetInventarioLista(GetInventarioListaRequest request, string token)
		{
			ApiResponse<List<InventarioDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{INV_LISTA}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().Result;
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InventarioDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().Result;
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}
	}
}
