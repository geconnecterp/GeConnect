using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.Dtos.Productos.PromoCombo;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using System.Text;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class ComboServicio : Servicio<Dto>, IComboServicio
    {
        // Constantes para rutas API
        private const string RutaAPI = "/api/ApiPromoCombo";
        private const string COMBO_TIPOS = "/combos-tipos";
        private const string COMBO_ESTADOS = "/combos-estados";
        private const string COMBO_BUSCAR = "/combos-buscar";
        private const string COMBO_CANALES = "/combo/{id}/canales";
        private const string COMBO_DATOS = "/combo/{id}/";
        private const string COMBO_PRODUCTOS = "/combo/{id}/productos";
        private const string COMBO_SUSTITUTOS = "/combo/{id}/producto/{productoId}/sustitutos";
        
        public ComboServicio(IOptions<AppSettings> options, ILogger<ComboServicio> logger)
            : base(options, logger)
        {
        }

        /// <summary>
        /// Obtiene los tipos disponibles para combos y promociones
        /// </summary>
        public async Task<RespuestaGenerica<ComboTipoDto>> ObtenerComboTipos(string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{COMBO_TIPOS}";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ComboTipoDto>>>(stringData)
                        ?? throw new NegocioException("Error al deserializar los datos");

                    return new RespuestaGenerica<ComboTipoDto>
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
                        Mensaje = "Error al obtener los tipos de combos. Si el problema persiste contacte al administrador."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<ComboTipoDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al obtener los tipos de combos"
                };
            }
        }

        /// <summary>
        /// Obtiene los estados disponibles para combos y promociones
        /// </summary>
        public async Task<RespuestaGenerica<ComboEstadoDto>> ObtenerComboEstados(string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{COMBO_ESTADOS}";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ComboEstadoDto>>>(stringData)
                        ?? throw new NegocioException("Error al deserializar los datos");

                    return new RespuestaGenerica<ComboEstadoDto>
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
                        Mensaje = "Error al obtener los estados de combos. Si el problema persiste contacte al administrador."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<ComboEstadoDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al obtener los estados de combos"
                };
            }
        }

        /// <summary>
        /// Busca combos y promociones según los filtros especificados
        /// </summary>
        public async Task<RespuestaGenerica<ComboListaDto>> BuscarCombos(QueryFilters filtros, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var jsonContent = JsonConvert.SerializeObject(filtros);
                var contentData = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var client = helper.InicializaCliente(token);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{COMBO_BUSCAR}";

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ComboListaDto>>>(stringData)
                        ?? throw new NegocioException("Error al deserializar los datos");

                    // Obtener información de paginación si existe

                    return new RespuestaGenerica<ComboListaDto>
                    {
                        Ok = true,
                        ListaEntidad = apiResponse.Data,
                        Meta = apiResponse.Meta ?? new(),
                        Mensaje = "OK"
                    };
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API (400): {errorData}");

                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(errorData);

                    return new()
                    {
                        Ok = false,
                        Mensaje = error?.Detail ?? "Los filtros proporcionados no son válidos"
                    };
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    return new()
                    {
                        Ok = false,
                        Mensaje = "Error al buscar combos. Si el problema persiste contacte al administrador."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<ComboListaDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al buscar combos"
                };
            }
        }

        public async Task<RespuestaGenerica<ComboCanalDto>> ObtenerCanalesDeCombo(string id, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI}/combo/{id}/canales";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ComboCanalDto>>>(stringData)
                        ?? throw new NegocioException("Error al deserializar los datos");

                    return new RespuestaGenerica<ComboCanalDto>
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
                        Mensaje = "Error al obtener los canales de combos. Si el problema persiste contacte al administrador."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<ComboCanalDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al obtener los canales de combos"
                };
            }
        }

        public async Task<RespuestaGenerica<ComboDatosDto>> ObtenerComboPorId(string id, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI}/combo/{id}";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ComboDatosDto>>(stringData)
                        ?? throw new NegocioException("Error al deserializar los datos");

                    return new RespuestaGenerica<ComboDatosDto>
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
                        Mensaje = "Error al obtener los DATOS del \"combo\". Si el problema persiste contacte al administrador."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<ComboDatosDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al obtener los DATOS del \"combo\""
                };
            }
        }

        /// <summary>
        /// Obtiene los productos asociados a un combo específico
        /// </summary>
        public async Task<RespuestaGenerica<ComboProductoDto>> ObtenerProductosDeCombo(string id, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI}/combo/{id}/productos";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ComboProductoDto>>>(stringData)
                        ?? throw new NegocioException("Error al deserializar los datos");

                    return new RespuestaGenerica<ComboProductoDto>
                    {
                        Ok = true,
                        ListaEntidad = apiResponse.Data,
                        Mensaje = "OK"
                    };
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new RespuestaGenerica<ComboProductoDto>
                    {
                        Ok = true,
                        ListaEntidad = new List<ComboProductoDto>(),
                        Mensaje = "No se encontraron productos asociados al combo"
                    };
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    return new()
                    {
                        Ok = false,
                        Mensaje = "Error al obtener los productos del combo. Si el problema persiste contacte al administrador."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<ComboProductoDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al obtener los productos del combo"
                };
            }
        }

        /// <summary>
        /// Obtiene los productos sustitutos asociados a un producto dentro de un combo específico
        /// </summary>
        public async Task<RespuestaGenerica<ComboSustitutoDto>> ObtenerProductosSustitutosDeCombo(string comboId, string productoId, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                var link = $"{_appSettings.RutaBase}{RutaAPI}/combo/{comboId}/producto/{productoId}/sustitutos";
                using var response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ComboSustitutoDto>>>(stringData)
                        ?? throw new NegocioException("Error al deserializar los datos");

                    return new RespuestaGenerica<ComboSustitutoDto>
                    {
                        Ok = true,
                        ListaEntidad = apiResponse.Data,
                        Mensaje = "OK"
                    };
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // En este caso, es válido que un producto no tenga sustitutos
                    return new RespuestaGenerica<ComboSustitutoDto>
                    {
                        Ok = true,
                        ListaEntidad = new List<ComboSustitutoDto>(),
                        Mensaje = "No se encontraron sustitutos para el producto"
                    };
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    return new()
                    {
                        Ok = false,
                        Mensaje = "Error al obtener los sustitutos del producto. Si el problema persiste contacte al administrador."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return new RespuestaGenerica<ComboSustitutoDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al obtener los sustitutos del producto"
                };
            }
        }
    }
}
