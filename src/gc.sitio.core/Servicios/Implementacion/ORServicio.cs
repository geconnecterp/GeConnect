using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenReparto;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class ORServicio : Servicio<Dto>, IORServicio
    {
        private const string RutaAPI = "/api/ApiOR";
        private const string RutaAPI02 = "/api/administracion";

        private const string POST_OBTENER_OR = "/ObtenerOrdenesReparto";
        private const string GET_VALIDA_USU = "/TIValidarUsuario";
        private const string GET_LISTA_OR_BOX = "/ObtenerListaORbyBox";
        private const string GET_LISTA_OR_RUBRO = "/ObtenerListaORbyRubro";
        private const string POST_LISTA_OR_PRODUCTOS = "/ObtenerListaORProductos";
        private const string POST_VALIDA_PRODUCTO_CARRITO_OR = "/ValidaProductoCarritoOR";
        private const string POST_RESGUARDAR_PRODUCTO_CARRITO_OR = "/ResguardarProductoCarritoOR";
        private const string GET_LISTA_ORCTL_PRODUCTOS = "/ObtenerListaProductosOrCtl";
        private const string POST_CARGA_PRODUCTO_OR_CTL = "/CargaProductoORCtl";

        public ORServicio(IOptions<AppSettings> options,ILogger<ORServicio> logger):base(options,logger,RutaAPI)
        {
            
        }

        public async Task<RespuestaGenerica<ORListaDto>> ObtenerListaORbyBox(string or_compte, string adm, string usu, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_LISTA_OR_BOX}?or_compte={or_compte}&adm={adm}&usu={usu}";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de los OR x BOX" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ORListaDto>>>(stringData)
                        ?? throw new NegocioException("Error al deserializar los datos");

                    if (apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "No se encontraron datos de los OR x BOX." };
                    }

                    return new RespuestaGenerica<ORListaDto>
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
                        Mensaje = "Error al obtener los OR x BOX. Si el problema persiste contacte al administrador."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<ORListaDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al obtener los OR x BOX"
                };
            }
        }

        public async Task<RespuestaGenerica<ORListaDto>> ObtenerListaORbyRubro(string or_compte, string adm, string usu, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_LISTA_OR_RUBRO}?or_compte={or_compte}&adm={adm}&usu={usu}";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de los OR x RUBRO" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ORListaDto>>>(stringData)
                        ?? throw new NegocioException("Error al deserializar los datos");

                    if (apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "No se encontraron datos de los OR x RUBRO." };
                    }

                    return new RespuestaGenerica<ORListaDto>
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
                        Mensaje = "Error al obtener los OR x RUBRO. Si el problema persiste contacte al administrador."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<ORListaDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al obtener los OR x RUBRO"
                };
            }
        }

        public async  Task<RespuestaGenerica<OrdenRepartoListDto>> ObtenerOrdenesReparto(ORRequestDto request, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(request, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_OBTENER_OR}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OrdenRepartoListDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    return new RespuestaGenerica<OrdenRepartoListDto>
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
                return new() { Ok = false, Mensaje = "Error al buscar las Etiquetas" };
            }
        }

        public async Task<RespuestaGenerica<ResponseBaseDto>> ValidarUsuario( string id, string usuId, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI02}{GET_VALIDA_USU}?tipo=OR&id={id}&usu={usuId}";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la validación del Usuario" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ResponseBaseDto>>(stringData)
                        ?? throw new NegocioException("Error al deserializar los datos");

                    if (apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "No se encontraron datos validación del Usuario." };
                    }

                    return new RespuestaGenerica<ResponseBaseDto>
                    {
                        Ok = true,
                        Entidad = apiResponse.Data,
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
                        Mensaje = "Error al obtener los estados de combos. Si el problema persiste contacte al administrador."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<ResponseBaseDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al validar el usuario"
                };
            }
        }

        public async Task<RespuestaGenerica<ORProductoDto>> ObtenerORProductos(ORProdRequestDto request, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(request, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_LISTA_OR_PRODUCTOS}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ORProductoDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    return new RespuestaGenerica<ORProductoDto>
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
                return new() { Ok = false, Mensaje = "Error al obtener los productos de la OR" };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> ValidaProductoCarritoOR(ORCargaCarritoRequest request, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(request, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_VALIDA_PRODUCTO_CARRITO_OR}";

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

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = true,
                        Mensaje = "OK",
                        Entidad = apiResponse.Data
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
                return new() { Ok = false, Mensaje = "Error al Validar producto en el carrito" };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> ResguardarProductoCarrito(ORCargaCarritoRequest request, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(request, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_RESGUARDAR_PRODUCTO_CARRITO_OR}";

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

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = true,
                        Mensaje = "OK",
                        Entidad = apiResponse.Data
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
                return new() { Ok = false, Mensaje = "Error al buscar las Etiquetas" };
            }
        }

        public async Task<RespuestaGenerica<OrCtlProductoDto>> ObtenerListaProductosOrCtl(string or_compte, string usu_id, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_LISTA_ORCTL_PRODUCTOS}?or_compte={or_compte}&usu_id={usu_id}";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de los OR x BOX" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<OrCtlProductoDto>>>(stringData)
                        ?? throw new NegocioException("Error al deserializar los datos");

                    if (apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "No se encontraron datos de los OR x BOX." };
                    }

                    return new RespuestaGenerica<OrCtlProductoDto>
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
                        Mensaje = "Error al obtener los OR x BOX. Si el problema persiste contacte al administrador."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<OrCtlProductoDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al obtener los OR x BOX"
                };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> CargaProductoORCtl(string json, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(json, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_CARGA_PRODUCTO_OR_CTL}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API, al cargar el Producto Controlado" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API, al cargar el Producto Controlado" };
                    }

                    if (apiResponse.Data.resultado == 0)
                    {

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = true,
                        Mensaje = "OK",
                        Entidad = apiResponse.Data
                        // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
                    };

                    } else if(apiResponse.Data.resultado>0)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = apiResponse.Data.resultado_msj,
                            Entidad = apiResponse.Data,
                            EsWarn = true,
                            EsError =false,

                            // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
                        };
                    }
                    else
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = apiResponse.Data.resultado_msj,
                            Entidad = apiResponse.Data,
                            EsWarn = false,
                            EsError = true,

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
                return new() { Ok = false, Mensaje = "Error al Validar producto en el carrito" };
            }
        }
    }
}
