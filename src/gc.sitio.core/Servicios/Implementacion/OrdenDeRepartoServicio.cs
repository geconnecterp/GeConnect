using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion
{
	public class OrdenDeRepartoServicio : Servicio<Dto>, IOrdenDeRepartoServicio
	{
		private const string RutaAPI = "/api/apiordendereparto";
		private const string OBTENER_ESTADOS = "/estados";
		private const string BUSCAR_ORDENES = "/buscar-ordenes-de-reparto";
		private const string BUSCAR_PEDIDOS_EN_ORDEN = "/buscar-pedidos-en-orden-de-reparto/";
		private const string CONFIRMAR_ORDEN = "/orden-de-reparto/confirmar";

		public OrdenDeRepartoServicio(IOptions<AppSettings> options, ILogger<OrdenDeRepartoServicio> logger) : base(options, logger)
		{

		}

		public async Task<RespuestaGenerica<OrdenDeRepartoListaDto>> BuscarOrdenesDeReparto(QueryFilters filtro, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(filtro, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_ORDENES}";

				using var response = await client.PostAsync(link, contentData);
				if (response.StatusCode == HttpStatusCode.OK)
				{
					var stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
					}

					var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OrdenDeRepartoListaDto>>>(stringData);
					if (apiResponse == null || apiResponse.Data == null)
					{
						return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
					}

					return new RespuestaGenerica<OrdenDeRepartoListaDto>
					{
						Ok = true,
						Mensaje = "OK",
						ListaEntidad = apiResponse.Data
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
				return new() { Ok = false, Mensaje = "Error al buscar ordenes de reparto" };
			}
		}

		public async Task<RespuestaGenerica<OrdenDeRepartoEstadoDto>> ObtenerEstadosDeOrdenDeReparto(string token)
		{
			try
			{
				var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_ESTADOS}";
				return await GetListaAsync<OrdenDeRepartoEstadoDto>(link, token, "Error al obtener los Estados de Orden de Reparto");
			}
			catch (Exception ex)
			{
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al obtener los Estados de Orden de Reparto" };
			}
		}

		public async Task<RespuestaGenerica<PedidoEnOrdenDeRepartoDto>> ObtenerPedidosDeLaOrdenDeReparto(string orCompte, string token)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(orCompte))
				{
					return new() { Ok = false, Mensaje = "Debe indicar el identificador de la orden." };
				}

				var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_PEDIDOS_EN_ORDEN}{orCompte}";
				return await GetListaAsync<PedidoEnOrdenDeRepartoDto>(link, token, "Error al obtener los Pedidos de la Orden");
			}
			catch (Exception ex)
			{
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al obtener los Pedidos de la Orden" };
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> ConfirmarOrdenDeReparto(ConfirmaOrdenDeRepartoRequest req, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(req, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{CONFIRMAR_ORDEN}";

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
	}
}
