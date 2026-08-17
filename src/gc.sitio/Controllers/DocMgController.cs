using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text; // ✅ AGREGADO

namespace gc.sitio.Controllers
{
    /// <summary>
    /// Controlador para generación de documentos on-demand desde URLs Públicas
    /// ✅ ACCESO PÚBLICO (sin autenticación)
    /// </summary>
    [AllowAnonymous]
    [EnableCors("AllowEmailClients")]
    public class DocMgController : Controller
    {
        private readonly IDocManagerServicio _docMgrServicio;
        private readonly ILogger<DocMgController> _logger;
        private readonly AppSettings _settings;
        private readonly DocsManager _docsManager;

        public DocMgController(
            IDocManagerServicio docMgrServicio,
            ILogger<DocMgController> logger,
            IOptions<AppSettings> settings,
            IOptions<DocsManager> docsManager)
        {
            _docMgrServicio = docMgrServicio;
            _logger = logger;
            _settings = settings.Value;
            _docsManager = docsManager.Value;
        }

        /// <summary>
        /// ✅ NUEVO: Genera documento PDF usando código temporal de LinkController
        /// GET: /d/{codigo}
        /// ACCESO PÚBLICO (sin autenticación)
        /// </summary>
        [HttpGet("/d/{codigo}")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerarDocumentoPorCodigo(string codigo)
        {
            long? accesoId = null;
            var cronometro = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📥 Solicitud de documento con código: {Codigo}", codigo);

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    return BadRequest(new
                    {
                        error = true,
                        mensaje = "Código inválido"
                    });
                }

                // ✅ PASO 1: Obtener ReporteSolicitudDto desde LinkController
                ReporteSolicitudDto? solicitud;

                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(30);

                    var apiUrl = _docsManager.ApiReporteUrl?.TrimEnd('/')
                        ?? throw new Exception("ApiReporteUrl no configurada");
                    var ctlr = _docsManager.ApiLink;
                    var obtener = _docsManager.Obtener;

                    var obtenerSolicitudUrl = $"{apiUrl}/api/{ctlr}/{obtener}?codigo={codigo}";

                    _logger.LogDebug("📡 Llamando a: {Url}", obtenerSolicitudUrl);

                    using var request = new HttpRequestMessage(HttpMethod.Get, obtenerSolicitudUrl);
                    request.Headers.TryAddWithoutValidation(
                        "X-Geco-Client-IP",
                        HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
                    request.Headers.TryAddWithoutValidation(
                        "X-Geco-User-Agent",
                        Request.Headers.UserAgent.ToString());
                    request.Headers.TryAddWithoutValidation(
                        "X-Geco-Referer",
                        Request.Headers.Referer.ToString());

                    var response = await httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError(
                            "❌ Error al obtener solicitud: {StatusCode} - {Error}",
                            response.StatusCode,
                            errorContent);

                        // ✅ MODIFICADO: Procesar múltiples errores de ErrorResponse
                        try
                        {
                            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);

                            // ✅ NUEVO: Concatenar todos los mensajes de error elegantemente
                            string mensaje = "Código inválido o expirado"; // Mensaje por defecto

                            if (errorResponse?.Error != null && errorResponse.Error.Count > 0)
                            {
                                var sb = new StringBuilder();

                                // ✅ Caso 1: Si hay un solo error, mostrar directamente
                                if (errorResponse.Error.Count == 1)
                                {
                                    var error = errorResponse.Error[0];
                                    mensaje = !string.IsNullOrWhiteSpace(error.Detail)
                                        ? error.Detail
                                        : error.Title ?? "Error desconocido";
                                }
                                else
                                {
                                    // ✅ Caso 2: Si hay múltiples errores, concatenar elegantemente
                                    sb.AppendLine("Se encontraron los siguientes problemas:");
                                    sb.AppendLine();

                                    for (int i = 0; i < errorResponse.Error.Count; i++)
                                    {
                                        var error = errorResponse.Error[i];
                                        var errorMsg = !string.IsNullOrWhiteSpace(error.Detail)
                                            ? error.Detail
                                            : error.Title ?? "Error desconocido";

                                        sb.AppendLine($"• {errorMsg}");
                                    }

                                    mensaje = sb.ToString().TrimEnd();
                                }

                                _logger.LogWarning("⚠️ Errores de validación: {Mensaje}", mensaje);
                            }

                            return StatusCode((int)response.StatusCode, new
                            {
                                error = true,
                                mensaje = mensaje
                            });
                        }
                        catch (JsonException jsonEx)
                        {
                            _logger.LogWarning(jsonEx, "⚠️ No se pudo deserializar ErrorResponse, usando mensaje genérico");

                            return StatusCode((int)response.StatusCode, new
                            {
                                error = true,
                                mensaje = "Código inválido o expirado"
                            });
                        }
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();

                    // ✅ MODIFICADO: Usar JsonConvert en lugar de JsonSerializer
                    var sobre = JObject.Parse(responseContent);
                    var data = sobre.GetValue("data", StringComparison.OrdinalIgnoreCase);
                    var dataObject = data as JObject;
                    var solicitudNueva = dataObject?.GetValue("solicitud", StringComparison.OrdinalIgnoreCase);

                    if (solicitudNueva != null)
                    {
                        var acceso = dataObject?.ToObject<ReporteLinkAccesoResponseDto>();
                        accesoId = acceso?.AccesoId;
                        solicitud = acceso?.Solicitud;
                    }
                    else
                    {
                        // Compatibilidad durante despliegues graduales: una API anterior
                        // devuelve la solicitud directamente y conserva el uso único.
                        solicitud = data?.ToObject<ReporteSolicitudDto>();
                    }
                }

