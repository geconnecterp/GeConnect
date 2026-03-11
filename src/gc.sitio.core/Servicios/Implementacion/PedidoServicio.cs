using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion
{
	public class PedidoServicio : Servicio<Dto>, IPedidoServicio
	{
		private const string RutaAPI = "/api/apipedido";
		private const string BUSCAR_PEDIDOS = "/buscar-pedidos";
		private const string OBTENER_PEDIDO = "/pedido/";
		private const string OBTENER_DETALLE = "/pedido/detalle/";
		private const string CONFIRMAR_PEDIDO = "/pedido/confirmar";

		public PedidoServicio(IOptions<AppSettings> options, ILogger<PedidoServicio> logger) : base(options, logger)
		{

		}

		public async Task<RespuestaGenerica<PedidoListDto>> BuscarPedidos(QueryFilters filtro, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(filtro, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_PEDIDOS}";

				using var response = await client.PostAsync(link, contentData);
				if (response.StatusCode == HttpStatusCode.OK)
				{
					var stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{
						return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
					}

					var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PedidoListDto>>>(stringData);
					if (apiResponse == null || apiResponse.Data == null)
					{
						return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
					}

					return new RespuestaGenerica<PedidoListDto>
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
				return new() { Ok = false, Mensaje = "Error al buscar Pedidos" };
			}
		}

		public async Task<RespuestaGenerica<PedidoProductoDto>> ObtenerDetalleDePedido(string pcCompte, string token)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(pcCompte))
				{
					return new() { Ok = false, Mensaje = "Debe indicar el identificador del pedido." };
				}

				var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_DETALLE}{pcCompte}";
				return await GetListaAsync<PedidoProductoDto>(link, token, "Error al obtener el Detalle del Pedido");
			}
			catch (Exception ex)
			{
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al obtener el Detalle del Pedido" };
			}
		}

		public async Task<RespuestaGenerica<PedidoDto>> ObtenerPedido(string pcCompte, string token)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(pcCompte))
				{
					return new() { Ok = false, Mensaje = "Debe indicar el identificador del pedido de cliente." };
				}

				var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_PEDIDO}{pcCompte}";
				return await GetListaAsync<PedidoDto>(link, token, "Error al obtener el Pedido");
			}
			catch (Exception ex)
			{
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al obtener el Pedido" };
			}
		}

		public async Task<RespuestaGenerica<RespuestaDto>> ConfirmarPedido(ConfirmarPedidoRequest req, string token)
		{
			try
			{
				var helper = new HelperAPI();
				var client = helper.InicializaCliente(req, token, out StringContent contentData);
				var link = $"{_appSettings.RutaBase}{RutaAPI}{CONFIRMAR_PEDIDO}";

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
