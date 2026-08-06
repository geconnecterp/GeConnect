using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using gc.infraestructura.Dtos.Productos.Precio;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class PrecioListaServicio : Servicio<Dto>, IPrecioListaServicio
    {
        private const string RutaAPI = "/api/apipreciolista";
        private const string OBTENER_LISTA_PRECIOS = "/ObtenerListaPrecios/";
        private const string OBTENER_LISTA_DETALLE = "/ObtenerDetallePrecios";
		private const string OBTENER_LISTA_RUB_CTA = "/ObtenerListaPreciosRubCta";

		public PrecioListaServicio(IOptions<AppSettings> options, ILogger<EtiquetaServicio> logger) : base(options, logger)
        {

        }

        public async Task<RespuestaGenerica<PrecioListaDetalleDto>> ObtenerDetallePrecios(QueryFilters filtro, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(filtro, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_LISTA_DETALLE}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PrecioListaDetalleDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    return new RespuestaGenerica<PrecioListaDetalleDto>
                    {
                        Ok = true,
                        Mensaje = "OK",
                        ListaEntidad = apiResponse.Data
                        // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
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
                return new() { Ok = false, Mensaje = "Error al buscar la lista de precios" };
            }
            ;
        }

        public async Task<RespuestaGenerica<PrecioListaDto>> ObtenerListaPrecios(string token)
        {
            try
            {               

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_LISTA_PRECIOS}";
                return await GetListaAsync<PrecioListaDto>(link, token, "Error al indicar la administración");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener la Lista de Precios" };
            }
        }

		public async Task<RespuestaGenerica<ListaPrecioRubCtaDto>> ObtenerListaPreciosRubCta(string lp_id, string token)
		{
			try
			{

				var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_LISTA_RUB_CTA}?id={lp_id}";
				return await GetListaAsync<ListaPrecioRubCtaDto>(link, token, "Error al indicar la administración");
			}
			catch (Exception ex)
			{
				_logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
				return new() { Ok = false, Mensaje = "Error al obtener la Lista de Precios Por Rubro/Cuenta" };
			}
		}
	}
}
