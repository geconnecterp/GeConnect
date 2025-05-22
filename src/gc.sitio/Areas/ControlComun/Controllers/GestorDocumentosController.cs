using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;

namespace gc.sitio.Areas.ControlComun.Controllers
{
    [Area("ControlComun")]
    public class GestorDocumentosController : ControladorBase
    {
        private readonly AppSettings _settings;
        private readonly IDocManagerServicio _docMSv;
        private readonly IWebHostEnvironment _env;

        public GestorDocumentosController(
            IOptions<AppSettings> options,
            IHttpContextAccessor accessor,
            ILogger<GestorDocumentosController> logger,
            IDocManagerServicio docManager,
            IWebHostEnvironment env)
            : base(options, accessor, logger)
        {
            _settings = options.Value;
            _docMSv = docManager;
            _env = env;
        }

        [HttpPost]
        public IActionResult ObtenerModalGestor(string modulo, params string[] parametros)
        {
            try
            {
                _logger?.LogInformation($"Iniciando ObtenerModalGestor para módulo {modulo}");

                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    _logger?.LogWarning("Sesión caducada en ObtenerModalGestor");
                    return Json(new { error = true, warn = false, msg = "Su sesión ha caducado. Por favor, inicie sesión nuevamente.", auth = true });
                }

                // Obtener el objeto DocumentManager de la sesión (fue inicializado en el controlador original)
                var docMgr = DocumentManager;

                if (docMgr == null)
                {
                    _logger?.LogWarning("DocumentManager no inicializado en ObtenerModalGestor");
                    return Json(new { error = true, warn = false, msg = "No se pudo inicializar el gestor documental." });
                }

                // Devolver la vista parcial con el modelo
                return PartialView("~/Areas/ControlComun/Views/GestorDocumental/_gestorDocumentalModal.cshtml", docMgr);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener el modal del gestor documental");
                return Json(new { error = true, warn = false, msg = "Error al cargar el gestor documental: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult ObtenerArbolDocumentos()
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión ha caducado. Por favor, inicie sesión nuevamente." });
                }

                // Obtener el árbol de archivos de la sesión
                var arbol = ArchivosCargadosModulo;
                var jsonArbol = JsonConvert.SerializeObject(arbol);

                // Obtener los datos de la cuenta para completar campos de contacto
                var cuenta = CuentaComercialDatosSeleccionada;

                return Json(new { error = false, warn = false, arbol = jsonArbol, cuenta });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener el árbol de documentos");
                return Json(new { error = true, warn = false, msg = "Error al cargar los documentos: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ImprimirDocumentos(ReporteSolicitudDto solicitud)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión ha caducado. Por favor, inicie sesión nuevamente." });
                }

                //if (solicitudes == null || !solicitudes.Any())
                //{
                //    return Json(new { error = false, warn = true, msg = "No se han seleccionado documentos para imprimir." });
                //}

                List<string> resultados = new List<string>();
                bool hayErrores = false;

                //foreach (var solicitud in solicitudes)
                //{
                //    try
                //    {
                //        var resultado = await _docMSv.ObtenerPdfDesdeAPI(solicitud, TokenCookie);

                //        if (resultado.resultado == 0)
                //        {
                //            resultados.Add(resultado.Base64);
                //        }
                //        else
                //        {
                //            hayErrores = true;
                //            resultados.Add(string.Empty);
                //        }
                //    }
                //    catch
                //    {
                //        hayErrores = true;
                //        resultados.Add(string.Empty);
                //    }
                //}


                var result = await _docMSv.ObtenerPdfDesdeAPI(solicitud, TokenCookie);

                if (result.resultado == 0)
                { //todo fue bien
                    return Json(new { error = false, warn = false, base64 = result.Base64 });
                }
                else
                {
                    throw new NegocioException(result.resultado_msj);
                }


                //string mensaje = hayErrores
                //    ? "Se encontraron errores al generar algunos documentos."
                //    : "Documentos generados correctamente.";

                //return Json(new
                //{
                //    error = false,
                //    warn = hayErrores,
                //    documentos = resultados,
                //    msg = mensaje
                //});
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al imprimir documentos");
                return Json(new { error = true, warn = false, msg = "Error al imprimir los documentos: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ExportarDocumentos(List<ReporteSolicitudDto> solicitudes, string formato)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión ha caducado. Por favor, inicie sesión nuevamente." });
                }

