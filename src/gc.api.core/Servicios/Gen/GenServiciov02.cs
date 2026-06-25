using gc.api.core.Contratos.Servicios.Gen;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Web;

namespace gc.api.core.Servicios.Gen
{
    public class GenServiciov02 : Servicio<EntidadBase>, IGenServicio
    {
        private readonly ILogger<GenServiciov02> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public GenServiciov02(IUnitOfWork uow, ILogger<GenServiciov02> logger, IHttpClientFactory httpClientFactory) : base(uow)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Invoca una API externa utilizando el método POST y devuelve la respuesta como un string.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RespuestaGenericaBase<string>> InvokeApiGET(ApiInvokeRequest request)
        {
            _logger.LogInformation("Iniciando invocación de API GET a la URL: {Url}", request.Url);
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(request.TimeoutInSeconds));

                var client = _httpClientFactory.CreateClient("ApiGen");
                HelperAPIv03.ConfigurarCliente(client, request.Header);

                var link = ConstruirUrlConQueryString(
                            request.Url,
                            request.Body);

                _logger.LogInformation("Enviando solicitud GET a {Url}", link);
                using var response = await client.GetAsync(link, cts.Token);
                var stringData = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Respuesta exitosa ({StatusCode}) de la API: {Url}", response.StatusCode, request.Url);
                    
                    if (string.IsNullOrEmpty(stringData))
                    {
                        _logger.LogWarning("La API en {Url} devolvió una respuesta vacía.", request.Url);
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }
                    return new RespuestaGenericaBase<string>
                    {
                        Ok = true,
                        EsError = false,
                        EsWarn = false,
                        Mensaje = "Solicitud exitosa",
                        Entidad = stringData
                    };
                }
                else
                {
                    _logger.LogWarning("Error en la solicitud a {Url}. Código de estado: {StatusCode}, Razón: {ReasonPhrase}", request.Url, response.StatusCode, response.ReasonPhrase);
                    return new RespuestaGenericaBase<string>
                    {
                        Ok = false,
                        EsError = false,
                        EsWarn = true,
                        Mensaje = $"Error en la solicitud: {response.ReasonPhrase}",
                        Entidad = string.IsNullOrWhiteSpace(stringData) ? null : stringData
                    };
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout al invocar la API (GET) en {Url}", request.Url);
                return new RespuestaGenericaBase<string>
                {
                    Ok = false,
                    EsError = true,
                    Mensaje = "Timeout al invocar la API externa.",
                    Entidad = JsonConvert.SerializeObject(new { error = ex.Message })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió una excepción al invocar la API en {Url}", request.Url);
                return new RespuestaGenericaBase<string>
                {
                    Ok = false,
                    EsError = true,
                    EsWarn = false,
                    Mensaje = "Ocurrió un error interno al procesar la solicitud.",
                    Entidad = JsonConvert.SerializeObject(new { error = ex.Message })
                };
            }
        }

        /// <summary>
        /// Invoca una API externa utilizando el método POST y devuelve la respuesta como un string.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RespuestaGenericaBase<string>> InvokeApiPOST(ApiInvokeRequest request)
        {
            _logger.LogInformation("Iniciando invocación de API POST a la URL: {Url}", request.Url);
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(request.TimeoutInSeconds));

                var client = _httpClientFactory.CreateClient("ApiGen");
                HelperAPIv03.ConfigurarCliente(client, request.Header);

                using var contentData = HelperAPIv03.PrepararContenido(request.Body, request.Header);

                _logger.LogInformation("Enviando solicitud POST a {Url}", request.Url);

