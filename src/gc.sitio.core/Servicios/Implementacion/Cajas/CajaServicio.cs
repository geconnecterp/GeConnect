using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.core.Servicios.Contratos.Cajas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion.Cajas
{
	public class CajaServicio : Servicio<Dto>, ICajaServicio
	{
		private const string RutaAPI = "/api/apicaja";

		private const string POST_CIERRE_CAJA_GRAL = "/CierreCajaGral";
		private const string POST_HABILITAR_CAJA_GRAL = "/HabilitarCajaGral";
		private const string GET_OBTENER_PV_ABIERTOS = "/ObtenerPVAbiertos";

		private const string GET_FAL_ULTIMA_FECHA = "/BuscarFaltanteDeCajaUltimaFecha";
		private const string GET_FAL_FALTANTE_DE_CAJA = "/BuscarFaltanteDeCaja";
		private const string GET_FAL_FALTANTE_DE_CAJA_DETALLE = "/BuscarFaltanteDeCajaDetalle";

		private const string POST_CONFIRMAR_FALTANTE_DE_CAJA = "/ConfirmarFaltanteDeCaja";

		public CajaServicio(IOptions<AppSettings> options, ILogger<CajaServicio> logger) : base(options, logger)
		{
		}

		public async Task<RespuestaGenerica<RespuestaDto>> CierreCajaGral(string usu_id, string adm_id, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var request = new CajaCerrarRequest() { adm_id = adm_id, usu_id = usu_id };
				var client = helper.InicializaCliente(request, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_CIERRE_CAJA_GRAL}";

				using var response = await client.PostAsync(link, contentData);
				if (response.StatusCode == HttpStatusCode.OK)
				{
					var stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
					}

					var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);
					if (apiResponse == null || apiResponse.Data == null)
					{
						return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
					}

					var resp = apiResponse.Data;
					if (resp.resultado == 0)
					{
						return new RespuestaGenerica<RespuestaDto>
						{
							Ok = true,
							Mensaje = "OK",
							Entidad = apiResponse.Data
						};
					}
					else if (resp.resultado > 0)
					{
						return new RespuestaGenerica<RespuestaDto>
						{
							Ok = false,
							EsWarn = true,
							EsError = false,
							Mensaje = resp.resultado_msj,
							Entidad = apiResponse.Data
						};
					}
					else
					{
						return new RespuestaGenerica<RespuestaDto>
						{
							Ok = false,
							EsWarn = false,
							EsError = true,
							Mensaje = resp.resultado_msj,
							Entidad = apiResponse.Data
						};
					}
				}
				else
				{
					var msg = await ReadApiErrorAsync(response);
					_logger.LogWarning($"Error API ({response.StatusCode}): {msg}");
					return new() { Ok = false, Mensaje = msg };
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al realizar el cierre general de caja" };
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> HabilitarCajaGral(string usu_id, string adm_id, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var request = new CajaHabilitarRequest() { adm_id = adm_id, usu_id = usu_id };
				var client = helper.InicializaCliente(request, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_HABILITAR_CAJA_GRAL}";

				using var response = await client.PostAsync(link, contentData);
				if (response.StatusCode == HttpStatusCode.OK)
				{
					var stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
					}

					var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);
					if (apiResponse == null || apiResponse.Data == null)
					{
						return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
					}

					var resp = apiResponse.Data;
					if (resp.resultado == 0)
					{
						return new RespuestaGenerica<RespuestaDto>
						{
							Ok = true,
							Mensaje = "OK",
							Entidad = apiResponse.Data
						};
					}
					else if (resp.resultado > 0)
					{
						return new RespuestaGenerica<RespuestaDto>
						{
							Ok = false,
							EsWarn = true,
							EsError = false,
							Mensaje = resp.resultado_msj,
							Entidad = apiResponse.Data
						};
					}
					else
					{
						return new RespuestaGenerica<RespuestaDto>
						{
							Ok = false,
							EsWarn = false,
							EsError = true,
							Mensaje = resp.resultado_msj,
							Entidad = apiResponse.Data
						};
					}
				}
				else
				{
					var msg = await ReadApiErrorAsync(response);
					_logger.LogWarning($"Error API ({response.StatusCode}): {msg}");
					return new() { Ok = false, Mensaje = msg };
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al habilitar la caja general" };
			}
		}

		public async Task<List<CajaPVAbiertosDto>> ObtenerPVAbiertos(string admId, string token)
		{
			ApiResponse<List<CajaPVAbiertosDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_OBTENER_PV_ABIERTOS}?admId={admId}";

			response = client.GetAsync(link).GetAwaiter().GetResult();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<CajaPVAbiertosDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}

		}

		public async Task<List<FaltanteDeCajaUltimaFechaDto>> FaltanteDeCajaUltimaFecha(string admId, string token)
		{
			ApiResponse<List<FaltanteDeCajaUltimaFechaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(token);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_FAL_ULTIMA_FECHA}?admId={admId}";

			response = client.GetAsync(link).GetAwaiter().GetResult();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FaltanteDeCajaUltimaFechaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}

		}

		public async Task<List<FaltanteDeCajaDto>> BuscarFaltanteDeCaja(FaltanteDeCajaRequest request, string token)
		{
			ApiResponse<List<FaltanteDeCajaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_FAL_FALTANTE_DE_CAJA}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros adm_id:{request.adm_id}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FaltanteDeCajaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<List<FaltanteDeCajaDetalleDto>> BuscarFaltanteDeCajaDetalle(FaltanteDeCajaDetalleRequest request, string token)
		{
			ApiResponse<List<FaltanteDeCajaDetalleDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_FAL_FALTANTE_DE_CAJA_DETALLE}";

			response = await client.PostAsync(link, contentData);

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros adm_id:{request.adm_id}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<FaltanteDeCajaDetalleDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
				return apiResponse.Data;
			}
			else
			{
				string stringData = await response.Content.ReadAsStringAsync();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public RespuestaGenerica<RespuestaDto> ConfirmarFaltanteDeCaja(ConfirmarFaltanteDeCajaRequest request, string token)
		{
			try
			{
				ApiResponse<RespuestaDto> apiResponse;

				HelperAPI helper = new HelperAPI();

				HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_CONFIRMAR_FALTANTE_DE_CAJA}";

				response = client.PostAsync(link, contentData).Result;

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API devolvió error.");
						return new();
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
					return new RespuestaGenerica<RespuestaDto>() { Entidad = apiResponse.Data };
				}
				else
				{
					string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
					return new();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al confirmar el faltante de caja.");
				return new() { Mensaje = "Algo no fue bien al intentar confirmar el faltante de caja. Verifique Log.", EsError = true };
			}
		}
	}
}
