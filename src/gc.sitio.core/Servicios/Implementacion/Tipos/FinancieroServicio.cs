using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion
{
	public class FinancieroServicio : Servicio<FinancieroDto>, IFinancieroServicio
	{
		private const string RutaTiposAPI = "/api/tiposvs";
		private const string RutaAPI = "/api/apifinancieros";
		private const string ObtenerFinancierosPorTipoCfLista = "/GetFinancierosPorTipoCfLista";
		private const string ObtenerFinancierosRelaPorTipoCfLista = "/GetFinancierosRelaPorTipoCfLista";
		private const string ObtenerFinancierosEstados = "/GetFinancierosEstados";
		private const string ObtenerPlanContableCuenta = "/GetPlanContableCuentaLista";
		private const string ObtenerFinancieroDesdeTipoParaSeleccionDeValores = "/GetFinancieroDesdeTipoParaSeleccionDeValores";
		private const string ObtenerFinancieroCarteraParaSeleccionDeValores = "/GetFinancieroCarteraParaSeleccionDeValores";
		private const string SetFinancieroConfirmarTransferencia = "/FinancieroConfirmarTransferencia";
		private const string ObtenerCuentaAlCobroRela = "/GetCuentaAlCobroRela";
		private const string ObtenerFinancieroChequeDepositado = "/GetFinancieroChequeDepositado";
		private const string ObtenerFinancieroTraUsu = "/GetFinancieroTraUsu";
		private const string ObtenerMovimientoFinanciero = "/BuscarMovimientoFinanciero";
		private const string SetMovimientoFinancieroAnular = "/MovimientoFinancieroAnular";
		private const string ObtenerFinancieroTraRepoDDto = "/GetFinancieroTraRepoDDto";
		private const string ObtenerFinancieroTraRepoCtag = "/GetFinancieroTraRepoCtag";
		private const string GetMovimientoFinancieroReporte = "/BuscarMovimientoFinancieroReporte";
		private const string ObtenerFinancieroBcoExtracto = "/GetFinancieroBcoExtracto";
		private const string ObtenerFinancieroBcoCtaCte = "/GetFinancieroBcoCtaCte";

		private readonly AppSettings _appSettings;
		public FinancieroServicio(IOptions<AppSettings> options, ILogger<AdministracionServicio> logger) : base(options, logger)
		{
			_appSettings = options.Value;
		}

		public List<PlanContableDto> GetPlanContableCuentaLista(string token)
		{
			try
			{
				ApiResponse<List<PlanContableDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaTiposAPI}{ObtenerPlanContableCuenta}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PlanContableDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public List<FinancieroEstadoDto> GetFinancierosEstados(string token)
		{
			try
			{
				ApiResponse<List<FinancieroEstadoDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaTiposAPI}{ObtenerFinancierosEstados}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroEstadoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public List<FinancieroDto> GetFinancierosPorTipoCfLista(string tcf_id, string token)
		{
			try
			{
				ApiResponse<List<FinancieroDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaTiposAPI}{ObtenerFinancierosPorTipoCfLista}?tcf_id={tcf_id}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public List<FinancieroDto> GetFinancierosRelaPorTipoCfLista(string tcf_id, string token)
		{
			try
			{
				ApiResponse<List<FinancieroDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaTiposAPI}{ObtenerFinancierosRelaPorTipoCfLista}?tcf_id={tcf_id}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public List<FinancieroDesdeSeleccionDeTipoDto> GetFinancieroDesdeTipoParaSeleccionDeValores(string tcf_id, string adm_id, string token)
		{
			try
			{
				ApiResponse<List<FinancieroDesdeSeleccionDeTipoDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaTiposAPI}{ObtenerFinancieroDesdeTipoParaSeleccionDeValores}?tcf_id={tcf_id}&adm_id={adm_id}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroDesdeSeleccionDeTipoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public List<FinancieroCarteraDto> GetFinancieroCarteraParaSeleccionDeValores(string ctaf_id, string token, string cta_id = "%")
		{
			try
			{
				ApiResponse<List<FinancieroCarteraDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaTiposAPI}{ObtenerFinancieroCarteraParaSeleccionDeValores}?ctaf_id={ctaf_id}&cta_id={cta_id}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroCarteraDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public RespuestaGenerica<RespuestaDto> FinancieroConfirmarTransferencia(ConfirmarTransferenciaRequest request, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{SetFinancieroConfirmarTransferencia}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros ttra_id:{request.ttra_id} tra_concepto: {request.tra_concepto} tra_fecha: {request.tra_fecha} ");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return new RespuestaGenerica<RespuestaDto>() { Entidad = apiResponse.Data.First() };
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<FinancieroCuentaAlCobroRelaDto> GetCuentaAlCobroRela(string ctaf_id, string token)
		{
			try
			{
				ApiResponse<List<FinancieroCuentaAlCobroRelaDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCuentaAlCobroRela}?ctaf_id={ctaf_id}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroCuentaAlCobroRelaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public List<FinancieroChequeDepositadoDto> GetFinancieroChequeDepositado(string ctaf_id, DateTime fechaDesde, DateTime fechaHasta, string token)
		{
			ApiResponse<List<FinancieroChequeDepositadoDto>> apiResponse;

			var request = new FinancieroChequeDepositadoRequest
			{
				ctaf_id = ctaf_id,
				fechaDesde = fechaDesde,
				fechaHasta = fechaHasta
			};
			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroChequeDepositado}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros ctaf_id:{request.ctaf_id} fechaDesde: {request.fechaDesde} fechaHasta: {request.fechaHasta} ");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroChequeDepositadoDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}


		public List<PerfilUserDto> GetFinancieroTraUsu(DateTime fechaDesde, DateTime fechaHasta, string token)
		{
			ApiResponse<List<PerfilUserDto>> apiResponse;

			var request = new FinancieroTraUsuRequest
			{
				FechaDesde = fechaDesde,
				FechaHasta = fechaHasta
			};
			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroTraUsu}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros desde:{request.FechaDesde} hasta: {request.FechaHasta}");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PerfilUserDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<(List<MovimientoFinancieroListaDto>, MetadataGrid)> BuscarMovimientoFinanciero(ConsultaMovFinancierosRequest filters, string token)
		{
			try
			{
				ApiResponse<List<MovimientoFinancieroListaDto>>? apiResponse;
				HelperAPI helper = new();

				HttpClient client = helper.InicializaCliente(filters, token, out StringContent contentData);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerMovimientoFinanciero}";

				response = await client.PostAsync(link, contentData);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<MovimientoFinancieroListaDto>>>(stringData);

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
				_logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

				throw new Exception("Algo no fue bien al intentar cargar los conteos previso de ajustes.");
			}
		}

		public RespuestaGenerica<RespuestaDto> MovimientoFinancieroAnular(MovimientoFinancieroAnularRequest request, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{SetMovimientoFinancieroAnular}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros tra_compte:{request.tra_compte}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return new RespuestaGenerica<RespuestaDto>() { Entidad = apiResponse.Data.First() };
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<FinancieroTraRepoCtagDto> GetFinancieroTraRepoCtag(string tra_compte, string token)
		{
			try
			{
				ApiResponse<List<FinancieroTraRepoCtagDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroTraRepoCtag}?tra_compte={tra_compte}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroTraRepoCtagDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public List<FinancieroTraRepoDDto> GetFinancieroTraRepoDDto(string tra_compte, string token)
		{
			try
			{
				ApiResponse<List<FinancieroTraRepoDDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroTraRepoDDto}?tra_compte={tra_compte}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroTraRepoDDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public List<MovimientoFinancieroListaDto> BuscarMovimientoFinancieroReporte(ConsultaMovFinancierosRequest request, string token)
		{
			ApiResponse<List<MovimientoFinancieroListaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GetMovimientoFinancieroReporte}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<MovimientoFinancieroListaDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<FinancieroBcoExtractoDto> GetFinancieroBcoExtracto(FinancieroBcoExtractoRequest request, string token)
		{
			ApiResponse<List<FinancieroBcoExtractoDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroBcoExtracto}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroBcoExtractoDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<FinancieroBcoCtaCteDto> GetFinancieroBcoCtaCte(FinancieroBcoCtaCteRequest request, string token)
		{
			ApiResponse<List<FinancieroBcoCtaCteDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroBcoCtaCte}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroBcoCtaCteDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}
	}
}
