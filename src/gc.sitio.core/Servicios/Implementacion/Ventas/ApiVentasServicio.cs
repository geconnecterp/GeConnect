using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Dtos.Ventas.Request;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion
{
	public class ApiVentasServicio : Servicio<Dto>, IApiVentasServicio
	{
		private const string RutaAPI = "/api/apiventas";
		private const string GET_VTAS_PV_CTL_PROCESOS = "/ObtenerVtasPVCtlProcesosLista";
		private const string GET_VTAS_PV_CTL_CIERRES = "/ObtenerVtasPVCtlCierresLista";
		private const string GET_VTAS_PV_CTL_REND = "/ObtenerVtasPVCtlRendLista";
		private const string GET_VTAS_PV_CTL_REND_DETALLE = "/ObtenerVtasPVCtlRendDetalleLista";
		private const string SET_VTAS_CTL_NUEVO = "/CargaCtlNuevoItemDetalle";
		private const string SET_VTAS_CTL_GUARDAR = "/GuardarCtlDetalle";
		private const string SET_VTAS_CTL_CONFIRMAR = "/ConfirmarCtlArqueo";
		private const string SET_VTAS_CTL_ANULAR = "/AnularCtlArqueo";
		private const string SET_VTAS_CTL_AGREGAR_MEDIO_DE_PAGO = "/AgregarMedioDePago";
		private const string SET_VTAS_CTL_CONFIRMACION_CONTABLE = "/ConfirmacionContable";
		private const string GET_VTAS_PV_CTL_ENTREGA = "/ObtenerVtasPVCtlEntregaLista";
		private const string GET_VTAS_PV_CTL_ENTREGA_REND = "/ObtenerVtasPVCtlEntregaRendLista";
		private const string SET_VTAS_CTL_ENTREGA_CONFIRMR = "/ConfirmarCtlEntrega";
		private const string SET_VTAS_CTL_ENTREGA_ANULAR = "/AnularCtlEntrega";
		private const string GET_ANA_VTA_MES = "/ObtenerAnaVtaMesLista";
		private const string GET_ANA_VTA_DET_DIA = "/ObtenerAnaVtaMesDetalleDiaLista";
		private const string GET_ANA_VTA_DET_HORA = "/ObtenerAnaVtaMesDetalleHoraLista";
		private const string GET_ANA_VTA_DET_SUCURSAL = "/ObtenerAnaVtaMesDetalleSucursalLista";
		private const string GET_ANA_VTA_DET_ANUAL = "/ObtenerAnaVtaMesDetalleAnualLista";
		private const string GET_ANA_VTA_DET_CIERRE = "/ObtenerAnaVtaMesDetalleCierreLista";
		private const string GET_ANA_DE_VAL_DE_VTA_MES = "/ObtenerAnaDeValDeVtaMesLista";
		private const string GET_ANA_DE_VAL_DE_VTA_DET_DIA = "/ObtenerAnaDeValDeVtaDetDiarioLista";
		private const string GET_ANA_DE_VAL_DE_VTA_DET_PV = "/ObtenerAnaDeValDeVtaDetPVLista";
		private const string GET_ANA_DE_VAL_DE_VTA_DET_CB = "/ObtenerAnaDeValDeVtaDetCBLista";

		private const string SORTEOS_LISTA = "/BuscarSorteosLista";
		private const string SORTEOS_DATOS = "/sorteo/";
		private const string SORTEOS_ADM = "/sorteo/adm/";
		private const string SORTEOS_PROD = "/sorteo/prod/";
		private const string SORTEOS_CONFIRMAR = "/sorteo/confirmar";
		private const string SORTEOS_COMPTES_LISTA = "/ObtenerSorteoComptesLista";
		private const string SORTEOS_ANALISIS_PROD_LISTA = "/ObtenerSorteoAnalisisProdLista";

		private const string CAJA_PROCESOS_LISTA = "/ObtenerCajaProcesoLista";
		private const string CAJA_PROCESOS_CIERRES_LISTA = "/ObtenerCajaProcesoCierresLista";


		public ApiVentasServicio(IOptions<AppSettings> options, ILogger<ApiVentasServicio> logger) : base(options, logger, RutaAPI)
		{

		}

		public async Task<RespuestaGenerica<VtasPVCtlProcesoDto>> ObtenerVtasPVCtlProcesosLista(string adm_id, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(token);

				var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_VTAS_PV_CTL_PROCESOS}?adm_id={adm_id}";
				using var response = await client.GetAsync(link);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					var stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						return new() { Ok = false, Mensaje = "No se recibió respuesta válida de VtasPVCtlProceso" };
					}

					var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<VtasPVCtlProcesoDto>>>(stringData)
						?? throw new NegocioException("Error al deserializar los datos");

					if (apiResponse.Data == null)
					{
						return new() { Ok = false, Mensaje = "No se encontraron datos de VtasPVCtlProceso." };
					}

					return new RespuestaGenerica<VtasPVCtlProcesoDto>
					{
						Ok = true,
						ListaEntidad = apiResponse.Data,
						Mensaje = "OK"
					};
				}
				else
				{
					var errorData = await response.Content.ReadAsStringAsync();
					_logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

					return new()
					{
						Ok = false,
						Mensaje = "Error al obtener VtasPVCtlProceso. Si el problema persiste contacte al administrador."
					};
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
				return new RespuestaGenerica<VtasPVCtlProcesoDto>
				{
					Ok = false,
					Mensaje = "Error interno al obtener VtasPVCtlProceso"
				};
			}
		}

		public async Task<RespuestaGenerica<VtasPVCtlCierresDto>> ObtenerVtasPVCtlCierresLista(string caja_nro_proceso, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(token);

				var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_VTAS_PV_CTL_CIERRES}?caja_nro_proceso={caja_nro_proceso}";
				using var response = await client.GetAsync(link);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					var stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						return new() { Ok = false, Mensaje = "No se recibió respuesta válida de VtasPVCtlCierres" };
					}

					var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<VtasPVCtlCierresDto>>>(stringData)
						?? throw new NegocioException("Error al deserializar los datos");

					if (apiResponse.Data == null)
					{
						return new() { Ok = false, Mensaje = "No se encontraron datos de VtasPVCtlCierres." };
					}

					return new RespuestaGenerica<VtasPVCtlCierresDto>
					{
						Ok = true,
						ListaEntidad = apiResponse.Data,
						Mensaje = "OK"
					};
				}
				else
				{
					var errorData = await response.Content.ReadAsStringAsync();
					_logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

					return new()
					{
						Ok = false,
						Mensaje = "Error al obtener VtasPVCtlCierres. Si el problema persiste contacte al administrador."
					};
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
				return new RespuestaGenerica<VtasPVCtlCierresDto>
				{
					Ok = false,
					Mensaje = "Error interno al obtener VtasPVCtlCierres"
				};
			}
		}

		public async Task<RespuestaGenerica<VtasPVCtlRendDto>> ObtenerVtasPVCtlRendLista(string caja_nro_proceso, int caja_nro_cierre, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(token);

				var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_VTAS_PV_CTL_REND}?caja_nro_proceso={caja_nro_proceso}&caja_nro_cierre={caja_nro_cierre}";
				using var response = await client.GetAsync(link);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					var stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						return new() { Ok = false, Mensaje = "No se recibió respuesta válida de VtasPVCtlRend" };
					}

					var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<VtasPVCtlRendDto>>>(stringData)
						?? throw new NegocioException("Error al deserializar los datos");

					if (apiResponse.Data == null)
					{
						return new() { Ok = false, Mensaje = "No se encontraron datos de VtasPVCtlRend." };
					}

					return new RespuestaGenerica<VtasPVCtlRendDto>
					{
						Ok = true,
						ListaEntidad = apiResponse.Data,
						Mensaje = "OK"
					};
				}
				else
				{
					var errorData = await response.Content.ReadAsStringAsync();
					_logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

					return new()
					{
						Ok = false,
						Mensaje = "Error al obtener VtasPVCtlRend. Si el problema persiste contacte al administrador."
					};
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
				return new RespuestaGenerica<VtasPVCtlRendDto>
				{
					Ok = false,
					Mensaje = "Error interno al obtener VtasPVCtlRend"
				};
			}
		}

		public async Task<RespuestaGenerica<VtasPVCtlRendDetalleDto>> ObtenerVtasPVCtlRendDetalleLista(string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, string tcf_id, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(token);

				var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_VTAS_PV_CTL_REND_DETALLE}?caja_nro_proceso={caja_nro_proceso}&caja_nro_cierre={caja_nro_cierre}&caja_nro_rend={caja_nro_rend}&tcf_id={tcf_id}";
				using var response = await client.GetAsync(link);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					var stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						return new() { Ok = false, Mensaje = "No se recibió respuesta válida de VtasPVCtlRendDetalle" };
					}

					var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<VtasPVCtlRendDetalleDto>>>(stringData)
						?? throw new NegocioException("Error al deserializar los datos");

					if (apiResponse.Data == null)
					{
						return new() { Ok = false, Mensaje = "No se encontraron datos de VtasPVCtlRendDetalle." };
					}

					return new RespuestaGenerica<VtasPVCtlRendDetalleDto>
					{
						Ok = true,
						ListaEntidad = apiResponse.Data,
						Mensaje = "OK"
					};
				}
				else
				{
					var errorData = await response.Content.ReadAsStringAsync();
					_logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

					return new()
					{
						Ok = false,
						Mensaje = "Error al obtener VtasPVCtlRendDetalle. Si el problema persiste contacte al administrador."
					};
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
				return new RespuestaGenerica<VtasPVCtlRendDetalleDto>
				{
					Ok = false,
					Mensaje = "Error interno al obtener VtasPVCtlRendDetalle"
				};
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> CargaCtlNuevoItemDetalle(CargaCtlNuevoItemDetalleRequest req, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(req, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{SET_VTAS_CTL_NUEVO}";

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
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error carga nuevo item de detalle" };
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> GuardarCtlDetalle(GuardarCtlDetalleRequest req, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(req, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{SET_VTAS_CTL_GUARDAR}";

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
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al guardar el detalle" };
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> ConfirmarCtlArqueo(ConfirmarCtlArqueoRequest req, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(req, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{SET_VTAS_CTL_CONFIRMAR}";

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
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al confirmar el Arqueo" };
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> AnularCtlArqueo(AnularCtlArqueoRequest req, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(req, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{SET_VTAS_CTL_ANULAR}";

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
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al anular el Arqueo" };
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> AgregarMedioDePago(AgregarMedioDePagoRequest req, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(req, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{SET_VTAS_CTL_AGREGAR_MEDIO_DE_PAGO}";

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
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al agregar un Medio de Pago" };
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> ConfirmacionContable(ConfirmacionContableRequest req, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(req, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{SET_VTAS_CTL_CONFIRMACION_CONTABLE}";

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
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al realizar la Confirmación Contable" };
			}
		}

		public async Task<RespuestaGenerica<VtasPVCtlEntregaDto>> ObtenerVtasPVCtlEntregaLista(string adm_id, string estado, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(token);

				var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_VTAS_PV_CTL_ENTREGA}?adm_id={adm_id}&estado={estado}";
				using var response = await client.GetAsync(link);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					var stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						return new() { Ok = false, Mensaje = "No se recibió respuesta válida de VtasPVCtlEntrega" };
					}

					var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<VtasPVCtlEntregaDto>>>(stringData)
						?? throw new NegocioException("Error al deserializar los datos");

					if (apiResponse.Data == null)
					{
						return new() { Ok = false, Mensaje = "No se encontraron datos de VtasPVCtlEntrega." };
					}

					return new RespuestaGenerica<VtasPVCtlEntregaDto>
					{
						Ok = true,
						ListaEntidad = apiResponse.Data,
						Mensaje = "OK"
					};
				}
				else
				{
					var errorData = await response.Content.ReadAsStringAsync();
					_logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

					return new()
					{
						Ok = false,
						Mensaje = "Error al obtener VtasPVCtlEntrega. Si el problema persiste contacte al administrador."
					};
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
				return new RespuestaGenerica<VtasPVCtlEntregaDto>
				{
					Ok = false,
					Mensaje = "Error interno al obtener VtasPVCtlEntrega"
				};
			}
		}

		public async Task<RespuestaGenerica<VtasPVCtlEntregaRendDto>> ObtenerVtasPVCtlEntregaRendLista(string ent_compte, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(token);

				var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_VTAS_PV_CTL_ENTREGA_REND}?ent_compte={ent_compte}";
				using var response = await client.GetAsync(link);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					var stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						return new() { Ok = false, Mensaje = "No se recibió respuesta válida de VtasPVCtlEntregaRend" };
					}

					var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<VtasPVCtlEntregaRendDto>>>(stringData)
						?? throw new NegocioException("Error al deserializar los datos");

					if (apiResponse.Data == null)
					{
						return new() { Ok = false, Mensaje = "No se encontraron datos de VtasPVCtlEntregaRend." };
					}

					return new RespuestaGenerica<VtasPVCtlEntregaRendDto>
					{
						Ok = true,
						ListaEntidad = apiResponse.Data,
						Mensaje = "OK"
					};
				}
				else
				{
					var errorData = await response.Content.ReadAsStringAsync();
					_logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

					return new()
					{
						Ok = false,
						Mensaje = "Error al obtener VtasPVCtlEntregaRend. Si el problema persiste contacte al administrador."
					};
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
				return new RespuestaGenerica<VtasPVCtlEntregaRendDto>
				{
					Ok = false,
					Mensaje = "Error interno al obtener VtasPVCtlEntregaRend"
				};
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> ConfirmarCtlEntrega(ConfirmarCtlEntregaRequest req, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(req, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{SET_VTAS_CTL_ENTREGA_CONFIRMR}";

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
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al confirmar la Entrega" };
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> AnularCtlEntrega(AnularCtlEntregaRequest req, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(req, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{SET_VTAS_CTL_ENTREGA_ANULAR}";

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
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al anular la Entrega" };
			}
		}

		public List<AnaVtaMesDto> ObtenerAnaVtaMesLista(AnaVtaMesRequest request, string token)
		{
			ApiResponse<List<AnaVtaMesDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_ANA_VTA_MES}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AnaVtaMesDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<AnaVtaMesDetalleDiarioDto> ObtenerAnaVtaMesDetalleDiaLista(AnaVtaMesRequest request, string token)
		{
			ApiResponse<List<AnaVtaMesDetalleDiarioDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_ANA_VTA_DET_DIA}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AnaVtaMesDetalleDiarioDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<AnaVtaMesDetalleHoraDto> ObtenerAnaVtaMesDetalleHoraLista(AnaVtaMesRequest request, string token)
		{
			ApiResponse<List<AnaVtaMesDetalleHoraDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_ANA_VTA_DET_HORA}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AnaVtaMesDetalleHoraDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<AnaVtaMesDetalleSucursalDto> ObtenerAnaVtaMesDetalleSucursalLista(AnaVtaMesRequest request, string token)
		{
			ApiResponse<List<AnaVtaMesDetalleSucursalDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_ANA_VTA_DET_SUCURSAL}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AnaVtaMesDetalleSucursalDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<AnaVtaMesDetalleCierreDto> ObtenerAnaVtaMesDetalleCierreLista(AnaVtaMesRequest request, string token)
		{
			ApiResponse<List<AnaVtaMesDetalleCierreDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_ANA_VTA_DET_CIERRE}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AnaVtaMesDetalleCierreDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<AnaVtaMesDetalleAnualDto> ObtenerAnaVtaMesDetalleAnualLista(AnaVtaMesRequest request, string token)
		{
			ApiResponse<List<AnaVtaMesDetalleAnualDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_ANA_VTA_DET_ANUAL}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AnaVtaMesDetalleAnualDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<AnaValDeVtaMesDto> ObtenerAnaDeValDeVtaMesLista(AnaDeValDeVtaMesRequest request, string token)
		{
			ApiResponse<List<AnaValDeVtaMesDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_ANA_DE_VAL_DE_VTA_MES}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AnaValDeVtaMesDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<AnaValDeVtaDetDiarioDto> ObtenerAnaDeValDeVtaDetDiarioLista(AnaDeValDeVtaMesRequest request, string token)
		{
			ApiResponse<List<AnaValDeVtaDetDiarioDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_ANA_DE_VAL_DE_VTA_DET_DIA}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AnaValDeVtaDetDiarioDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<AnaValDeVtaDetPVDto> ObtenerAnaDeValDeVtaDetPVLista(AnaDeValDeVtaMesRequest request, string token)
		{
			ApiResponse<List<AnaValDeVtaDetPVDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_ANA_DE_VAL_DE_VTA_DET_PV}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AnaValDeVtaDetPVDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<AnaValDeVtaDetCBDto> ObtenerAnaDeValDeVtaDetCBLista(AnaDeValDeVtaMesRequest request, string token)
		{
			ApiResponse<List<AnaValDeVtaDetCBDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_ANA_DE_VAL_DE_VTA_DET_CB}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AnaValDeVtaDetCBDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<RespuestaGenerica<SorteoCargaListaDto>> BuscarSorteosLista(QueryFilters filtro, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(filtro, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{SORTEOS_LISTA}";

				using var response = await client.PostAsync(link, contentData);
				if (response.StatusCode == HttpStatusCode.OK)
				{
					var stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
					}

					var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<SorteoCargaListaDto>>>(stringData);
					if (apiResponse == null || apiResponse.Data == null)
					{
						return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
					}

					return new RespuestaGenerica<SorteoCargaListaDto>
					{
						Ok = true,
						Mensaje = "OK",
						ListaEntidad = apiResponse.Data,
						Meta = apiResponse.Meta ?? new()
					};
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
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al buscar Sorteos" };
			}
		}

		public async Task<RespuestaGenerica<SorteoCargaDatosDto>> ObtenerSorteoDatos(string so_sorteo, string token)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(so_sorteo))
				{
					return new() { Ok = false, Mensaje = "Debe indicar el identificador del sorteo." };
				}

				var link = $"{_appSettings.RutaBase}{RutaAPI}{SORTEOS_DATOS}{so_sorteo}";
				return await GetListaAsync<SorteoCargaDatosDto>(link, token, "Error al obtener el Sorteo");
			}
			catch (Exception ex)
			{
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al obtener el Sorteo" };
			}
		}

		public async Task<RespuestaGenerica<SorteoCargaAdmDto>> ObtenerSorteoAdmDatos(string so_sorteo, string token)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(so_sorteo))
					return new() { Ok = false, Mensaje = "Debe indicar el identificador del sorteo." };

				var link = $"{_appSettings.RutaBase}{RutaAPI}{SORTEOS_ADM}{so_sorteo}";
				return await GetListaAsync<SorteoCargaAdmDto>(link, token, "Error al obtener las sucursales del Sorteo");
			}
			catch (Exception ex)
			{
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al obtener las sucursales del Sorteo" };
			}
		}

		public async Task<RespuestaGenerica<SorteoCargaProdDto>> ObtenerSorteoProdDatos(string so_sorteo, string token)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(so_sorteo))
					return new() { Ok = false, Mensaje = "Debe indicar el identificador del sorteo." };

				var link = $"{_appSettings.RutaBase}{RutaAPI}{SORTEOS_PROD}{so_sorteo}";
				return await GetListaAsync<SorteoCargaProdDto>(link, token, "Error al obtener los productos del Sorteo");
			}
			catch (Exception ex)
			{
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al obtener el Sorteo" };
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> ConfirmarSorteo(ConfirmarSorteoRequest req, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(req, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{SORTEOS_CONFIRMAR}";

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
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al buscar Presupuestos" };
			}
		}

		public List<SorteoComptesDto> ObtenerSorteoComptesLista(SorteoCompteRequest request, string token)
		{
			ApiResponse<List<SorteoComptesDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{SORTEOS_COMPTES_LISTA}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<SorteoComptesDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public List<SorteoAnalisisProdDto> ObtenerSorteoAnalisisProdLista(SorteoAnalisisProdRequest request, string token)
		{
			ApiResponse<List<SorteoAnalisisProdDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{SORTEOS_ANALISIS_PROD_LISTA}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<SorteoAnalisisProdDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<RespuestaGenerica<CajaProcesoListaDto>> ObtenerCajaProcesoLista(CajaProcesoListaRequest request, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(request, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{CAJA_PROCESOS_LISTA}";

				using var response = await client.PostAsync(link, contentData);
				if (response.StatusCode == HttpStatusCode.OK)
				{
					var stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
					}

					var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<CajaProcesoListaDto>>>(stringData);
					if (apiResponse == null || apiResponse.Data == null)
					{
						return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
					}

					return new RespuestaGenerica<CajaProcesoListaDto>
					{
						Ok = true,
						Mensaje = "OK",
						ListaEntidad = apiResponse.Data,
						Meta = apiResponse.Meta ?? new()
					};
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
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al buscar Procesos de Cierre de Caja" };
			}
		}

		public async Task<RespuestaGenerica<CajaProcesoCierresListaDto>> ObtenerCajaProcesoCierresLista(string id, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(token);

				var link = $"{_appSettings.RutaBase}{RutaAPI}{CAJA_PROCESOS_CIERRES_LISTA}?caja_nro_proceso={id}";
				using var response = await client.GetAsync(link);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					var stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						return new() { Ok = false, Mensaje = "No se recibió respuesta válida de ObtenerCajaProcesoCierresLista" };
					}

					var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<CajaProcesoCierresListaDto>>>(stringData)
						?? throw new NegocioException("Error al deserializar los datos");

					if (apiResponse.Data == null)
					{
						return new() { Ok = false, Mensaje = "No se encontraron datos de ObtenerCajaProcesoCierresLista." };
					}

					return new RespuestaGenerica<CajaProcesoCierresListaDto>
					{
						Ok = true,
						ListaEntidad = apiResponse.Data,
						Mensaje = "OK"
					};
				}
				else
				{
					var errorData = await response.Content.ReadAsStringAsync();
					_logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

					return new()
					{
						Ok = false,
						Mensaje = "Error al obtener ObtenerCajaProcesoCierresLista. Si el problema persiste contacte al administrador."
					};
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
				return new RespuestaGenerica<CajaProcesoCierresListaDto>
				{
					Ok = false,
					Mensaje = "Error interno al obtener ObtenerCajaProcesoCierresLista"
				};
			}
		}
	}
}
