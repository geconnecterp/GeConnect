using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace gc.caja.core.Servicios.Implementacion.Cajas
{
    /// <summary>
    /// ✅ ACTUALIZADO v11.3: Servicio para obtener reportes desde API externa
    /// MEJORADO: Logs exhaustivos para debugging
    /// </summary>
    public class ReportesService : IReportesService
    {
        private const string RutaAPI = "/api/reportes";
        private const string RutaGenerar = "/generate";

        private readonly IConfiguration _configuration;
        private readonly ILogger<ReportesService> _logger;

        public ReportesService(IConfiguration configuration, ILogger<ReportesService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RespuestaReportDto> ObtenerPdfDesdeAPI(ReporteSolicitudDto reporteSolicitud, string token)
        {
            try
            {
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation("📡 INVOCAR API DE REPORTES v11.3");
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation($"   Reporte ID: {reporteSolicitud.Reporte}");
                _logger.LogInformation($"   Título: {reporteSolicitud.Titulo}");
                _logger.LogInformation($"   Formato: {reporteSolicitud.Formato}");
                _logger.LogInformation($"   Cantidad de Parámetros: {reporteSolicitud.Parametros?.Count ?? 0}");

                // ❶ LOGS DE PARÁMETROS
                if (reporteSolicitud.Parametros != null && reporteSolicitud.Parametros.Any())
                {
                    _logger.LogInformation("   📋 PARÁMETROS DEL REPORTE:");
                    foreach (var param in reporteSolicitud.Parametros)
                    {
                        _logger.LogInformation($"      - {param.Key}: {param.Value}");
                    }
                }
                else
                {
                    _logger.LogWarning("   ⚠️ No hay parámetros en la solicitud");
                }

                // ❷ VALIDAR TOKEN
                _logger.LogInformation("   🔑 VALIDANDO TOKEN:");
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogError("   ❌ Token está vacío o null");
                    return new RespuestaReportDto
                    {
                        resultado = -1,
                        resultado_msj = "Token de autenticación no proporcionado",
                        Base64 = string.Empty
                    };
                }

                _logger.LogInformation($"      ✅ Token presente (longitud: {token.Length})");
                _logger.LogInformation($"      - Primeros 30 caracteres: {token.Substring(0, Math.Min(30, token.Length))}...");
                _logger.LogInformation($"      - Últimos 10 caracteres: ...{token.Substring(Math.Max(0, token.Length - 10))}");

                // ❸ OBTENER URL DE API
                _logger.LogInformation("   🌐 OBTENIENDO URL DE API DE REPORTES:");
                var apiReporteUrl = _configuration.GetValue<string>("DocsManager:ApiReporteUrl");

                if (string.IsNullOrWhiteSpace(apiReporteUrl))
                {
                    _logger.LogError("   ❌ No se encontró 'DocsManager:ApiReporteUrl' en configuración");
                    throw new InvalidOperationException("No se encontró configuración 'DocsManager:ApiReporteUrl' en appsettings.json");
                }

                var urlCompleta = $"{apiReporteUrl}{RutaAPI}{RutaGenerar}";
                _logger.LogInformation($"      ✅ URL Base: {apiReporteUrl}");
                _logger.LogInformation($"      ✅ URL Completa: {urlCompleta}");

                // ❹ INICIALIZAR CLIENTE HTTP
                _logger.LogInformation("   🔧 INICIALIZANDO CLIENTE HTTP:");
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(reporteSolicitud, token, out StringContent content);

                _logger.LogInformation("      ✅ Cliente HTTP inicializado");
                _logger.LogInformation($"      - BaseAddress: {client.BaseAddress}");
                _logger.LogInformation($"      - Timeout: {client.Timeout}");
                _logger.LogInformation($"      - Headers Count: {client.DefaultRequestHeaders.Count()}");

                // ❺ LOGS DE HEADERS
                _logger.LogInformation("   📨 HEADERS DE LA SOLICITUD:");
                foreach (var header in client.DefaultRequestHeaders)
                {
                    var headerValues = string.Join(", ", header.Value);
                    
                    // ✅ CRÍTICO: Ocultar token de autorización en logs
                    if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation($"      - {header.Key}: Bearer [TOKEN OCULTO POR SEGURIDAD]");
                    }
                    else
                    {
                        _logger.LogInformation($"      - {header.Key}: {headerValues}");
                    }
                }

                // ❻ LOGS DE CONTENIDO
                _logger.LogInformation("   📦 CONTENIDO DE LA SOLICITUD:");
                if (content != null)
                {
                    var contentString = await content.ReadAsStringAsync();
                    _logger.LogInformation($"      - Content-Type: {content.Headers.ContentType}");
                    _logger.LogInformation($"      - Tamaño: {contentString.Length} caracteres");
                    _logger.LogInformation($"      - JSON: {contentString}");
                }
                else
                {
                    _logger.LogWarning("      ⚠️ Content es null");
                }

                // ❼ ENVIAR SOLICITUD
                _logger.LogInformation("   ⏳ ENVIANDO SOLICITUD HTTP POST...");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var response = await client.PostAsync(urlCompleta, content);

                stopwatch.Stop();
                _logger.LogInformation($"   ⏱️ Tiempo de respuesta: {stopwatch.ElapsedMilliseconds}ms");

                // ❽ LOGS DETALLADOS DE RESPUESTA
                _logger.LogInformation("   📥 RESPUESTA HTTP RECIBIDA:");
                _logger.LogInformation($"      - Status Code: {(int)response.StatusCode} ({response.StatusCode})");
                _logger.LogInformation($"      - Reason Phrase: {response.ReasonPhrase}");
                _logger.LogInformation($"      - IsSuccessStatusCode: {response.IsSuccessStatusCode}");

                // ❾ LOGS DE HEADERS DE RESPUESTA
                _logger.LogInformation("   📨 HEADERS DE RESPUESTA:");
                foreach (var header in response.Headers)
                {
                    var headerValues = string.Join(", ", header.Value);
                    _logger.LogInformation($"      - {header.Key}: {headerValues}");
                }

                if (response.Content.Headers != null)
                {
                    _logger.LogInformation("   📨 CONTENT HEADERS:");
                    foreach (var header in response.Content.Headers)
                    {
                        var headerValues = string.Join(", ", header.Value);
                        _logger.LogInformation($"      - {header.Key}: {headerValues}");
                    }
                }

                // ❿ VALIDAR STATUS CODE
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("   ❌ STATUS CODE NO ES 200 OK");

                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"      - Contenido de error: {errorContent}");

                    // ✅ LOGS ADICIONALES PARA CASOS ESPECÍFICOS
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _logger.LogError("      ⚠️ ERROR 401: No autorizado - Token inválido o expirado");
                    }
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        _logger.LogError("      ⚠️ ERROR 403: Prohibido - Sin permisos suficientes");
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogError("      ⚠️ ERROR 404: No encontrado - Endpoint incorrecto");
                    }
                    else if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        _logger.LogError("      ⚠️ ERROR 500: Error interno del servidor de reportes");
                    }

                    return new RespuestaReportDto
                    {
                        resultado = -1,
                        resultado_msj = $"Error en API de Reportes: {response.StatusCode} - {response.ReasonPhrase}",
                        Base64 = string.Empty
                    };
                }

                // ⓫ LEER CONTENIDO DE RESPUESTA
                _logger.LogInformation("   📖 LEYENDO CONTENIDO DE RESPUESTA:");
                var stringData = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"      - Tamaño: {stringData?.Length ?? 0} caracteres");

                if (string.IsNullOrWhiteSpace(stringData))
                {
                    _logger.LogError("      ❌ La API devolvió respuesta vacía");

                    return new RespuestaReportDto
                    {
                        resultado = -1,
                        resultado_msj = "La API de Reportes no devolvió datos",
                        Base64 = string.Empty
                    };
                }

                // ⓬ LOG DE PRIMEROS CARACTERES DE RESPUESTA (para debugging)
                _logger.LogInformation($"      - Primeros 200 caracteres: {stringData.Substring(0, Math.Min(200, stringData.Length))}...");

                // ⓭ DESERIALIZAR RESPUESTA
                _logger.LogInformation("   🔄 DESERIALIZANDO RESPUESTA:");

                ApiResponse<RespuestaReportDto>? respuesta = null;

                try
                {
                    respuesta = JsonConvert.DeserializeObject<ApiResponse<RespuestaReportDto>>(stringData);
                    _logger.LogInformation("      ✅ Deserialización exitosa");
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "      ❌ Error al deserializar JSON");
                    _logger.LogError($"      - JSON recibido: {stringData}");

                    return new RespuestaReportDto
                    {
                        resultado = -1,
                        resultado_msj = "Error al procesar respuesta de la API (JSON inválido)",
                        Base64 = string.Empty
                    };
                }

                // ⓮ VALIDAR RESPUESTA DESERIALIZADA
                if (respuesta == null)
                {
                    _logger.LogError("      ❌ Respuesta deserializada es null");

                    return new RespuestaReportDto
                    {
                        resultado = -1,
                        resultado_msj = "Error al procesar respuesta de la API (null)",
                        Base64 = string.Empty
                    };
                }

                _logger.LogInformation("   📊 ESTRUCTURA DE RESPUESTA:");
                _logger.LogInformation($"      - Success: {respuesta.Data.resultado}");
                _logger.LogInformation($"      - Message: {respuesta.Data.resultado_msj}");
                _logger.LogInformation($"      - Base64: {(string.IsNullOrEmpty(respuesta.Data.Base64)?"VACIO":"VIENE")}");

                if (respuesta.Data == null)
                {
                    _logger.LogError("      ❌ respuesta.Data es null");

                    return new RespuestaReportDto
                    {
                        resultado = -1,
                        resultado_msj = "Error al procesar respuesta de la API (Data null)",
                        Base64 = string.Empty
                    };
                }

                // ⓯ LOGS DETALLADOS DE DATA
                _logger.LogInformation("   📄 DATOS DEL REPORTE:");
                _logger.LogInformation($"      - resultado: {respuesta.Data.resultado}");
                _logger.LogInformation($"      - resultado_msj: {respuesta.Data.resultado_msj}");
                _logger.LogInformation($"      - resultado_id: {respuesta.Data.resultado_id}");
                _logger.LogInformation($"      - Base64 es null: {respuesta.Data.Base64 == null}");
                _logger.LogInformation($"      - Base64 longitud: {respuesta.Data.Base64?.Length ?? 0}");

                if (!string.IsNullOrWhiteSpace(respuesta.Data.Base64))
                {
                    _logger.LogInformation($"      - Primeros 50 caracteres Base64: {respuesta.Data.Base64.Substring(0, Math.Min(50, respuesta.Data.Base64.Length))}...");
                }

                // ⓰ RESULTADO FINAL
                _logger.LogInformation("═══════════════════════════════════════════════════");
                if (respuesta.Data.resultado == 0 && !string.IsNullOrWhiteSpace(respuesta.Data.Base64))
                {
                    _logger.LogInformation("✅ PDF OBTENIDO EXITOSAMENTE");
                    _logger.LogInformation($"   Tamaño Base64: {respuesta.Data.Base64.Length} caracteres");
                    _logger.LogInformation($"   Mensaje: {respuesta.Data.resultado_msj}");
                }
                else
                {
                    _logger.LogWarning("⚠️ RESPUESTA CON ADVERTENCIAS O ERRORES");
                    _logger.LogWarning($"   resultado: {respuesta.Data.resultado}");
                    _logger.LogWarning($"   resultado_msj: {respuesta.Data.resultado_msj}");
                }
                _logger.LogInformation("═══════════════════════════════════════════════════");

                return respuesta.Data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "═══════════════════════════════════════════════════");
                _logger.LogError("❌ ERROR DE CONEXIÓN HTTP");
                _logger.LogError("═══════════════════════════════════════════════════");
                _logger.LogError($"   Mensaje: {ex.Message}");
                _logger.LogError($"   InnerException: {ex.InnerException?.Message}");
                _logger.LogError($"   StackTrace: {ex.StackTrace}");

                return new RespuestaReportDto
                {
                    resultado = -1,
                    resultado_msj = "No se pudo conectar con el servicio de reportes. Verifique la conexión.",
                    Base64 = string.Empty
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "═══════════════════════════════════════════════════");
                _logger.LogError("❌ TIMEOUT EN SOLICITUD HTTP");
                _logger.LogError("═══════════════════════════════════════════════════");
                _logger.LogError($"   Mensaje: {ex.Message}");

                return new RespuestaReportDto
                {
                    resultado = -1,
                    resultado_msj = "La solicitud al servicio de reportes excedió el tiempo límite",
                    Base64 = string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "═══════════════════════════════════════════════════");
                _logger.LogError("❌ ERROR INESPERADO");
                _logger.LogError("═══════════════════════════════════════════════════");
                _logger.LogError($"   Tipo: {ex.GetType().Name}");
                _logger.LogError($"   Mensaje: {ex.Message}");
                _logger.LogError($"   InnerException: {ex.InnerException?.Message}");
                _logger.LogError($"   StackTrace: {ex.StackTrace}");

                return new RespuestaReportDto
                {
                    resultado = -1,
                    resultado_msj = "Error interno al generar el reporte",
                    Base64 = string.Empty
                };
            }
        }
    }
}
