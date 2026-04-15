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
	}
}
