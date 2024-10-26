using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace gc.sitio.core.Servicios.Implementacion
{
	public class CuentaServicio : Servicio<CuentaDto>, ICuentaServicio
	{
		private const string RutaAPI = "/api/apicuenta";
		private const string ProveedorLista = "/GetProveedorLista";
		private const string CuentaComercialBuscar = "/GetCuentaComercialLista";
		private const string OCxCuentaBuscar = "/GetOCxCuenta";
		private const string DetalleOCBuscar = "/GetOCDetalle";
		private const string ProveedorFamiliaLista = "/GetProveedorFamiliaLista";
		private readonly AppSettings _appSettings;
		public CuentaServicio(IOptions<AppSettings> options, ILogger<CuentaServicio> logger) : base(options, logger)
		{
			_appSettings = options.Value;
		}

		public async Task<List<CuentaDto>> ObtenerListaCuentaComercial(string texto, char tipo, string token)
		{
			try
			{
				ApiResponse<List<CuentaDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{CuentaComercialBuscar}?texto={texto}&tipo={tipo}";
				response = await client.GetAsync(link);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda texto:{texto}-tipo:{tipo}");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<CuentaDto>>>(stringData);
					return apiResponse.Data;
				}
				else
				{
					string stringData = await response.Content.ReadAsStringAsync();
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

		public async Task<List<RPROrdenDeCompraDto>> ObtenerListaOCxCuenta(string cta_id, string token)
		{
			try
			{
				ApiResponse<List<RPROrdenDeCompraDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{OCxCuentaBuscar}?cta_id={cta_id}";
				response = await client.GetAsync(link);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda cta_id:{cta_id}");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RPROrdenDeCompraDto>>>(stringData);
					return apiResponse.Data;
				}
				else
				{
					string stringData = await response.Content.ReadAsStringAsync();
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

		public async Task<List<RPROrdenDeCompraDetalleDto>> ObtenerDetalleDeOC(string oc_compte, string token)
		{
			try
			{
				ApiResponse<List<RPROrdenDeCompraDetalleDto>> apiResponse;
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{DetalleOCBuscar}?oc_compte={oc_compte}";
				response = await client.GetAsync(link);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						_logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda oc_compte:{oc_compte}");
						return [];
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RPROrdenDeCompraDetalleDto>>>(stringData);
					return apiResponse.Data;
				}
				else
				{
					string stringData = await response.Content.ReadAsStringAsync();
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


		public List<ProveedorListaDto> ObtenerListaProveedores(string token)
		{
			ApiResponse<List<ProveedorListaDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ProveedorLista}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<ProveedorListaDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de las cuentas de proveedores. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los Proveedores: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los proveedores");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los Proveedores.");
				throw;
			}
		}

		public List<ProveedorFamiliaListaDto> ObtenerListaProveedoresFamilia(string ctaId, string token)
		{
			ApiResponse<List<ProveedorFamiliaListaDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ProveedorFamiliaLista}?ctaId={ctaId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<ProveedorFamiliaListaDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de las cuentas de familia de proveedores. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener las Familia de Proveedores: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener las Familia de proveedores");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener las Familia de Proveedores.");
				throw;
			}
		}
	}
}
