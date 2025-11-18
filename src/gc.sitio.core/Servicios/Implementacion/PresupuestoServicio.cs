using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class PresupuestoServicio : Servicio<Dto>, IPresupuestoServicio
    {
        private const string RutaAPI = "/api/apipresupuesto";
        private const string BUSCAR_PRESUPUESTOS = "/buscar-presupuestos";
        private const string OBTENER_PRESUPUESTO = "/presupuesto/";
        private const string OBTENER_DETALLE = "/presupuesto/detalle/";
        private const string OBTENER_DETALLE_ACTUALIZADO = "/presupuesto/detalle/actualizado/";
        private const string OBTENER_ESTADOS = "/estados";
        private const string OBTENER_TIPOS = "/tipos";
        private const string CONFIRMAR_PRESUPUESTO = "/presupuesto/confirmar";

        public PresupuestoServicio(IOptions<AppSettings> options, ILogger<PresupuestoServicio> logger) : base(options, logger)
        {

        }

        public async Task<RespuestaGenerica<RespuestaDto>> ConfirmarPresupuesto(AbmPlusGenDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{CONFIRMAR_PRESUPUESTO}";

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
                            // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
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
                            // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
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
                            // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
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

        public async Task<RespuestaGenerica<PresupuestoListDto>> BuscarPresupuestos(QueryFilters filtro, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(filtro, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_PRESUPUESTOS}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PresupuestoListDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    return new RespuestaGenerica<PresupuestoListDto>
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

        public async Task<RespuestaGenerica<PresupuestoDto>> ObtenerPresupuesto(string id, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return new() { Ok = false, Mensaje = "Debe indicar el identificador del presupuesto." };
                }

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_PRESUPUESTO}{id}";
                return await GetListaAsync<PresupuestoDto>(link, token, "Error al obtener el Presupuesto");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener el Presupuesto" };
            }
        }

        public async Task<RespuestaGenerica<PresupuestoProductoDto>> ObtenerDetallePresupuesto(string id, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return new() { Ok = false, Mensaje = "Debe indicar el identificador del presupuesto." };
                }

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_DETALLE}{id}";
                return await GetListaAsync<PresupuestoProductoDto>(link, token, "Error al obtener el Detalle del Presupuesto");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener el Detalle del Presupuesto" };
            }
        }

        public async Task<RespuestaGenerica<PresupuestoProductoDto>> ObtenerDetallePresupuestoActualizado(string id, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return new() { Ok = false, Mensaje = "Debe indicar el identificador del presupuesto." };
                }

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_DETALLE_ACTUALIZADO}{id}";
                return await GetListaAsync<PresupuestoProductoDto>(link, token, "Error al obtener el Detalle del Presupuesto Actualizado");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener el Detalle del Presupuesto Actualizado" };
            }
        }

        public async Task<RespuestaGenerica<PresupE>> ObtenerEstadosPresupuesto(string token)
        {
            try
            {
                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_ESTADOS}";
                return await GetListaAsync<PresupE>(link, token, "Error al obtener los Estados de Presupuesto");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener los Estados de Presupuesto" };
            }
        }
        public async Task<RespuestaGenerica<PresupT>> ObtenerTiposPresupuesto(string token)
        {
            try
            {
                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_TIPOS}";
                return await GetListaAsync<PresupT>(link, token, "Error al obtener los Tipos de Presupuesto");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener los Tipos de Presupuesto" };
            }
        }

        


    }
}