                if (solicitud == null)
                {
                    _logger.LogWarning("⚠️ Solicitud nula después de llamar a LinkController");
                    return BadRequest(new
                    {
                        error = true,
                        mensaje = "No se pudo obtener la información del documento"
                    });
                }

                _logger.LogInformation(
                    "📊 Generando documento: {Titulo} (Código: {Codigo})",
                    solicitud.Titulo,
                    codigo);

                // ✅ PASO 2: Generar PDF (igual que antes)
                var resultado = await _docMgrServicio.ObtenerPdfDesdeAPI(
                    solicitud,
                    tokenCookie: string.Empty); // Público sin autenticación

                if (resultado.resultado != 0)
                {
                    await RegistrarFalloAsync(
                        codigo,
                        accesoId,
                        cronometro.ElapsedMilliseconds,
                        StatusCodes.Status500InternalServerError,
                        resultado.resultado_msj);

                    _logger.LogError(
                        "❌ Error al generar PDF: {Mensaje}",
                        resultado.resultado_msj);

                    return StatusCode(500, new
                    {
                        error = true,
                        mensaje = resultado.resultado_msj
                    });
                }

                var pdfBytes = Convert.FromBase64String(resultado.Base64);

                await ConfirmarDescargaAsync(
                    codigo,
                    accesoId,
                    pdfBytes.LongLength,
                    cronometro.ElapsedMilliseconds);

                _logger.LogInformation(
                    "✅ PDF generado exitosamente: {Tamaño} KB (Código: {Codigo})",
                    pdfBytes.Length / 1024,
                    codigo);

                var nombreArchivo = !string.IsNullOrWhiteSpace(solicitud.Titulo)
                    ? $"{solicitud.Titulo.Replace(" ", "_")}.pdf"
                    : "documento.pdf";

                // ✅ Headers CORS
                Response.Headers.Append("Access-Control-Allow-Origin", "*");
                Response.Headers.Append("Access-Control-Allow-Methods", "GET");
                Response.Headers.Append("X-Content-Type-Options", "nosniff");

