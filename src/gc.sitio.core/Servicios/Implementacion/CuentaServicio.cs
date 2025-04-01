using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Reflection;
using System.Security.Claims;

namespace gc.sitio.core.Servicios.Implementacion
{
	public class CuentaServicio : Servicio<CuentaDto>, ICuentaServicio
	{
		private const string RutaAPI = "/api/apicuenta";
		private const string ProveedorLista = "/GetProveedorLista";
		private const string CuentaComercialBuscar = "/GetCuentaComercialLista";  //llamada de la consulta para el control de CUENTAS COMERCIALES
		private const string OCxCuentaBuscar = "/GetOCxCuenta";
		private const string DetalleOCBuscar = "/GetOCDetalle";
		private const string ProveedorFamiliaLista = "/GetProveedorFamiliaLista";
		private const string ObtenerCuentaParaABM = "/GetCuentaParaABM";
		private const string ObtenerCuentaFormaDePago = "/GetCuentaFormaDePago";
		private const string ObtenerCuentaContactos = "/GetCuentaContactos";
		private const string ObtenerCuentaObs = "/GetCuentaObs";
		private const string ObtenerCuentaNota = "/GetCuentaNota";
		private const string ObtenerFormaDePagoPorCuentaYFP = "/GetFormaDePagoPorCuentaYFP";
        private const string ObtenerCuentaContactosPorCuentaYTC = "/GetCuentaContactosPorCuentaYTC";
        private const string ObtenerCuentaNotaDatos = "/GetCuentaNotaDatos";
		private const string ObtenerCuentaObsDatos = "/GetCuentaObsDatos";
		private const string ProveedorABMFamiliaLista = "/GetABMProveedorFamiliaLista";
		private const string ProveedorABMFamiliaDatos = "/GetABMProveedorFamiliaDatos";
		private const string OBTENER_LISTA_CLIENTES = "/GetClienteLista";
		private const string ObtenerCompteDatosProv = "/GetCompteDatosProv";
		private const string ObtenerCompteCargaRprAsoc = "/GetCompteCargaRprAsoc";
		private const string ObtenerCompteCargaCtaAsoc = "/GetCompteCargaCtaAsoc";
		//
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


		public List<ProveedorListaDto> ObtenerListaProveedores(string ope_iva,string token)
		{
			ApiResponse<List<ProveedorListaDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ProveedorLista}?ope_iva={ope_iva}";
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