                using var response = await client.PostAsync(request.Url, contentData, cts.Token);
                var stringData = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Respuesta exitosa ({StatusCode}) de la API: {Url}", response.StatusCode, request.Url);
                    if (string.IsNullOrEmpty(stringData))
                    {
                        _logger.LogWarning("La API en {Url} devolvió una respuesta vacía.", request.Url);
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }
                    return new RespuestaGenericaBase<string>
                    {
                        Ok = true,
                        EsError = false,
                        EsWarn = false,
                        Mensaje = "Solicitud exitosa",
                        Entidad = stringData
                    };
                }
                else
                {
                    _logger.LogWarning("Error en la solicitud a {Url}. Código de estado: {StatusCode}, Razón: {ReasonPhrase}", request.Url, response.StatusCode, response.ReasonPhrase);
                    return new RespuestaGenericaBase<string>
                    {
                        Ok = false,
                        EsError = false,
                        EsWarn = true,
                        Mensaje = $"Error en la solicitud: {response.ReasonPhrase}",
                        Entidad = string.IsNullOrWhiteSpace(stringData) ? null : stringData
                    };
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Timeout al invocar la API (POST) en {Url}", request.Url);
                return new RespuestaGenericaBase<string>
                {
                    Ok = false,
                    EsError = true,
                    Mensaje = "Timeout al invocar la API externa.",
                    Entidad = JsonConvert.SerializeObject(new { error = ex.Message })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió una excepción al invocar la API en {Url}", request.Url);
                return new RespuestaGenericaBase<string>
                {
                    Ok = false,
                    EsError = true,
                    EsWarn = false,
                    Mensaje = "Ocurrió un error interno al procesar la solicitud.",
                    Entidad = JsonConvert.SerializeObject(new { error = ex.Message })
                };
            }
        }

        private string ConstruirUrlConQueryString(string baseUrl, JToken? body)
        {
            if (body == null ||
                body.Type == JTokenType.Null ||
                body.Type == JTokenType.Undefined)
            {
                return baseUrl;
            }

            try
            {
                JObject? parametros = ObtenerObjetoParametros(body);

                if (parametros == null || !parametros.Properties().Any())
                {
                    _logger.LogWarning(
                        "El body recibido para construir el query string no es un objeto JSON válido. URL: {Url}",
                        baseUrl);

                    return baseUrl;
                }

                var uriBuilder = new UriBuilder(baseUrl);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                foreach (var propiedad in parametros.Properties())
                {
                    var valor = ConvertirValorQueryString(propiedad.Value);

                    // Decisión: si llega null, no se agrega el parámetro.
                    if (valor == null)
                    {
                        query.Remove(propiedad.Name);
                        continue;
                    }

                    query[propiedad.Name] = valor;
                }

                uriBuilder.Query = query.ToString();

                return uriBuilder.Uri.ToString();
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Error al procesar el body JSON para construir el query string. URL: {Url}",
                    baseUrl);

                return baseUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al construir el query string. URL: {Url}",
                    baseUrl);

                return baseUrl;
            }
        }

        /// <summary>
        /// Obtiene un JObject desde el body.
        /// Soporta tanto body como objeto JSON como body enviado antiguamente como string JSON.
        /// </summary>
        private static JObject? ObtenerObjetoParametros(JToken body)
        {
            if (body.Type == JTokenType.Object)
            {
                return body as JObject;
            }

            // Compatibilidad: permite recibir:
            // "body": "{\"origen\":\"C\",\"valor\":\"C0198746\"}"
            if (body.Type == JTokenType.String)
            {
                var jsonInterno = body.Value<string>();

                if (string.IsNullOrWhiteSpace(jsonInterno))
                {
                    return null;
                }

                var tokenParseado = JToken.Parse(jsonInterno);

                return tokenParseado as JObject;
            }

            return null;
        }

        /// <summary>
        /// Convierte valores JSON a valores aptos para query string.
        /// Objetos y arrays se mantienen como JSON compacto.
        /// </summary>
        private static string? ConvertirValorQueryString(JToken valor)
        {
            return valor.Type switch
            {
                JTokenType.Null => null,

                JTokenType.Undefined => null,

                JTokenType.String => valor.Value<string>(),

                JTokenType.Date => valor.Value<DateTime>()
                    .ToUniversalTime()
                    .ToString("O", CultureInfo.InvariantCulture),

                JTokenType.Boolean => valor.Value<bool>()
                    ? "true"
                    : "false",

                JTokenType.Integer => valor.ToString(Formatting.None),

                JTokenType.Float => valor.ToString(Formatting.None),

                // Para arrays u objetos anidados se envía JSON compacto.
                JTokenType.Array => valor.ToString(Formatting.None),

                JTokenType.Object => valor.ToString(Formatting.None),

                _ => valor.ToString(Formatting.None)
            };
        }
    }
}