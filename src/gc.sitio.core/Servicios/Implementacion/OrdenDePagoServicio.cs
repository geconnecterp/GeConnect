using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.Dtos.OrdenDePago.Request;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion
{
	public class OrdenDePagoServicio : Servicio<OrdenDePagoDto>, IOrdenDePagoServicio
	{
		private const string RutaAPI = "/api/apiordendepago";
		private const string ObtenerOPValidacionesPrev = "/GetOPValidacionesPrev";
		private const string ObtenerOPDebitoYCreditoDelProveedor = "/GetOPDebitoYCreditoDelProveedor";
		private const string AgregarQuitarOPDebitoCreditoDelProveedor = "/CargarSacarOPDebitoCreditoDelProveedor";
		private const string ObtenerRetencionesDesdeObligYCredSeleccionados = "/CargarRetencionesDesdeObligYCredSeleccionados";
		private const string ObtenerValoresDesdeObligYCredSeleccionados = "/CargarValoresDesdeObligYCredSeleccionados";
		private const string ObtenerOPMotivosCtag = "/CargarOPMotivosCtag";
		private const string GuardarOrdenDePagoAProveedor = "/ConfirmarOrdenDePagoAProveedor";
		private const string ConsOrdPagoDetExtend = "/ConsultaOrdPagoDetExtend";
		private const string GuardarOrdenDePagoDirecta = "/ConfirmarOrdenDePagoDirecta";
		private readonly AppSettings _appSettings;
		public OrdenDePagoServicio(IOptions<AppSettings> options, ILogger<OrdenDePagoServicio> logger) : base(options, logger, RutaAPI)
		{
			_appSettings = options.Value;
		}

		public List<OPValidacionPrevDto> GetOPValidacionesPrev(string cta_id, string token)
		{
			ApiResponse<List<OPValidacionPrevDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerOPValidacionesPrev}?cta_id={cta_id}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<OPValidacionPrevDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de validación del proveedor. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de validación del proveedor: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de validación del proveedor");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de validación del proveedor.");
				throw;
			}
		}

		public List<OPDebitoYCreditoDelProveedorDto> GetOPDebitoYCreditoDelProveedor(string cta_id, char tipo, bool excluye_notas, string admId, string usuId, string token)
		{
			ApiResponse<List<OPDebitoYCreditoDelProveedorDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerOPDebitoYCreditoDelProveedor}?cta_id={cta_id}&tipo={tipo}&excluye_notas={excluye_notas}&admId={admId}&usuId={usuId}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<OPDebitoYCreditoDelProveedorDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los Débitos o Créditos del del proveedor. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los Débitos o Créditos del proveedor: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los Débitos o Créditos del proveedor");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de validación del proveedor.");
				throw;
			}
		}

		public RespuestaGenerica<RespuestaRelaDto> CargarSacarOPDebitoCreditoDelProveedor(CargarOSacarObligacionesOCreditosRequest r, string token)
		{
			ApiResponse<List<RespuestaRelaDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(r, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{AgregarQuitarOPDebitoCreditoDelProveedor}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros cta_id:{r.cta_id} dia_movi: {r.dia_movi} tco_id: {r.tco_id} cm_compte: {r.cm_compte} cm_compte_cuota: {r.cuota}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaRelaDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return new RespuestaGenerica<RespuestaRelaDto>() { Entidad = apiResponse.Data.First() };
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<RetencionesDesdeObligYCredDto> CargarRetencionesDesdeObligYCredSeleccionados(CargarRetencionesDesdeObligYCredSeleccionadosRequest r, string token)
		{
			ApiResponse<List<RetencionesDesdeObligYCredDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(r, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerRetencionesDesdeObligYCredSeleccionados}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().Result;
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros cta_id:{r.cta_id}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RetencionesDesdeObligYCredDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().Result;
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<ValoresDesdeObligYCredDto> CargarValoresDesdeObligYCredSeleccionados(CargarValoresDesdeObligYCredSeleccionadosRequest r, string token)
		{
			ApiResponse<List<ValoresDesdeObligYCredDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(r, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerValoresDesdeObligYCredSeleccionados}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().Result;
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros cta_id:{r.cta_id}");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ValoresDesdeObligYCredDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().Result;
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return [];
			}
		}

		public RespuestaGenerica<RespuestaDto> ConfirmarOrdenDePagoAProveedor(ConfirmarOPaProveedorRequest r, string token)
		{
			ApiResponse<RespuestaDto> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(r, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GuardarOrdenDePagoAProveedor}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().Result;
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error. Parametros cta_id:{r.cta_id}");
					return new();
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return new RespuestaGenerica<RespuestaDto>() { Entidad = apiResponse.Data };
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().Result;
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<ConsOrdPagoDetExtendDto> ConsultaOrdPagoDetExtend(string op_compte, string token)
		{
			ApiResponse<List<ConsOrdPagoDetExtendDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ConsOrdPagoDetExtend}?op_compte={op_compte}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<ConsOrdPagoDetExtendDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API consulta de orden de pago a proveedor. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener consulta de orden de pago a proveedor: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener consulta de orden de pago a proveedor");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de consulta de orden de pago a proveedor.");
				throw;
			}
		}

		public List<OPMotivoCtagDto> CargarOPMotivosCtag(string opt_id, string token)
		{
			ApiResponse<List<OPMotivoCtagDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerOPMotivosCtag}?opt_id={opt_id}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<OPMotivoCtagDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de motivos de op. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de motivos de op: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de motivos de op");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de motivos de op.");
				throw;
			}
		}

		public RespuestaGenerica<RespuestaDto> ConfirmarOrdenDePagoDirecta(ConfirmarOrdenDePagoDirectaRequest r, string token)
		{
			ApiResponse<RespuestaDto> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(r, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GuardarOrdenDePagoDirecta}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().Result;
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
				string stringData = response.Content.ReadAsStringAsync().Result;
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}
	}
}