                if (solicitudes == null || !solicitudes.Any())
                {
                    return Json(new { error = false, warn = true, msg = "No se han seleccionado documentos para exportar." });
                }

                List<ExportResponseDto> resultados = new List<ExportResponseDto>();
                bool hayErrores = false;

                foreach (var solicitud in solicitudes)
                {
                    try
                    {
                        solicitud.Formato = formato; // Asignar el formato seleccionado
                        var resultado = await _docMSv.ObtenerRepoDesdeAPI(solicitud, TokenCookie);

                        if (resultado.resultado == 0)
                        {
                            resultados.Add(new ExportResponseDto
                            {
                                Base64 = resultado.Base64,
                                Nombre = resultado.resultado_msj ?? $"documento_{DateTime.Now.Ticks}.{ObtenerExtension(formato)}"
                            });
                        }
                        else
                        {
                            hayErrores = true;
                            resultados.Add(null);
                        }
                    }
                    catch
                    {
                        hayErrores = true;
                        resultados.Add(null);
                    }
                }

                string mensaje = hayErrores
                    ? "Se encontraron errores al generar algunos documentos."
                    : "Documentos exportados correctamente.";

                return Json(new
                {
                    error = false,
                    warn = hayErrores,
                    documentos = resultados,
                    msg = mensaje
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al exportar documentos");
                return Json(new { error = true, warn = false, msg = "Error al exportar los documentos: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> EnviarEmail(List<ReporteSolicitudDto> solicitudes, string emailTo, string emailSubject, string emailBody)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión ha caducado. Por favor, inicie sesión nuevamente." });
                }

                if (solicitudes == null || !solicitudes.Any())
                {
                    return Json(new { error = false, warn = true, msg = "No se han seleccionado documentos para enviar por email." });
                }

                if (string.IsNullOrEmpty(emailTo))
                {
                    return Json(new { error = false, warn = true, msg = "Debe especificar un destinatario de email." });
                }

                var message = new MailMessage();
                message.From = new MailAddress(_settings.CredUserEmail);
                message.To.Add(new MailAddress(emailTo));
                message.Subject = emailSubject ?? "Documentos del sistema";
                message.Body = emailBody ?? "Adjunto los documentos solicitados.";
                message.IsBodyHtml = true;

                bool hayErrores = false;

                // Procesar cada solicitud
                foreach (var solicitud in solicitudes)
                {
                    try
                    {
                        solicitud.Formato = "P"; // PDF para adjuntos de email
                        var resultado = await _docMSv.ObtenerPdfDesdeAPI(solicitud, TokenCookie);

                        if (resultado.resultado == 0)
                        {
                            var archivoBytes = Convert.FromBase64String(resultado.Base64);
                            var archivoStream = new MemoryStream(archivoBytes);

                            string nombreAdjunto = $"documento_{DateTime.Now.Ticks}.pdf";
                            if (!string.IsNullOrEmpty(resultado.resultado_msj) &&
                                !resultado.resultado_msj.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                            {
                                nombreAdjunto = resultado.resultado_msj + ".pdf";
                            }
                            else if (!string.IsNullOrEmpty(resultado.resultado_msj))
                            {
                                nombreAdjunto = resultado.resultado_msj;
                            }

                            message.Attachments.Add(new Attachment(archivoStream, nombreAdjunto, "application/pdf"));
                        }
                        else
                        {
                            hayErrores = true;
                        }
                    }
                    catch
                    {
                        hayErrores = true;
                    }
                }

                // Enviar el email
                using (var smtp = new SmtpClient())
                {
                    smtp.Host = _settings.ServerSMTP;
                    smtp.Port = _settings.Port.ToInt();
                    smtp.Credentials = new NetworkCredential(_settings.CredUserEmail, _settings.CredPass);
                    smtp.EnableSsl = _settings.EnabledSSL;
                    smtp.Send(message);
                }

                string mensaje = hayErrores
                    ? "Se enviaron algunos documentos, pero hubo errores al generar otros."
                    : "Email enviado correctamente.";

                return Json(new
                {
                    error = false,
                    warn = hayErrores,
                    msg = mensaje
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al enviar email");
                return Json(new { error = true, warn = false, msg = "Error al enviar el email: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> EnviarWhatsApp(List<ReporteSolicitudDto> solicitudes, string whatsappTo, string whatsappMessage, bool adjuntarArchivos)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión ha caducado. Por favor, inicie sesión nuevamente." });
                }

                if (string.IsNullOrEmpty(whatsappTo))
                {
                    return Json(new { error = false, warn = true, msg = "Debe especificar un número de teléfono." });
                }

                // Si no se adjuntan archivos, simplemente construir y devolver la URL
                if (!adjuntarArchivos || solicitudes == null || !solicitudes.Any())
                {
                    var urlwsp = $"https://api.whatsapp.com/send?phone={whatsappTo}&text={Uri.EscapeDataString(whatsappMessage ?? "")}";
                    return Json(new { error = false, warn = false, url = urlwsp });
                }

                // Si se adjuntan archivos, generar y guardar los PDFs
                List<string> fileLinks = new List<string>();
                bool hayErrores = false;

                var path = Path.Combine(_env.WebRootPath, _settings.FolderArchivo);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var cuenta = CuentaComercialSeleccionada;
                var timestamp = DateTime.Now.Ticks;

                foreach (var solicitud in solicitudes)
                {
                    try
                    {
                        solicitud.Formato = "P"; // PDF para WhatsApp
                        var resultado = await _docMSv.ObtenerPdfDesdeAPI(solicitud, TokenCookie);

                        if (resultado.resultado == 0)
                        {
                            var archivoBytes = Convert.FromBase64String(resultado.Base64);

                            string nombreArchivo = $"documento_{timestamp}.pdf";
                            if (!string.IsNullOrEmpty(resultado.resultado_msj))
                            {
                                nombreArchivo = resultado.resultado_msj.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                                    ? resultado.resultado_msj
                                    : resultado.resultado_msj + ".pdf";
                            }

                            string idCuenta = cuenta?.Cta_Id ?? "0";
                            string filePath = Path.Combine(path, $"{nombreArchivo.Replace(".pdf", "")}-{idCuenta}-{timestamp}.pdf");
                            System.IO.File.WriteAllBytes(filePath, archivoBytes);

                            string fileName = Path.GetFileName(filePath);
                            fileLinks.Add($"{_settings.RutaBase}/{_settings.FolderArchivo}/{fileName}");
                        }
                        else
                        {
                            hayErrores = true;
                        }
                    }
                    catch
                    {
                        hayErrores = true;
                    }
                }

                // Construir el mensaje con enlaces a los archivos
                string messageWithLinks = whatsappMessage ?? "";
                if (fileLinks.Any())
                {
                    messageWithLinks += "\n\nArchivos:\n" + string.Join("\n", fileLinks);
                }

                var url = $"https://api.whatsapp.com/send?phone={whatsappTo}&text={Uri.EscapeDataString(messageWithLinks)}";

                string mensaje = hayErrores
                    ? "Se prepararon algunos documentos, pero hubo errores con otros."
                    : "Mensaje de WhatsApp preparado correctamente.";

                return Json(new
                {
                    error = false,
                    warn = hayErrores,
                    url = url,
                    msg = mensaje
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al preparar mensaje de WhatsApp");
                return Json(new { error = true, warn = false, msg = "Error al preparar el mensaje de WhatsApp: " + ex.Message });
            }
        }

        #region Métodos auxiliares
        private string ObtenerExtension(string formato)
        {
            return formato switch
            {
                "P" => "pdf",
                "X" => "xlsx",
                "T" => "txt",
                _ => "bin"
            };
        }
        #endregion
    }

    // Clase auxiliar para respuestas de exportación
    public class ExportResponseDto
    {
        public string Base64 { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }
}
