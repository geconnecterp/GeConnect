using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace gc.sitio.core.Servicios.Implementacion
{
	public class MedioDePagoServicio : Servicio<MedioDePagoDto>, IMedioDePagoServicio
	{
		private const string RutaAPI = "/api/apimediodepago";
		private const string ObtenerMedioDePagoParaABM = "/GetMedioDePagoParaABM";
		private const string ObtenerOpcionesDeCuotaParaABM = "/GetOpcionesDeCuotaParaABM";
		private const string ObtenerOpcionDeCuotaParaABM = "/GetOpcionDeCuotaParaABM";
		private const string ObtenerCuentaFinYContableLista = "/GetCuentaFinYContableLista";
		private const string ObtenerCuentaFinYContable = "/GetCuentaFinYContable";
		private readonly AppSettings _appSettings;

		public MedioDePagoServicio(IOptions<AppSettings> options, ILogger<MedioDePagoServicio> logger) : base(options, logger, RutaAPI)
		{
			_appSettings = options.Value;
		}

		public List<FinancieroListaDto> GetCuentaFinYContableLista(string insId, string token)
		{
			ApiResponse<List<FinancieroListaDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCuentaFinYContableLista}?ins_id={insId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroListaDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la cuenta financiera lista. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de la cuenta financiera lista: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de la cuenta financiera lista");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta financiera lista.");
				throw;
			}
		}

		public List<FinancieroListaDto> GetCuentaFinYContable(string ctafId, string token)
		{
			ApiResponse<List<FinancieroListaDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCuentaFinYContable}?ctaf_id={ctafId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<FinancieroListaDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la cuenta financiera. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de la cuenta financiera: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de la cuenta financiera");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta financiera.");
				throw;
			}
		}

		public List<OpcionCuotaDto> GetOpcionCuota(string insId, int cuota, string token)
		{
			ApiResponse<List<OpcionCuotaDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerOpcionDeCuotaParaABM}?ins_id={insId}&cuota={cuota}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<OpcionCuotaDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la opcion de cuota. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de la opcion de cuota: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de la opcion de cuota");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la opcion de cuota.");
				throw;
			}
		}

		public List<OpcionCuotaDto> GetOpcionesCuota(string insId, string token)
		{
			ApiResponse<List<OpcionCuotaDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerOpcionesDeCuotaParaABM}?ins_id={insId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<OpcionCuotaDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de las opciones de cuota. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de las opciones de cuota: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de las opciones de cuota");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de las opciones de cuota.");
				throw;
			}
		}

		public List<MedioDePagoABMDto> GetMedioDePagoParaABM(string insId, string token)
		{
			ApiResponse<List<MedioDePagoABMDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerMedioDePagoParaABM}?ins_id={insId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<MedioDePagoABMDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos del medio de pago. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos del medio de pago: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos del medio de pago");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos del medio de pago.");
				throw;
			}
		}


	}
}