		public List<ProveedorGrupoDto> ObtenerProveedoresABMFamiliaLista(string ctaId, string token)
		{
			ApiResponse<List<ProveedorGrupoDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ProveedorABMFamiliaLista}?ctaId={ctaId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<ProveedorGrupoDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la lista de familia de productos del proveedor. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener la lista de familia de productos del proveedor: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener la lista de familia de productos del proveedor");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener la lista de familia de productos del proveedor.");
				throw;
			}
		}

		public List<ProveedorGrupoDto> ObtenerProveedoresABMFamiliaDatos(string ctaId, string pgId, string token)
		{
			ApiResponse<List<ProveedorGrupoDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ProveedorABMFamiliaDatos}?ctaId={ctaId}&pgId={pgId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<ProveedorGrupoDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la familia de producto del proveedor. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener la familia de producto del proveedor: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener la familia de producto del proveedor");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener la familia de producto del proveedor.");
				throw;
			}
		}

		public List<CuentaABMDto> GetCuentaParaABM(string ctaId, string token)
		{
			ApiResponse<List<CuentaABMDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCuentaParaABM}?cta_id={ctaId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<CuentaABMDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la cuenta. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de la cuenta: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de la cuenta");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta.");
				throw;
			}
		}

		public List<CuentaFPDto> GetCuentaFormaDePago(string ctaId, string token)
		{
			ApiResponse<List<CuentaFPDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCuentaFormaDePago}?cta_id={ctaId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<CuentaFPDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de forma de pago la cuenta. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de forma de pago de la cuenta: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de forma de pago de la cuenta");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta.");
				throw;
			}
		}

		public List<CuentaContactoDto> GetCuentaContactos(string cta_id, string token)
		{
			ApiResponse<List<CuentaContactoDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCuentaContactos}?cta_id={cta_id}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<CuentaContactoDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de contactos la cuenta. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de contactos de la cuenta: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de contactos de la cuenta");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta.");
				throw;
			}
		}

		public List<CuentaObsDto> GetCuentaObs(string cta_id, string token)
		{
			ApiResponse<List<CuentaObsDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCuentaObs}?cta_id={cta_id}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<CuentaObsDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de observaciones la cuenta. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de observaciones de la cuenta: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de observaciones de la cuenta");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta.");
				throw;
			}
		}

		public List<CuentaNotaDto> GetCuentaNota(string cta_id, string token)
		{
			ApiResponse<List<CuentaNotaDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCuentaNota}?cta_id={cta_id}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<CuentaNotaDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de notas de la cuenta. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de notas de la cuenta: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de notas de la cuenta");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta.");
				throw;
			}
		}

		public List<CuentaFPDto> GetFormaDePagoPorCuentaYFP(string cta_id, string fp_id, string token)
		{
			ApiResponse<List<CuentaFPDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerFormaDePagoPorCuentaYFP}?cta_id={cta_id}&fp_id={fp_id}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<CuentaFPDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de forma de pago por cuenta y fp. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de forma de pago por cuenta y fp: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de forma de pago por cuenta y fp");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de forma de pago por cuenta y fp.");
				throw;
			}
		}

        public List<CuentaContactoDto> GetCuentContactosporCuentaYTC(string cta_id, string tc_id, string token)
        {
            ApiResponse<List<CuentaContactoDto>> respuesta;
            string stringData;
            try
            {
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;
                var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCuentaContactosPorCuentaYTC}?cta_id={cta_id}&tc_id={tc_id}";
                response = client.GetAsync(link).GetAwaiter().GetResult();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(stringData))
                    {
                        respuesta = JsonConvert.DeserializeObject<ApiResponse<List<CuentaContactoDto>>>(stringData);
                    }
                    else
                    {
                        throw new Exception("No se logro obtener la respuesta de la API con los datos de otros contactos por cuenta y tc. Verifique.");
                    }
                    return respuesta.Data;
                }
                else
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.LogError($"Error al intentar obtener los datos de otros contactos por cuenta y tc: {stringData}");
                    throw new NegocioException("Hubo un error al intentar obtener los datos de otros contactos por cuenta y tc");
                }

            }
            catch (NegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar obtener los datos de forma de pago por cuenta y fp.");
                throw;
            }
        }

        public List<CuentaNotaDto> GetCuentaNotaDatos(string cta_id, string usu_id, string token)
        {
            ApiResponse<List<CuentaNotaDto>> respuesta;
            string stringData;
            try
            {
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;
                var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCuentaNotaDatos}?cta_id={cta_id}&usu_id={usu_id}";
                response = client.GetAsync(link).GetAwaiter().GetResult();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(stringData))
                    {
                        respuesta = JsonConvert.DeserializeObject<ApiResponse<List<CuentaNotaDto>>>(stringData);
                    }
                    else
                    {
                        throw new Exception("No se logro obtener la respuesta de la API con los datos de notas por cuenta y usuID. Verifique.");
                    }
                    return respuesta.Data;
                }
                else
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.LogError($"Error al intentar obtener los datos de notas por cuenta y usuID: {stringData}");
                    throw new NegocioException("Hubo un error al intentar obtener los datos de notas por cuenta y usuID");
                }

            }
            catch (NegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar obtener los datos de forma de pago por cuenta y fp.");
                throw;
            }
        }

		public List<CuentaObsDto> GetCuentaObsDatos(string cta_id, string to_id, string token)
		{
			ApiResponse<List<CuentaObsDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCuentaObsDatos}?cta_id={cta_id}&to_id={to_id}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<CuentaObsDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de notas por cuenta y usuID. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de notas por cuenta y usuID: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de notas por cuenta y usuID");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de forma de pago por cuenta y fp.");
				throw;
			}
		}

        public async Task<RespuestaGenerica<ClienteListaDto>> ObtenerListaClientes(string search, string token)
        {
            try
            {
                ApiResponse<List<ClienteListaDto>> apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_LISTA_CLIENTES}?search={search}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ClienteListaDto>>>(stringData);

                    return new RespuestaGenerica<ClienteListaDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<ClienteListaDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<ClienteListaDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else
                    {
                        throw new Exception(error.Detail);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod().Name} - {ex}");

                return new RespuestaGenerica<ClienteListaDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener la lista de clientes." };
            }
        }

		public List<ComprobanteDeCompraDto> GetCompteDatosProv(string ctaId, string token)
		{
			ApiResponse<List<ComprobanteDeCompraDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCompteDatosProv}?cta_id={ctaId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<ComprobanteDeCompraDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la cuenta. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de la cuenta: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de la cuenta");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta.");
				throw;
			}
		}

		public List<RprAsociadosDto> GetCompteCargaRprAsoc(string ctaId, string token)
		{
			ApiResponse<List<RprAsociadosDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCompteCargaRprAsoc}?cta_id={ctaId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<RprAsociadosDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la cuenta. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de la cuenta: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de la cuenta");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta.");
				throw;
			}
		}

		public List<NotasACuenta> GetCompteCargaCtaAsoc(string ctaId, string token)
		{
			ApiResponse<List<NotasACuenta>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerCompteCargaCtaAsoc}?cta_id={ctaId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<NotasACuenta>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la cuenta. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de la cuenta: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de la cuenta");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta.");
				throw;
			}
		}
	}
}