                return File(
                    pdfBytes,
                    "application/pdf",
                    nombreArchivo);
            }
            catch (NegocioException ex)
            {
                await RegistrarFalloAsync(
                    codigo,
                    accesoId,
                    cronometro.ElapsedMilliseconds,
                    StatusCodes.Status405MethodNotAllowed,
                    ex.Message);

                _logger.LogError(ex, "❌ Error de validación al intentar obtener el documento según Código: {Codigo}", codigo);
                return StatusCode(StatusCodes.Status405MethodNotAllowed,
                    new
                    {
                        error = true,
                        mensaje = ex.Message
                    });
            }
            catch (Exception ex)
            {
                await RegistrarFalloAsync(
                    codigo,
                    accesoId,
                    cronometro.ElapsedMilliseconds,
                    StatusCodes.Status500InternalServerError,
                    ex.Message);

                _logger.LogError(ex, "❌ Error crítico al generar documento por código: {Codigo}", codigo);

                return StatusCode(500, new
                {
                    error = true,
                    mensaje = "Error interno del servidor al generar el documento."
                });
            }
        }

        private async Task ConfirmarDescargaAsync(
            string codigo,
            long? accesoId,
            long bytes,
            long duracionMs)
        {
            if (!accesoId.HasValue || accesoId.Value <= 0)
            {
                return;
            }

            var dto = new ReporteLinkDescargaDto
            {
                Codigo = codigo,
                AccesoId = accesoId.Value,
                Bytes = bytes,
                DuracionMs = NormalizarDuracion(duracionMs),
                ResultadoHttp = StatusCodes.Status200OK
            };

            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            var apiUrl = _docsManager.ApiReporteUrl?.TrimEnd('/')
                ?? throw new NegocioException("ApiReporteUrl no configurada");
            var url = $"{apiUrl}/api/{_docsManager.ApiLink}/ConfirmarDescarga";
            using var content = new StringContent(
                JsonConvert.SerializeObject(dto),
                Encoding.UTF8,
                "application/json");
            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var detalle = await response.Content.ReadAsStringAsync();
                throw new NegocioException(ObtenerMensajeError(
                    detalle,
                    "No se pudo confirmar la descarga del documento."));
            }
        }

        private async Task RegistrarFalloAsync(
            string codigo,
            long? accesoId,
            long duracionMs,
            int resultadoHttp,
            string? detalle)
        {
            if (!accesoId.HasValue || accesoId.Value <= 0)
            {
                return;
            }

            try
            {
                var dto = new ReporteLinkDescargaDto
                {
                    Codigo = codigo,
                    AccesoId = accesoId.Value,
                    DuracionMs = NormalizarDuracion(duracionMs),
                    ResultadoHttp = resultadoHttp,
                    Detalle = detalle
                };

                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };
                var apiUrl = _docsManager.ApiReporteUrl?.TrimEnd('/');
                if (string.IsNullOrWhiteSpace(apiUrl))
                {
                    return;
                }

                var url = $"{apiUrl}/api/{_docsManager.ApiLink}/RegistrarFallo";
                using var content = new StringContent(
                    JsonConvert.SerializeObject(dto),
                    Encoding.UTF8,
                    "application/json");
                await httpClient.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "No se pudo registrar el fallo de descarga para el código {Codigo}",
                    codigo);
            }
        }

        private static int NormalizarDuracion(long duracionMs)
        {
            return duracionMs > int.MaxValue ? int.MaxValue : (int)duracionMs;
        }

        private static string ObtenerMensajeError(string contenido, string mensajePredeterminado)
        {
            try
            {
                var error = JsonConvert.DeserializeObject<ErrorResponse>(contenido);
                var detalle = error?.Error?.FirstOrDefault()?.Detail;
                return string.IsNullOrWhiteSpace(detalle)
                    ? mensajePredeterminado
                    : detalle;
            }
            catch (JsonException)
            {
                return mensajePredeterminado;
            }
        }

        /// <summary>
        /// ⚠️ DEPRECADO: Mantener por compatibilidad con enlaces antiguos
        /// GET: /docmanager/{parametros}
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [Obsolete("Usar /d/{codigo} en su lugar")]
        public async Task<IActionResult> Index(string parametros)
        {
            _logger.LogWarning("⚠️ Usando endpoint DEPRECADO /docmanager con parámetros heredados");

            return StatusCode(410, new
            {
                error = true,
                mensaje = "Este endpoint está deprecado. Por favor, solicita un nuevo enlace."
            });
        }
    }
}
