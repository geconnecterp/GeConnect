using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Precio;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.caja.core.Servicios.Implementacion.Cajas
{
    public class ProductoFactServicio : Servicio<Dto>, IProductoFactServicio
    {
        private const string RutaAPI = "/api/apiproductofact";
       

        private const string RutaAPIProducto = "/api/apiproducto";

        private const string POST_OBTENER_PRODUCTO_DATOS = "/ObtenerProductoDatos";
        private const string BUSCAR_PROD = "/ProductoBuscar";
        private const string BUSCAR_LISTA = "/ProductoListaBuscar";
        private const string POST_CALCULAR_FILAS = "/CalcularFilas"; // ✅ NUEVO
        private const string POST_OBTENER_PREFACTURA = "/ObtenerPrefactura";
        private const string POST_OBTENER_COTIZACION = "/ObtenerCotizacion";
        private const string POST_CREAR_PREF_DIFERIDA = "/CrearPrefacturaDiferida";
        private const string POST_CREAR_PAGO_DIFERIDO = "/CrearPagoDiferido";
        private const string GET_LISTAS_PRECIOS = "/api/ApiPrecioLista/ObtenerListaPrecios";

        

        public ProductoFactServicio(IOptions<AppSettings> options, ILogger<ProductoFactServicio> logger) : base(options, logger)
        {
        }

        public async Task<RespuestaGenerica<PrecioListaDto>> ObtenerListasPrecios(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
            }

            try
            {
                var helper = new HelperAPI();
                using var client = helper.InicializaCliente(token);
                var link = $"{_appSettings.RutaBase}{GET_LISTAS_PRECIOS}";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedException("Debe autenticarse nuevamente para continuar.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    var mensaje = await ReadApiErrorAsync(response);
                    _logger.LogWarning(
                        "No se pudo obtener el catálogo de listas de precios. Estado={StatusCode}, Detalle={Detalle}",
                        (int)response.StatusCode,
                        mensaje);

                    return new RespuestaGenerica<PrecioListaDto>
                    {
                        Ok = false,
                        EsError = true,
                        Mensaje = mensaje,
                        ListaEntidad = []
                    };
                }

                var contenido = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PrecioListaDto>>>(contenido);
                if (apiResponse?.Data is null)
                {
                    return new RespuestaGenerica<PrecioListaDto>
                    {
                        Ok = false,
                        EsError = true,
                        Mensaje = "La API devolvió un catálogo de listas de precios inválido.",
                        ListaEntidad = []
                    };
                }

                var listas = apiResponse.Data
                    .Where(x => !string.IsNullOrWhiteSpace(x.lp_id))
                    .GroupBy(x => x.lp_id.Trim(), StringComparer.OrdinalIgnoreCase)
                    .Select(x => x.First())
                    .ToList();

                return new RespuestaGenerica<PrecioListaDto>
                {
                    Ok = true,
                    Mensaje = "OK",
                    ListaEntidad = listas
                };
            }
            catch (UnauthorizedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo el catálogo de listas de precios.");
                return new RespuestaGenerica<PrecioListaDto>
                {
                    Ok = false,
                    EsError = true,
                    Mensaje = "No se pudo obtener el catálogo de listas de precios.",
                    ListaEntidad = []
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

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoDatosResponseDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    var resp = apiResponse.Data;
                    if (!resp.Any())
                    {
                        return new() { Ok = false, Mensaje = "No se encontraron productos según el criterio." };
                    }
                    else
                    {
                        return new RespuestaGenerica<ProductoDatosResponseDto>
                        {
                            Ok = true,
                            Mensaje = "OK",
                            ListaEntidad = apiResponse.Data
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

        public async Task<ProductoBusquedaDto> BusquedaBaseProductos(BusquedaBase busqueda, string token)
        {
            ApiResponse<ProductoBusquedaDto> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;
            string parametros = EvaluarEntidad4Link(busqueda);
            var link = $"{_appSettings.RutaBase}{RutaAPIProducto}{BUSCAR_PROD}?{parametros}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {parametros}");
                    return new ProductoBusquedaDto();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<ProductoBusquedaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new ProductoBusquedaDto();

            }
        }

        public async Task<(List<ProductoListaDto>, MetadataGrid?)> BusquedaListaProductos(BusquedaProducto busqueda, string token)
        {
            ApiResponse<List<ProductoListaDto>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(busqueda, token, out StringContent content);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPIProducto}{BUSCAR_LISTA}";

            response = await client.PostAsync(link, content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoListaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");
                return (apiResponse.Data ?? [], apiResponse.Meta ?? new());
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return (new(), null);
            }
        }

        /// <summary>
        /// ✅ NUEVO: Invoca al SP SPGECO_CAJA_Ope_Calcula_Filas para calcular totales
        /// </summary>
        /// <param name="req">Request con datos del cliente, totales y JSON de productos</param>
        /// <param name="token">Token de autenticación</param>
        /// <returns>Response con 3 JSONs: subtotal, sorteo, productos impositivos</returns>
        public async Task<CalculaFilasResDto> CalcularFilas(CalcularFilasReqDto req, string token)
        {
            try
            {
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🔢 CALCULAR FILAS - SERVICIO");
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation($"   caja_id: {req.caja_id}");
                _logger?.LogInformation($"   usu_id: {req.usu_id}");
                _logger?.LogInformation($"   cta_id: {req.cta_id}");
                _logger?.LogInformation($"   lp_id: {req.lp_id}");
                _logger?.LogInformation($"   tot_rows: {req.tot_rows}");
                _logger?.LogInformation($"   tot_cantidad: {req.tot_cantidad}");
                _logger?.LogInformation($"   tot_pvta: {req.tot_pvta}");
                _logger?.LogInformation($"   Longitud JSON productos: {req.json_p?.Length ?? 0}");
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                // ❶ Validar request
                if (req == null)
                {
                    _logger?.LogError("❌ Request es null");
                    return new CalculaFilasResDto();
                }

                if (string.IsNullOrEmpty(req.json_p) || req.json_p == "[]")
                {
                    _logger?.LogWarning("⚠️ JSON de productos vacío");
                    return new CalculaFilasResDto();
                }

                // ❷ Inicializar helper y cliente HTTP
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_CALCULAR_FILAS}";

                _logger?.LogInformation($"📡 Endpoint: {link}");

                // ❸ Realizar POST
                using var response = await client.PostAsync(link, contentData);

                // ❹ Procesar respuesta
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        _logger?.LogWarning("⚠️ API retornó respuesta vacía");
                        return new CalculaFilasResDto();
                    }

                    _logger?.LogInformation("✅ Respuesta recibida correctamente");
                    _logger?.LogInformation($"   Longitud respuesta: {stringData.Length} caracteres");

                    // ❺ Deserializar respuesta
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CalculaFilasResDto>>(stringData);

                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        _logger?.LogError("❌ Error deserializando la respuesta");
                        return new CalculaFilasResDto();
                    }

                    var resultado = apiResponse.Data;

                    _logger?.LogInformation("═══════════════════════════════════════════════════");
                    _logger?.LogInformation("✅ DATOS CALCULADOS EXITOSAMENTE");
                    _logger?.LogInformation($"   json_subtotal: {(string.IsNullOrEmpty(resultado.json_subtotal) ? "vacío" : $"{resultado.json_subtotal.Length} caracteres")}");
                    _logger?.LogInformation($"   json_sorteo: {(string.IsNullOrEmpty(resultado.json_sorteo) ? "vacío" : $"{resultado.json_sorteo.Length} caracteres")}");
                    _logger?.LogInformation($"   json_p: {(string.IsNullOrEmpty(resultado.json_p) ? "vacío" : $"{resultado.json_p.Length} caracteres")}");
                    _logger?.LogInformation("═══════════════════════════════════════════════════");

                    return resultado;
                }
                else
                {
                    var msg = await ReadApiErrorAsync(response);
                    _logger?.LogWarning($"❌ Error API ({response.StatusCode}): {msg}");
                    return new CalculaFilasResDto();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"❌ EXCEPCIÓN en CalcularFilas: {ex.Message}");
                _logger?.LogError($"   Stack Trace: {ex.StackTrace}");
                return new CalculaFilasResDto();
            }
        }

        /// <summary>
        /// Obtiene los datos de prefactura para un cliente
        /// </summary>
        /// <param name="req">Request con datos de la prefactura</param>
        /// <param name="token">Token de autenticación</param>
        /// <returns>Response con datos de la prefactura</returns>
        public async Task<RespuestaGenerica<PrefacturaResDto>> ObtenerPrefactura(PrefacturaReqDto req, string token)
        {
            try
            {
               
                if (req == null)
                {
                    _logger?.LogError("❌ Request es null");
                    return new() { Ok = false, Mensaje = "Request inválido" };
                }

                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_OBTENER_PREFACTURA}";

                _logger?.LogInformation($"📡 Endpoint: {link}");

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        _logger?.LogWarning("⚠️ API retornó respuesta vacía");
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PrefacturaResDto>>>(stringData);

                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        _logger?.LogError("❌ Error deserializando la respuesta");
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    _logger?.LogInformation("✅ Prefactura obtenida exitosamente");

                    return new RespuestaGenerica<PrefacturaResDto>
                    {
                        Ok = true,
                        Mensaje = "OK",
                        ListaEntidad = apiResponse.Data
                    };
                }
                else
                {
                    var msg = await ReadApiErrorAsync(response);
                    _logger?.LogWarning($"❌ Error API ({response.StatusCode}): {msg}");
                    return new() { Ok = false, Mensaje = msg };
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"❌ EXCEPCIÓN en ObtenerPrefactura: {ex.Message}");
                _logger?.LogError($"   Stack Trace: {ex.StackTrace}");
                return new() { Ok = false, Mensaje = "Error al obtener la prefactura" };
            }
        }

        /// <summary>
        /// Obtiene los datos de cotización para un cliente
        /// </summary>
        /// <param name="req">Request con datos de la cotización</param>
        /// <param name="token">Token de autenticación</param>
        /// <returns>Response con datos de la cotización</returns>
        public async Task<RespuestaGenerica<CotizacionResDto>> ObtenerCotizacion(CotizacionReqDto req, string token)
        {
            try
            {
               
                if (req == null)
                {
                    _logger?.LogError("❌ Request es null");
                    return new() { Ok = false, Mensaje = "Request inválido" };
                }

                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_OBTENER_COTIZACION}";

                _logger?.LogInformation($"📡 Endpoint: {link}");

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        _logger?.LogWarning("⚠️ API retornó respuesta vacía");
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<CotizacionResDto>>>(stringData);

                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        _logger?.LogError("❌ Error deserializando la respuesta");
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    _logger?.LogInformation("✅ Cotización obtenida exitosamente");

                    return new RespuestaGenerica<CotizacionResDto>
                    {
                        Ok = true,
                        Mensaje = "OK",
                        ListaEntidad = apiResponse.Data
                    };
                }
                else
                {
                    var msg = await ReadApiErrorAsync(response);
                    _logger?.LogWarning($"❌ Error API ({response.StatusCode}): {msg}");
                    return new() { Ok = false, Mensaje = msg };
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"❌ EXCEPCIÓN en ObtenerCotizacion: {ex.Message}");
                _logger?.LogError($"   Stack Trace: {ex.StackTrace}");
                return new() { Ok = false, Mensaje = "Error al obtener la cotización" };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> CrearPrefacturaDiferida(CajaPrefDiferidaReqDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_CREAR_PREF_DIFERIDA}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name} - 01 - Error deserializando la respuesta de la API");
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name} - 02 - Error deserializando la respuesta de la API");
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

        public async Task<RespuestaGenerica<RespuestaDto>> CrearDiferirPago(CajaOpeConfirmarReq req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_CREAR_PAGO_DIFERIDO}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name} - 01 - Error deserializando la respuesta de la API");
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        _logger.LogWarning($"{MethodBase.GetCurrentMethod().Name} - 02 - Error deserializando la respuesta de la API");
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
    }
}
