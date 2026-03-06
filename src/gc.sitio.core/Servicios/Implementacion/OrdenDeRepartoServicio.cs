using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
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
	}
}
