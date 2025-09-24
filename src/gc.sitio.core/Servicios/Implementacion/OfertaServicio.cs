using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class OfertaServicio : Servicio<Dto>, IOfertaServicio
    {
        private const string RutaAPI = "/api/apioferta";

        private const string CONOCER_ESTADO_OFERTA = "/conocer-estado-oferta";
        private const string BUSCAR_CANALES = "/buscar-canales";
        private const string ALTA_OFERTA = "/confirmacion-alta-oferta";
        private const string OBTENER_ESTADO_OFERTA_PRODUCTO = "/obtener-estado-oferta-producto";
        
        private const string OBTENER_OFERTAS_SIN_ACTIVAR = "/obtener-ofertas-sin-activar";
        private const string ACTIVAR_OFERTA = "/activacion-de-oferta";
        private const string ACTUALIZAR_OFERTA_VENCIDA_SIN_ACTIVAR = "/actualizar-oferta-vencida-sin-activar";
        private const string CARGAR_ACTIVAS_A_SINACT = "/cargar-activas-a-sin-activar";
        private const string ELIMINAR_OFERTAS = "/eliminar-ofertas";

        private const string OBTENER_OFERTAS_ACTIVAS = "/obtener-ofertas-activas";
        private const string ELIMINA_OFERTAS_ACTIVAS = "/elimina-ofertas-activas";
        private const string COPIAR_A_CANAL = "/copiar-a-canal";


        public OfertaServicio(IOptions<AppSettings> options, ILogger<OfertaServicio> logger) : base(options, logger)
        {
        }

        public async Task<RespuestaGenerica<RespuestaDto>> ActivacionDeOferta(AbmPlusGenDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{ACTIVAR_OFERTA}";

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "No se recibió respuesta válida de la API"
                        };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);

                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "Error deserializando la respuesta de la API"
                        };
                    }

                    if (apiResponse.Data.resultado != 0)
                    {
                        if (apiResponse.Data.resultado > 0)
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsWarn = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando la activacion de la oferta."
                            };
                        }
                        else
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsError = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando la activacion de la oferta."
                            };
                        }
                    }
                    else
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = true,
                            Entidad = apiResponse?.Data ?? new RespuestaDto(),
                            Mensaje = "Importación procesada exitosamente"
                        };
                    }
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(errorData);
                    var mensaje = error?.Detail ?? "Error desconocido en la API";

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ConfirmacionAltaOferta");

                return new RespuestaGenerica<RespuestaDto>
                {
                    Ok = false,
                    Mensaje = "Error interno procesando el Alta de la Oferta"
                };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> ActualizarOfertaVencidaSinActivar(AbmGenDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{ACTUALIZAR_OFERTA_VENCIDA_SIN_ACTIVAR}";

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "No se recibió respuesta válida de la API"
                        };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);

                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "Error deserializando la respuesta de la API"
                        };
                    }

                    if (apiResponse.Data.resultado != 0)
                    {
                        if (apiResponse.Data.resultado > 0)
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsWarn = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando la actualización de la oferta vencida sin activar."
                            };
                        }
                        else
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsError = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando la activacion de la oferta."
                            };
                        }
                    }
                    else
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = true,
                            Entidad = apiResponse?.Data ?? new RespuestaDto(),
                            Mensaje = "Importación procesada exitosamente"
                        };
                    }
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(errorData);
                    var mensaje = error?.Detail ?? "Error desconocido en la API";

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ConfirmacionAltaOferta");

                return new RespuestaGenerica<RespuestaDto>
                {
                    Ok = false,
                    Mensaje = "Error interno procesando el Alta de la Oferta"
                };
            }
        }

        public async Task<RespuestaGenerica<CanalDto>> BuscarCanales(string token)
        {
            try
            {
                ApiResponse<List<CanalDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_CANALES}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<CanalDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<CanalDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<CanalDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los canales" };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> CargarActivasASinActivar(AbmGenDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{CARGAR_ACTIVAS_A_SINACT}";

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "No se recibió respuesta válida de la API"
                        };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);

                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "Error deserializando la respuesta de la API"
                        };
                    }

                    if (apiResponse.Data.resultado != 0)
                    {
                        if (apiResponse.Data.resultado > 0)
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsWarn = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando la carga de ofertas activas a sin activar."
                            };
                        }
                        else
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsError = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando la carga de ofertas activas a sin activar."
                            };
                        }
                    }
                    else
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = true,
                            Entidad = apiResponse?.Data ?? new RespuestaDto(),
                            Mensaje = "Importación procesada exitosamente"
                        };
                    }
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(errorData);
                    var mensaje = error?.Detail ?? "Error desconocido en la API";

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ConfirmacionAltaOferta");

                return new RespuestaGenerica<RespuestaDto>
                {
                    Ok = false,
                    Mensaje = "Error interno procesando la carga de ofertas activas a sin activar"
                };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> ConfirmacionAltaOferta(AbmPlusGenDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{ALTA_OFERTA}";

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "No se recibió respuesta válida de la API"
                        };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);

                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "Error deserializando la respuesta de la API"
                        };
                    }

                    if (apiResponse.Data.resultado != 0)
                    {
                        if (apiResponse.Data.resultado > 0)
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsWarn=true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando el alta de la oferta."
                            };
                        }
                        else
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsError = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando el alta de la oferta."
                            };
                        }
                    }
                    else
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = true,
                            Entidad = apiResponse?.Data ?? new RespuestaDto(),
                            Mensaje = "Importación procesada exitosamente"
                        };
                    }
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(errorData);
                    var mensaje = error?.Detail ?? "Error desconocido en la API";

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ConfirmacionAltaOferta");

                return new RespuestaGenerica<RespuestaDto>
                {
                    Ok = false,
                    Mensaje = "Error interno procesando el Alta de la Oferta"
                };
            }
        }

        public async Task<RespuestaGenerica<string>> ConocerEstadoOferta(string p_id, string admId, string lp_id, string token)
        {
            try
            {
                ApiResponse<string> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{CONOCER_ESTADO_OFERTA}?p_id={p_id}&admId={admId}&lp_id={lp_id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<string>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<string> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<string> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el estado de la oferta" };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> EliminarOfertas(AbmPlusGenDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{ELIMINAR_OFERTAS}";

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "No se recibió respuesta válida de la API"
                        };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);

                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "Error deserializando la respuesta de la API"
                        };
                    }

                    if (apiResponse.Data.resultado != 0)
                    {
                        if (apiResponse.Data.resultado > 0)
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsWarn = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando la eliminación de la oferta."
                            };
                        }
                        else
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsError = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando la eliminación de la oferta."
                            };
                        }
                    }
                    else
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = true,
                            Entidad = apiResponse?.Data ?? new RespuestaDto(),
                            Mensaje = ""
                        };
                    }
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(errorData);
                    var mensaje = error?.Detail ?? "Error desconocido en la API";

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EliminarOfertas");

                return new RespuestaGenerica<RespuestaDto>
                {
                    Ok = false,
                    Mensaje = "Error interno procesando la eliminación de la Oferta"
                };
            }
        }

        public async Task<RespuestaGenerica<OfertaEstadoDto>> ObtenerEstadoOfertaProducto(string p_id, string token)
        {
            try
            {
                ApiResponse<List<OfertaEstadoDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_ESTADO_OFERTA_PRODUCTO}?p_id={p_id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OfertaEstadoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<OfertaEstadoDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<OfertaEstadoDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener Ofertas, Promo o Combos del producto" };
            }
        }

        public async Task<RespuestaGenerica<OfertaDto>> ObtenerOfertasSinActivar(string admId, string lp_id, string token)
        {
            try
            {
                ApiResponse<List<OfertaDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_OFERTAS_SIN_ACTIVAR}?admId={admId}&lp_id={lp_id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OfertaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<OfertaDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<OfertaDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener las Ofertas sin Activar" };
            }
        }

        public async Task<RespuestaGenerica<OfertaDto>> ObtenerOfertasActivas(string admId, string lp_id, string token)
        {
            try
            {
                ApiResponse<List<OfertaDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_OFERTAS_ACTIVAS}?admId={admId}&lp_id={lp_id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OfertaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<OfertaDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<OfertaDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener las Ofertas Activas" };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> EliminaOfertasActivas(AbmGenDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{ELIMINA_OFERTAS_ACTIVAS}";

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "No se recibió respuesta válida de la API"
                        };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);

                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "Error deserializando la respuesta de la API"
                        };
                    }

                    if (apiResponse.Data.resultado != 0)
                    {
                        if (apiResponse.Data.resultado > 0)
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsWarn = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando la eliminación de la oferta activa."
                            };
                        }
                        else
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsError = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando la eliminación de la oferta activa."
                            };
                        }
                    }
                    else
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = true,
                            Entidad = apiResponse?.Data ?? new RespuestaDto(),
                            Mensaje = ""
                        };
                    }
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(errorData);
                    var mensaje = error?.Detail ?? "Error desconocido en la API";

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EliminarOfertas");

                return new RespuestaGenerica<RespuestaDto>
                {
                    Ok = false,
                    Mensaje = "Error interno procesando la eliminación de la Oferta Activa"
                };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> CopiarACanal(AbmGenDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{COPIAR_A_CANAL}";

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "No se recibió respuesta válida de la API"
                        };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);

                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "Error deserializando la respuesta de la API"
                        };
                    }

                    if (apiResponse.Data.resultado != 0)
                    {
                        if (apiResponse.Data.resultado > 0)
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsWarn = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando la copia de oferta a canal."
                            };
                        }
                        else
                        {
                            return new RespuestaGenerica<RespuestaDto>
                            {
                                Ok = false,
                                Entidad = apiResponse.Data,
                                EsError = true,
                                Mensaje = apiResponse.Data.resultado_msj ?? "Error procesando la copia de oferta a canal."
                            };
                        }
                    }
                    else
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = true,
                            Entidad = apiResponse?.Data ?? new RespuestaDto(),
                            Mensaje = ""
                        };
                    }
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(errorData);
                    var mensaje = error?.Detail ?? "Error desconocido en la API";

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EliminarOfertas");

                return new RespuestaGenerica<RespuestaDto>
                {
                    Ok = false,
                    Mensaje = "Error procesando la copia de oferta a canal."
                };
            }
        }

    }
}
