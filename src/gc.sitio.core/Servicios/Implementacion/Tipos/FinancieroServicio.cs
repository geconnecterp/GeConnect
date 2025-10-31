using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Consultas.ReporteFinanciero;
using gc.infraestructura.Dtos.Consultas.ReporteFinanciero.Request;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Tipos;
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
		private const string ObtenerFinancieroBcoLibroResumen = "/GetFinancieroBcoLibroResumen";
		private const string ObtenerFinancieroBcoLibro = "/GetFinancieroBcoLibro";
		private const string ObtenerFinancieroBcoVencChequeEmitido = "/GetFinancieroBcoVencChequeEmitido";
		private const string ObtenerFinancieroBcoVencChequeEmitidoLista = "/GetFinancieroBcoVencChequeEmitidoLista";
		private const string ObtenerChequesEmitidosEstadosLista = "/GetChequesEmitidosEstadosLista";
		private const string ObtenerChequeModificadosLista = "/GetChequeModificadosLista";
		private const string SetFChequeModificar = "/SetChequeModificar";
		private const string RegistrarFechaDeEntrega = "/SetFechaDeEntrega";
		private const string RegistrarRechazoDeCheque = "/SetRechazoDeCheque";
		private const string ObtenerECheqLista = "/GetECheqLista";
		private const string ExtractoBancarioConfirmar = "/SetExtractoBancarioConfirmar";
		private const string ObtenerBcoExtractoDesdeFile = "/GetBcoExtractoDesdeFile";
		private const string ObtenerFinancieroConciliaDatos = "/GetFinancieroConciliaDatos";
		private const string ObtenerFinancieroConciliaNro = "/GetFinancieroConciliaNro";
		private const string SetFinancieroExtractoDesconcilia = "/FinancieroExtractoDesconcilia";
		private const string SetFinancieroConciliacionExtractoConfirmar = "/FinancieroConciliacionExtractoConfirmar";
		private const string ObtenerGastosProyLista = "/GetGastosProyLista";
		private const string ObtenerGastosProyDatos = "/GetGastosProyDatos";
		private const string ObtenerProyeccionFinanciera = "/GetProyeccionFinanciera";
		private const string ObtenerSaldoDeCuentas = "/GetSaldoDeCuentas";
		private const string ObtenerFlujoDeIngreso = "/GetFlujoDeIngreso";
		private const string SetFinancieroAnticipoEmpleadoConfirma = "/FinancieroAnticipoEmpleadoConfirma";
		private const string ObtenerFinancieroTopePorCuenta = "/GetFinancieroTopePorCuenta";
		private const string ObtenerAnticipoDetalle = "/GetAnticipoDetalle";
		private const string ObtenerFinancieroUsuarios = "/GetFinancieroUsuarios";
		private const string ObtenerAnticipoFinancierosDeEmpleados = "/BuscarAnticipoFinancierosDeEmpleados";

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

		public List<FinancieroBcoLibroResumenDto> GetFinancieroBcoLibroResumen(FinancieroBcoLibroResumenRequest request, string token)
		{
			ApiResponse<List<FinancieroBcoLibroResumenDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroBcoLibroResumen}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroBcoLibroResumenDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<FinancieroBcoLibroDto> GetFinancieroBcoLibro(FinancieroBcoLibroRequest request, string token)
		{
			ApiResponse<List<FinancieroBcoLibroDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroBcoLibro}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroBcoLibroDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<FinancieroBcoVencChequeEmitidoDto> GetFinancieroBcoVencChequeEmitido(FinancieroBcoVencChequeEmitidoRequest request, string token)
		{
			ApiResponse<List<FinancieroBcoVencChequeEmitidoDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroBcoVencChequeEmitido}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroBcoVencChequeEmitidoDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<FinancieroBcoVencChequeEmitidoListaDto> GetFinancieroBcoVencChequeEmitidoLista(FinancieroBcoVencChequeEmitidoListaRequest request, string token)
		{
			ApiResponse<List<FinancieroBcoVencChequeEmitidoListaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroBcoVencChequeEmitidoLista}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroBcoVencChequeEmitidoListaDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<ChequeEmitidoEstadoDto> GetChequesEmitidosEstadosLista(string token)
		{
			try
			{
				ApiResponse<List<ChequeEmitidoEstadoDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaTiposAPI}{ObtenerChequesEmitidosEstadosLista}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ChequeEmitidoEstadoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public List<ChequeModificadosListaDto> GetChequeModificadosLista(GetChequeModificadosListaRequest request, string token)
		{
			ApiResponse<List<ChequeModificadosListaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerChequeModificadosLista}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ChequeModificadosListaDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public RespuestaGenerica<RespuestaDto> SetChequeModificar(GetChequeModificarListaRequest request, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{SetFChequeModificar}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros che_nro:{request.che_nro} che_emision: {request.che_emision}");
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

		public RespuestaGenerica<RespuestaDto> SetFechaDeEntrega(RegistrarFechaDeEntregaRequest request, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RegistrarFechaDeEntrega}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros che_emision: {request.che_emision}");
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

		public RespuestaGenerica<RespuestaDto> SetRechazoDeCheque(RegistrarRechazoDeChequeRequest request, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{RegistrarRechazoDeCheque}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros che_emision: {request.che_emision}");
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

		public List<ECheqDto> GetECheqLista(PasoPrevioECheqRequest request, string token)
		{
			ApiResponse<List<ECheqDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerECheqLista}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ECheqDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public RespuestaGenerica<RespuestaDto> SetExtractoBancarioConfirmar(SetExtractoBancarioConfirmaRequest request, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ExtractoBancarioConfirmar}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros ctaf_id: {request.ctaf_id}");
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

		public List<CrudExtractoBancarioDto> GetBcoExtractoDesdeFile(ExtractoBcoFileRequest request, string token)
		{
			ApiResponse<List<CrudExtractoBancarioDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerBcoExtractoDesdeFile}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<CrudExtractoBancarioDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<FinancieroConciliaDatosDto> GetFinancieroConciliaDatos(FinancieroConciliaDatosRequest request, string token)
		{
			ApiResponse<List<FinancieroConciliaDatosDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroConciliaDatos}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroConciliaDatosDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<FinancieroConciliaNroDto> GetFinancieroConciliaNro(FinancieroConciliaNrosRequest request, string token)
		{
			ApiResponse<List<FinancieroConciliaNroDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroConciliaNro}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroConciliaNroDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public RespuestaGenerica<RespuestaDto> FinancieroExtractoDesconcilia(FinancieroExtractoDesconciliaRequest request, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{SetFinancieroExtractoDesconcilia}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros ctaf_id: {request.ctaf_id}");
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

		public RespuestaGenerica<RespuestaDto> FinancieroConciliacionExtractoConfirmar(FinancieroConciliacionExtractoConfirmarRequest request, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{SetFinancieroConciliacionExtractoConfirmar}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros ctaf_id: {request.ctaf_id}");
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

		public List<GastoProyListaDto> GetGastosProyLista(string token)
		{
			try
			{
				ApiResponse<List<GastoProyListaDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerGastosProyLista}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<GastoProyListaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public List<GastoProyListaDto> GetGastosProyDatos(int items, string token)
		{
			try
			{
				ApiResponse<List<GastoProyListaDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerGastosProyDatos}?items={items}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<GastoProyListaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public List<ProyFinanDto> GetProyeccionFinanciera(BuscarProyFinanRequest request, string token)
		{
			ApiResponse<List<ProyFinanDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerProyeccionFinanciera}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProyFinanDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<SaldoDeCuentaDto> GetSaldoDeCuentas(BuscarSaldoDeCuentasRequest request, string token)
		{
			ApiResponse<List<SaldoDeCuentaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerSaldoDeCuentas}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<SaldoDeCuentaDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<FlujoDeIngresoDto> GetFlujoDeIngreso(BuscarFlujoDeIngresoRequest request, string token)
		{
			ApiResponse<List<FlujoDeIngresoDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFlujoDeIngreso}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FlujoDeIngresoDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public RespuestaGenerica<RespuestaDto> FinancieroAnticipoEmpleadoConfirma(CargaAnticipoEmpleadoRequest request, string token)
		{
			ApiResponse<List<RespuestaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{SetFinancieroAnticipoEmpleadoConfirma}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros cta_id: {request.cta_id}");
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

		public List<FinancieroTopeCtaDto> GetFinancieroTopePorCuenta(string cta_id, string token)
		{
			try
			{
				ApiResponse<List<FinancieroTopeCtaDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroTopePorCuenta}?cta_id={cta_id}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroTopeCtaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public List<AnticipoDetalleDto> GetAnticipoDetalle(string an_compte, string token)
		{
			try
			{
				ApiResponse<List<AnticipoDetalleDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerAnticipoDetalle}?an_compte={an_compte}";
				response = client.GetAsync(link).GetAwaiter().GetResult();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Sin parámetros de busqueda");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AnticipoDetalleDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
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

		public List<FinancieroUsuarioDto> GetFinancieroUsuarios(GetFinancieroUsuariosRequest request, string token)
		{
			ApiResponse<List<FinancieroUsuarioDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFinancieroUsuarios}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroUsuarioDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<(List<AnticipoFinanEmpListaDto>, MetadataGrid)> BuscarAnticipoFinancierosDeEmpleados(ConsultaAnticipoFinanEmpRequest filters, string token)
		{
			try
			{
				ApiResponse<List<AnticipoFinanEmpListaDto>>? apiResponse;
				HelperAPI helper = new();

				HttpClient client = helper.InicializaCliente(filters, token, out StringContent contentData);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerAnticipoFinancierosDeEmpleados}";

				response = await client.PostAsync(link, contentData);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AnticipoFinanEmpListaDto>>>(stringData);

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
	}
}
