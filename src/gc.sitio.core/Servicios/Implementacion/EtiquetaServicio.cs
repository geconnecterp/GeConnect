using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class EtiquetaServicio : Servicio<Dto>, IEtiquetaServicio
    {
        private const string RutaAPI = "/api/apiEtiqueta";
        private const string OBTENER_CARGA_PREVIA = "/ObtenerCargaPreviaUsuario/";
        private const string OBTENER_DETALLE_ETIQUETAS = "/ObtenerDetalleEtiquetas";

        public EtiquetaServicio(IOptions<AppSettings> options, ILogger<EtiquetaServicio> logger) : base(options, logger)
        {

        }

        public async Task<RespuestaGenerica<CargaPreviaDto>> ObtenerCargaPrevia(string adm_id, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(adm_id))
                {
                    return new() { Ok = false, Mensaje = "Debe indicar la sucursal actual." };
                }

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_CARGA_PREVIA}{adm_id}";
                return await GetListaAsync<CargaPreviaDto>(link, token, "Error al indicar la administración");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener el Presupuesto" };
            }
        }

        public async Task<RespuestaGenerica<IEDetalleDto>> ObtenerDetalleEtiquetas(QueryFilters filtro, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(filtro, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_DETALLE_ETIQUETAS}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<IEDetalleDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    return new RespuestaGenerica<IEDetalleDto>
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
                return new() { Ok = false, Mensaje = "Error al buscar Presupuestos" };
            }
        }
    }
}
