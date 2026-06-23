using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gc.infraestructura.Core.Helpers
{
    public sealed class HelperAPIv02
    {
        private readonly bool _permitirCertificadoNoConfiable;

        private static readonly HashSet<string> HeadersRestringidos =
            new(StringComparer.OrdinalIgnoreCase)
            {
            "Host",
            "Content-Length",
            "Transfer-Encoding",
            "Connection",
            "Keep-Alive",
            "Proxy-Connection",
            "Upgrade",
            "TE",
            "Trailer"
            };

        /// <summary>
        /// Constructor del helper.
        /// permitirCertificadoNoConfiable debe utilizarse solamente en desarrollo.
        /// </summary>
        public HelperAPIv02(bool permitirCertificadoNoConfiable = false)
        {
            _permitirCertificadoNoConfiable = permitirCertificadoNoConfiable;
        }

        /// <summary>
        /// Inicializa un cliente sin headers adicionales.
        /// </summary>
        public HttpClient InicializaCliente()
        {
            return InicializaCliente(
                (IReadOnlyDictionary<string, JToken>?)null);
        }

        /// <summary>
        /// Inicializa un cliente HTTP y aplica headers dinámicos.
        /// </summary>
        public HttpClient InicializaCliente(
            IReadOnlyDictionary<string, JToken>? headers)
        {
            var clientHandler = CrearHttpClientHandler();

            var client = new HttpClient(clientHandler);

            // Header por defecto.
            // Si el request trae Accept, será reemplazado.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            AgregarHeadersSolicitud(client, headers);

            return client;
        }

        /// <summary>
        /// Inicializa cliente y prepara contenido JSON para una solicitud POST.
        /// Body recibido como string JSON.
        /// </summary>
        public HttpClient InicializaClienteGen(
            string? json,
            IReadOnlyDictionary<string, JToken>? headers,
            out StringContent contentData)
        {
            var client = InicializaCliente(headers);

            contentData = new StringContent(
                json ?? string.Empty,
                Encoding.UTF8,
                "application/json");

            AplicarContentType(headers, contentData);

            return client;
        }

        /// <summary>
        /// Sobrecarga para recibir el body como JToken.
        /// Permite enviar "body": { ... } sin necesidad de escapar JSON.
        /// </summary>
        public HttpClient InicializaClienteGen(
            JToken? body,
            IReadOnlyDictionary<string, JToken>? headers,
            out StringContent contentData)
        {
            string json;

            if (body == null || body.Type == JTokenType.Null)
            {
                json = string.Empty;
            }
            else if (body.Type == JTokenType.String)
            {
                // Permite seguir enviando un JSON como texto.
                json = body.Value<string>() ?? string.Empty;
            }
            else
            {
                // Convierte objetos, arrays, números, etc. a JSON válido.
                json = body.ToString(Formatting.None);
            }

            return InicializaClienteGen(json, headers, out contentData);
        }

        /// <summary>
        /// Inicializa cliente y serializa una entidad para POST.
        /// </summary>
        public HttpClient InicializaCliente<T>(
            T entidad,
            IReadOnlyDictionary<string, JToken>? headers,
            out StringContent contentData)
        {
            var json = JsonConvert.SerializeObject(entidad);

            return InicializaClienteGen(
                json,
                headers,
                out contentData);
        }

        /// <summary>
        /// Crea el handler HTTP.
        /// </summary>
        private HttpClientHandler CrearHttpClientHandler()
        {
            var handler = new HttpClientHandler();

            if (_permitirCertificadoNoConfiable)
            {
                handler.ServerCertificateCustomValidationCallback =
                    (sender, cert, chain, sslPolicyErrors) => true;
            }

            return handler;
        }

        /// <summary>
        /// Aplica headers que pertenecen a la solicitud HTTP.
        /// </summary>
        private static void AgregarHeadersSolicitud(
            HttpClient client,
            IReadOnlyDictionary<string, JToken>? headers)
        {
            if (headers == null || headers.Count == 0)
            {
                return;
            }

            foreach (var item in headers)
            {
                var nombre = item.Key?.Trim();

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    continue;
                }

                if (HeadersRestringidos.Contains(nombre))
                {
                    throw new ArgumentException(
                        $"El header '{nombre}' no está permitido.");
                }

                var valor = ConvertirValorHeader(item.Value);

                if (string.IsNullOrWhiteSpace(valor))
                {
                    continue;
                }

                // Authorization requiere manejo tipado.
                if (nombre.Equals(
                        "Authorization",
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (!AuthenticationHeaderValue.TryParse(
                            valor,
                            out var authorizationHeader))
                    {
                        throw new ArgumentException(
                            "El header Authorization tiene un formato inválido.");
                    }

                    client.DefaultRequestHeaders.Authorization =
                        authorizationHeader;

                    continue;
                }

                // Accept requiere manejo tipado.
                if (nombre.Equals(
                        "Accept",
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (!MediaTypeWithQualityHeaderValue.TryParse(
                            valor,
                            out var acceptHeader))
                    {
                        throw new ArgumentException(
                            "El header Accept tiene un formato inválido.");
                    }

                    client.DefaultRequestHeaders.Accept.Clear();

                    client.DefaultRequestHeaders.Accept.Add(
                        acceptHeader);

                    continue;
                }

                // Content-Type pertenece al contenido del POST.
                // Se aplicará luego en AplicarContentType.
                if (nombre.Equals(
                        "Content-Type",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var agregado = client.DefaultRequestHeaders
                    .TryAddWithoutValidation(nombre, valor);

                if (!agregado)
                {
                    throw new ArgumentException(
                        $"No fue posible agregar el header '{nombre}'.");
                }
            }
        }

        /// <summary>
        /// Aplica Content-Type al contenido HTTP si fue enviado en Header.
        /// </summary>
        private static void AplicarContentType(
            IReadOnlyDictionary<string, JToken>? headers,
            HttpContent contentData)
        {
            var contentType = ObtenerHeader(headers, "Content-Type");

            if (string.IsNullOrWhiteSpace(contentType))
            {
                return;
            }

            if (!MediaTypeHeaderValue.TryParse(
                    contentType,
                    out var mediaType))
            {
                throw new ArgumentException(
                    "El header Content-Type tiene un formato inválido.");
            }

            contentData.Headers.ContentType = mediaType;
        }

        /// <summary>
        /// Busca un header ignorando mayúsculas y minúsculas.
        /// </summary>
        private static string? ObtenerHeader(
            IReadOnlyDictionary<string, JToken>? headers,
            string nombreBuscado)
        {
            if (headers == null || headers.Count == 0)
            {
                return null;
            }

            foreach (var item in headers)
            {
                if (item.Key.Equals(
                        nombreBuscado,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return ConvertirValorHeader(item.Value);
                }
            }

            return null;
        }

        /// <summary>
        /// Convierte valores JSON simples a texto para usarlos como header HTTP.
        /// </summary>
        private static string? ConvertirValorHeader(JToken? valor)
        {
            if (valor == null)
            {
                return null;
            }

            return valor.Type switch
            {
                JTokenType.String => valor.Value<string>(),

                JTokenType.Integer => valor.ToString(Formatting.None),

                JTokenType.Float => valor.ToString(Formatting.None),

                JTokenType.Boolean => valor.Value<bool>()
                    ? "true"
                    : "false",

                JTokenType.Date => valor.Value<DateTime>()
                    .ToUniversalTime()
                    .ToString("O", CultureInfo.InvariantCulture),

                JTokenType.Null => null,

                JTokenType.Undefined => null,

                _ => throw new ArgumentException(
                    $"El valor del header debe ser string, número, booleano o fecha. " +
                    $"Tipo recibido: {valor.Type}.")
            };
        }

        /// <summary>
        /// Valida firma, issuer, audience y vigencia del token.
        /// Requiere parámetros de validación configurados por la aplicación.
        /// </summary>
        public bool EsTokenValido(
            string token,
            TokenValidationParameters parametrosValidacion,
            out string usuario,
            out string role,
            out string email)
        {
            usuario = string.Empty;
            role = string.Empty;
            email = string.Empty;

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            if (parametrosValidacion == null)
            {
                throw new ArgumentNullException(
                    nameof(parametrosValidacion));
            }

            try
            {
                var tokenLimpio = QuitarPrefijoBearer(token);

                var handler = new JwtSecurityTokenHandler();

                var principal = handler.ValidateToken(
                    tokenLimpio,
                    parametrosValidacion,
                    out _);

                usuario = ObtenerClaim(
                    principal.Claims,
                    "user",
                    "User",
                    ClaimTypes.NameIdentifier,
                    JwtRegisteredClaimNames.Sub);

                role = ObtenerClaim(
                    principal.Claims,
                    "role",
                    "roles",
                    ClaimTypes.Role);

                email = ObtenerClaim(
                    principal.Claims,
                    "email",
                    ClaimTypes.Email,
                    JwtRegisteredClaimNames.Email);

                return true;
            }
            catch (SecurityTokenException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// Compatibilidad temporal con tu implementación anterior.
        /// Solo verifica formato y fecha de vigencia.
        /// No valida la firma del JWT.
        /// </summary>
        [Obsolete(
            "Este método no valida la firma del token. " +
            "Use EsTokenValido con TokenValidationParameters.")]
        public bool EsTokenValido(
            string token,
            out string usuario,
            out string role,
            out string email)
        {
            usuario = string.Empty;
            role = string.Empty;
            email = string.Empty;

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            try
            {
                var tokenLimpio = QuitarPrefijoBearer(token);

                var handler = new JwtSecurityTokenHandler();

                var jwt = handler.ReadJwtToken(tokenLimpio);

                var ahora = DateTime.UtcNow;

                if (ahora < jwt.ValidFrom || ahora >= jwt.ValidTo)
                {
                    return false;
                }

                usuario = ObtenerClaim(
                    jwt.Claims,
                    "user",
                    "User",
                    ClaimTypes.NameIdentifier,
                    JwtRegisteredClaimNames.Sub);

                role = ObtenerClaim(
                    jwt.Claims,
                    "role",
                    "roles",
                    ClaimTypes.Role);

                email = ObtenerClaim(
                    jwt.Claims,
                    "email",
                    ClaimTypes.Email,
                    JwtRegisteredClaimNames.Email);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string QuitarPrefijoBearer(string token)
        {
            const string prefijoBearer = "Bearer ";

            var tokenLimpio = token.Trim();

            if (tokenLimpio.StartsWith(
                    prefijoBearer,
                    StringComparison.OrdinalIgnoreCase))
            {
                return tokenLimpio[prefijoBearer.Length..].Trim();
            }

            return tokenLimpio;
        }

        private static string ObtenerClaim(
            IEnumerable<Claim> claims,
            params string[] tiposBuscados)
        {
            foreach (var tipo in tiposBuscados)
            {
                var claim = claims.FirstOrDefault(c =>
                    string.Equals(
                        c.Type,
                        tipo,
                        StringComparison.OrdinalIgnoreCase));

                if (claim != null &&
                    !string.IsNullOrWhiteSpace(claim.Value))
                {
                    return claim.Value;
                }
            }

            return string.Empty;
        }
    }
}