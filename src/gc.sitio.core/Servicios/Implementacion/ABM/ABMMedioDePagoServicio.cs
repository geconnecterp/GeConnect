using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion.ABM
{
	public class ABMMedioDePagoServicio : Servicio<ABMMedioDePagoSearchDto>, IABMMedioDePagoServicio
	{
		private const string RUTAAPI = "/api/abmmediodepago";
		private const string BUSCAR_MEDIOS_DE_PAGO = "/BuscarMediosDePago";

		private readonly AppSettings _appSettings;

		public ABMMedioDePagoServicio(IOptions<AppSettings> options, ILogger<ABMMedioDePagoServicio> logger) : base(options, logger, RUTAAPI)
		{
			_appSettings = options.Value;
		}

		public async Task<(List<ABMMedioDePagoSearchDto>, MetadataGrid)> BuscarMediosDePago(QueryFilters filters, string token)
		{
			try
			{
				ApiResponse<List<ABMMedioDePagoSearchDto>>? apiResponse;
				HelperAPI helper = new();

				HttpClient client = helper.InicializaCliente(filters, token, out StringContent contentData);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RUTAAPI}{BUSCAR_MEDIOS_DE_PAGO}";

				response = await client.PostAsync(link, contentData);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ABMMedioDePagoSearchDto>>>(stringData);

					return (apiResponse.Data, apiResponse.Meta);
				}
				else
				{
					string stringData = await response.Content.ReadAsStringAsync();
					_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");

					throw new NegocioException("Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod().Name} - {ex}");

				throw new Exception("Algo no fue bien al intentar cargar los conteos previso de ajustes.");
			}
		}
	}
}
