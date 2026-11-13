using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;

namespace gc.caja.core.Servicios.Implementacion.Cajas
{
    /// <summary>Recupera el PDF de un comprobante; no confirma ni reintenta operaciones de Caja.</summary>
    public class ReportesService : IReportesService
    {
        private const string RutaGenerar = "api/reportes/generate";
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReportesService> _logger;

        public ReportesService(IConfiguration configuration, ILogger<ReportesService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RespuestaReportDto> ObtenerPdfDesdeAPI(ReporteSolicitudDto reporteSolicitud, string token)
        {
            var llamada = Guid.NewGuid().ToString("N");
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return Error("Token de autenticación no proporcionado");

                var url = CrearUrlGeneracion(_configuration["DocsManager:ApiReporteUrl"]);
                var helper = new HelperAPI();
                using var client = helper.InicializaCliente(reporteSolicitud, token, out StringContent content);
                using var cuerpo = content;

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("[REPORTES][REQUEST] Generar | Llamada={Llamada} | POST {Url}\n{Json}",
                        llamada, url, CajaJsonDiagnostico.Formatear(await cuerpo.ReadAsStringAsync(), token));

                var reloj = Stopwatch.StartNew();
                using var response = await client.PostAsync(url, cuerpo);
                var texto = await response.Content.ReadAsStringAsync();
                reloj.Stop();
                var nivel = response.StatusCode == HttpStatusCode.OK ? LogLevel.Information : LogLevel.Warning;
                _logger.Log(nivel,
                    "[REPORTES][RESPONSE] Generar | Llamada={Llamada} | POST {Url} | Destino={Destino} | HTTP {Estado} | Tipo={Tipo} | {DuracionMs} ms | Caracteres={Caracteres}",
                    llamada, url, response.RequestMessage?.RequestUri?.GetLeftPart(UriPartial.Path),
                    (int)response.StatusCode, response.Content.Headers.ContentType?.ToString(), reloj.ElapsedMilliseconds, texto.Length);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // Los errores de IIS/ruta pueden ser HTML: conservar su detalle acotado, sin tokens.
                    RegistrarErrorRespuesta(llamada, texto, token);
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogError("[REPORTES][404] Llamada={Llamada} | POST {Url}. Revisar publicación de gc.api.report y DocsManager:ApiReporteUrl (URL base con subaplicación, sin api/reportes/generate).",
                            llamada, url);
                        return Error("El servicio de Reportes no está disponible en la dirección configurada (HTTP 404). Reintente sólo la impresión cuando se restablezca el servicio.");
                    }
                    return Error($"Error en API de Reportes: HTTP {(int)response.StatusCode} - {response.ReasonPhrase}");
                }

                if (string.IsNullOrWhiteSpace(texto))
                    return Error("La API de Reportes no devolvió datos");

                ApiResponse<RespuestaReportDto>? respuesta;
                try
                {
                    respuesta = JsonConvert.DeserializeObject<ApiResponse<RespuestaReportDto>>(texto);
                }
                catch (JsonException)
                {
                    _logger.LogError("[REPORTES][CONTRATO] Llamada={Llamada} | La respuesta no es JSON válido. Revisar URL, redirecciones y publicación.", llamada);
                    return Error("Error al procesar respuesta de la API de Reportes (JSON inválido)");
                }

                // Validar antes de acceder a Data; una respuesta HTML o sin datos nunca es un PDF exitoso.
                if (respuesta?.Data == null)
                {
                    _logger.LogError("[REPORTES][CONTRATO] Llamada={Llamada} | La respuesta no contiene Data.", llamada);
                    return Error("Error al procesar respuesta de la API de Reportes (sin datos)");
                }

                var datos = respuesta.Data;
                _logger.LogInformation("[REPORTES][RESULTADO] Llamada={Llamada}\n{Json}", llamada,
                    CajaJsonDiagnostico.Formatear(JsonConvert.SerializeObject(new
                    {
                        datos.resultado, datos.resultado_id, datos.resultado_msj,
                        pdf_recibido = !string.IsNullOrWhiteSpace(datos.Base64),
                        pdf_caracteres = datos.Base64?.Length ?? 0
                    }), token));

                if (datos.resultado == 0 && string.IsNullOrWhiteSpace(datos.Base64))
                    return Error("La API de Reportes no devolvió el PDF del comprobante");

                return datos;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("[REPORTES][CONFIGURACION] Llamada={Llamada} | {Mensaje}", llamada, ex.Message);
                return Error("La dirección del servicio de Reportes no está configurada correctamente. Solicite revisar DocsManager:ApiReporteUrl.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "[REPORTES][CONEXION] Llamada={Llamada}", llamada);
                return Error("No se pudo conectar con el servicio de reportes. Verifique la conexión.");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "[REPORTES][TIMEOUT] Llamada={Llamada}", llamada);
                return Error("La solicitud al servicio de reportes excedió el tiempo límite");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[REPORTES][ERROR] Llamada={Llamada}", llamada);
                return Error("Error interno al generar el reporte");
            }
        }

        private static Uri CrearUrlGeneracion(string? configuracion)
        {
            if (!Uri.TryCreate(configuracion?.Trim(), UriKind.Absolute, out var baseUri) ||
                (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps) ||
                !string.IsNullOrEmpty(baseUri.UserInfo) || !string.IsNullOrEmpty(baseUri.Query) ||
                !string.IsNullOrEmpty(baseUri.Fragment))
                throw new ArgumentException("DocsManager:ApiReporteUrl debe ser una URL base HTTP/HTTPS absoluta, sin credenciales, query ni fragmento.");

            // La ruta relativa conserva la subaplicación IIS. Evitar // al final de la base.
            var directorio = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/");
            return new Uri(directorio, RutaGenerar);
        }

        private void RegistrarErrorRespuesta(string llamada, string texto, string token)
        {
            var detalle = CajaJsonDiagnostico.Formatear(texto, token);
            const int limite = 8192;
            if (detalle.Length > limite) detalle = detalle[..limite] + "\n[respuesta truncada]";
            _logger.LogWarning("[REPORTES][ERROR-HTTP] Llamada={Llamada}\n{Respuesta}", llamada, detalle);
        }

        private static RespuestaReportDto Error(string mensaje) => new()
        {
            resultado = -1,
            resultado_msj = mensaje,
            Base64 = string.Empty
        };
    }
}
