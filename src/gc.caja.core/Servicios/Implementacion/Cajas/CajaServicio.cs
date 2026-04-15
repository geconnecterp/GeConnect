using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.caja.core.Servicios.Implementacion.Seguridad
{
    public class CajaServicio : Servicio<Dto>, ICajaServicio
    {
        private const string RutaAPI = "/api/apicaja";

        private const string POST_VALIDA_INTEGRIDAD = "/ValidaIntegridadUsuarioCaja";
        private const string POST_APERTURA_CAJA = "/AperturaCaja";
        private const string POST_CIERRE_CAJA = "/CierreCaja";
        private const string GET_BUSQUEDA_CUENTA = "/BusquedaClientes";
        private const string GET_BUSQUEDA_DATOS_CLIENTE = "/BuscarDatosCliente";
        private const string POST_OBTENER_PRODUCTO_DATOS = "/ObtenerProductoDatos";
        private const string POST_CARGAR_CF = "/Cargar_CF";
        private const string GET_OBTENER_DATOS_CF = "/ObtenerDatosCF";
        private const string POST_CIERRE_CAJA_GRAL = "/CierreCajaGral";
        private const string POST_HABILITAR_CAJA_GRAL = "/HabilitarCajaGral";

        public CajaServicio(IOptions<AppSettings> options, ILogger<CajaServicio> logger):base(options,logger)
        {
        }

        public async Task<RespuestaGenerica<RespuestaDto>> ValidarIntegridadUsuarioCaja(CajaReqDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_VALIDA_INTEGRIDAD}";

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
                    if (resp.resultado == 0 || resp.resultado == 3)
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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Hubo un error al intentar validar la integridad del usuario en la caja" };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> AperturaCaja(CajaReqDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_APERTURA_CAJA}";

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
                    if (resp.resultado == 0 || resp.resultado == 3)
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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Hubo un error al intentar realizar la apertura de caja." };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> CierreCaja(CajaReqDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_CIERRE_CAJA}";

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Hubo un error al intentar realizar el cierre de caja." };
            }
        }

        public async Task<RespuestaGenerica<CuentaBusquedaResultadoDto>> BusquedaClientes(string busqueda, string adm_id, string usu_id , string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_BUSQUEDA_CUENTA}?busqueda={busqueda}&adm_id={adm_id}&usu_id={usu_id}";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la búsqueda de cuenta" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<CuentaBusquedaResultadoDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    return new RespuestaGenerica<CuentaBusquedaResultadoDto>
                    {
                        Ok = true,
                        ListaEntidad = apiResponse.Data,
                        Mensaje = "OK"
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
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<CuentaBusquedaResultadoDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al buscar la cuenta en caja"
                };
            }
        }

        public async Task<RespuestaGenerica<ProductoDatosResponseDto>> ObtenerProductoDatos(ProductoDatosRequestDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_OBTENER_PRODUCTO_DATOS}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ProductoDatosResponseDto>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    var resp = apiResponse.Data;
                    if (resp.respuesta == 0)
                    {
                        return new RespuestaGenerica<ProductoDatosResponseDto>
                        {
                            Ok = true,
                            Mensaje = "OK",
                            Entidad = apiResponse.Data
                        };
                    }
                    else if (resp.respuesta > 0)
                    {
                        return new RespuestaGenerica<ProductoDatosResponseDto>
                        {
                            Ok = false,
                            EsWarn = true,
                            EsError = false,
                            Mensaje = resp.respuesta_msj,
                            Entidad = apiResponse.Data
                        };
                    }
                    else
                    {
                        return new RespuestaGenerica<ProductoDatosResponseDto>
                        {
                            Ok = false,
                            EsWarn = false,
                            EsError = true,
                            Mensaje = resp.respuesta_msj,
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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener los datos del producto" };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> Cargar_CF(CargaCFRequestDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_CARGAR_CF}";

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al cargar el cliente final" };
            }
        }

        public async Task<RespuestaGenerica<CajaDatosDto>> ObtenerDatosCF(string caja_id, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_OBTENER_DATOS_CF}?caja_id={caja_id}";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de los datos de CF" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CajaDatosDto>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    return new RespuestaGenerica<CajaDatosDto>
                    {
                        Ok = true,
                        Entidad = apiResponse.Data,
                        Mensaje = "OK"
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
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<CajaDatosDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al obtener los datos de caja CF"
                };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> CierreCajaGral(string usu_id, string adm_id, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var parametros = new { usu_id, adm_id };
                var client = helper.InicializaCliente(parametros, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_CIERRE_CAJA_GRAL}";

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al realizar el cierre general de caja" };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> HabilitarCajaGral(string usu_id, string adm_id, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var parametros = new { usu_id, adm_id };
                var client = helper.InicializaCliente(parametros, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_HABILITAR_CAJA_GRAL}";

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al habilitar la caja general" };
            }
        }

        public async Task<CajaSettings> ObtenerAsync(string ruta)
        {
            CajaSettings c = new();

            if (string.IsNullOrWhiteSpace(ruta))
            {
                throw new InvalidOperationException("No se recepcionó la ruta de acceso a la configuración de CAJA.");
            }

            if (!File.Exists(ruta))
            {
                throw new FileNotFoundException("No se encontró el archivo de configuración de caja .");
            }

            string json = await File.ReadAllTextAsync(ruta);

            var cajaSettings = JsonConvert.DeserializeObject<CajaSettings>(json);

            if (cajaSettings is null)
            {
                throw new InvalidOperationException("El contenido del JSON no pudo convertirse a CajaSettings.");
            }

            return cajaSettings;
        }

        public async Task<RespuestaGenerica<CuentaDatosResultadoDto>> BusquedaDatosCliente(string origen, string valor, string adm_id, string usu_id, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_BUSQUEDA_DATOS_CLIENTE}?origen={origen}&valor={valor}&adm_id={adm_id}&usu_id={usu_id}";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la búsqueda de datos del cliente" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CuentaDatosResultadoDto>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    return new RespuestaGenerica<CuentaDatosResultadoDto>
                    {
                        Ok = true,
                        Entidad = apiResponse.Data,
                        Mensaje = "OK"
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
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<CuentaDatosResultadoDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al buscar los datos del cliente"
                };
            }
        }
    }
}
