using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
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
	}
}
