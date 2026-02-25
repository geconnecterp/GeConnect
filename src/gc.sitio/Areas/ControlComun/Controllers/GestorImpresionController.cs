using ClosedXML.Excel;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace gc.sitio.Areas.ControlComun.Controllers
{
    [Area("ControlComun")]
    public class GestorImpresionController : ControladorBase
    {
        private readonly AppSettings _setting;
        private readonly IHttpContextAccessor _accessor;
        private readonly IDocManagerServicio _docMSv;
        private readonly IWebHostEnvironment _env;


        public GestorImpresionController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<GestorImpresionController> logger,
            IDocManagerServicio docManager, IWebHostEnvironment env)
            : base(options, accessor, logger)
        {
            _setting = options.Value;
            _accessor = accessor;
            _docMSv = docManager;
            _env = env;
        }



        [HttpPost]
        public IActionResult OrquestadorDeModulos(string modulo, params string[] parametros)
        {
            RespuestaGenerica<EntidadBase> response = new();

            //esta action tendrá como funcion enviar al componente de impresión, la configuración
            //del modulo respecto a la funcionalidad que tendrá permitida
            try
            {
                //el VM se va cargado en el Modulo origen a medida que se van ejecutando las consultas.
                var docMgr = DocumentManager;
                return View("~/areas/ControlComun/views/GestorImpresion/_docManagerModal.cshtml", docMgr);
            }
            catch (NegocioException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {

                string msg = "Error en la obtención de la configuración para el Gestor Documental.";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<JsonResult> ObtenerPdfDesdeAPI(ReporteSolicitudDto reporteSolicitud)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                //invocamos servicio para obtener pdf
                var result = await _docMSv.ObtenerPdfDesdeAPI(reporteSolicitud, TokenCookie);

                if (result.resultado == 0)
                {
                    //todo fue bien
                    return Json(new { error = false, warn = false, base64 = result.Base64 });
                }
                else
                {
                    throw new NegocioException(result.resultado_msj);
                }

            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, auth = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al presentar los archivos");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> GeneradorArchivo(ReporteSolicitudDto reporteSolicitud)
        {
            RespuestaReportDto? response = null; // Initialize the variable to avoid CS0165
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                switch (reporteSolicitud.Formato)
                {
                    case "P":
                        // Invocamos servicio para obtener pdf
                        response = await _docMSv.ObtenerPdfDesdeAPI(reporteSolicitud, TokenCookie);
                        break;
                    case "X":
                        response = await _docMSv.ObtenerRepoDesdeAPI(reporteSolicitud, TokenCookie);
                        break;
                    case "T":
                        response = await _docMSv.ObtenerRepoDesdeAPI(reporteSolicitud, TokenCookie);
                        break;
                    default:
                        throw new NegocioException("Formato no soportado.");
                }

                if (response != null && response.resultado == 0)
                {
                    // Todo fue bien
                    return Json(new { error = false, warn = false, base64 = response.Base64, name = response.resultado_msj });
                }
                else
                {
                    throw new NegocioException(response?.resultado_msj ?? "Error desconocido.");
                }
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, auth = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }


        [HttpPost]
        public JsonResult PresentarArchivos()
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                var arbol = ArchivosCargadosModulo;
                var jarbol = JsonConvert.SerializeObject(arbol);
                CuentaDatoDto cuenta = new();
                if (CuentaComercialDatosSeleccionada != null)
                {
                    cuenta = CuentaComercialDatosSeleccionada;
                }

                return Json(new { error = false, warn = false, arbol = jarbol, cuenta });

            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, auth = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al presentar los archivos");
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }




        [HttpPost]
        public JsonResult EnviarEmail(List<ArchivoSendDto> archivos, string emailTo, string emailSubject, string emailBody)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                var message = new MailMessage();
                message.From = new MailAddress(_setting.CredUserEmail); // Establecer la dirección de correo del remitente
                message.To.Add(new MailAddress(emailTo));
                message.Subject = emailSubject;
                message.Body = emailBody;
                message.IsBodyHtml = true;

                foreach (var archivo in archivos)
                {
                    var archivoBytes = Convert.FromBase64String(archivo.archivoBase64);
                    var archivoStream = new MemoryStream(archivoBytes);
                    message.Attachments.Add(new Attachment(archivoStream, archivo.nombre, "application/pdf"));
                }

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = _setting.ServerSMTP;
                    smtp.Port = _setting.Port.ToInt();
                    smtp.Credentials = new NetworkCredential(_setting.CredUserEmail, _setting.CredPass);
                    smtp.EnableSsl = _setting.EnabledSSL;
                    smtp.Send(message);
                }

                return Json(new { error = false, warn = false });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EnviarWhatsApp(List<ArchivoSendDto> archivos, string whatsappTo, string whatsappMessage)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                if (archivos.Count == 0)
                {
                    // Construir la URL de la API de WhatsApp
                    var url = $"https://api.whatsapp.com/send?phone={whatsappTo}&text={Uri.EscapeDataString(whatsappMessage)}";
                    return Json(new { error = false, warn = false, url = url, msg = $"Mensaje enviado a {whatsappTo} satisfactoriamente" });
                }
                else
                {
                    var cuentaActual = CuentaComercialSeleccionada;
                    if (cuentaActual == null)
                    {
                        throw new NegocioException("No se ha seleccionado una cuenta comercial.");
                    }
                    var ahora = DateTime.Now.Ticks;
                    // Guardar los archivos en el servidor
                    var fileLinks = new List<string>();
                    var path = Path.Combine(_env.WebRootPath, _setting.FolderArchivo);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    foreach (var archivo in archivos)
                    {
                        var archivoBytes = Convert.FromBase64String(archivo.archivoBase64);
                        var filePath = Path.Combine(path, $"{archivo.nombre}-{cuentaActual.Cta_Id}-{ahora}.pdf");
                        System.IO.File.WriteAllBytes(filePath, archivoBytes);
                        fileLinks.Add($"{_setting.RutaBase}/{_setting.FolderArchivo}/{archivo.nombre}");
                    }

                    // Construir el mensaje con enlaces a los archivos
                    var messageWithLinks = $"{whatsappMessage}\n\nArchivos:\n" + string.Join("\n", fileLinks);
                    var url = $"https://api.whatsapp.com/send?phone={whatsappTo}&text={Uri.EscapeDataString(messageWithLinks)}";
                    return Json(new { error = false, warn = false, url });
                }
                //TwilioClient.Init(_setting.WspAccountSID, _setting.WspAuthToken);

                //var mediaUrls = new List<Uri>();
                //foreach (var archivo in archivos)
                //{
                //    mediaUrls.Add(new Uri("data:application/pdf;base64," + archivo.archivoBase64));
                //}

                //// Limpiar el número de teléfono
                //whatsappTo = Regex.Replace(whatsappTo, @"[\s\-\.\(\)]", "");

                //var message = MessageResource.Create(
                //    body: whatsappMessage,
                //    from: new Twilio.Types.PhoneNumber($"whatsapp:{_setting.WspNroTelefono}"),
                //    to: new Twilio.Types.PhoneNumber("whatsapp:" + whatsappTo),
                //    mediaUrl: mediaUrls
                //);

                //return Json(new { error = false, warn = false });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        /// <summary>
        /// Genera un enlace mailto: para abrir Outlook Desktop con el borrador prellenado
        /// </summary>
        [HttpPost]
        public JsonResult GenerateMailtoLink([FromBody] MailtoRequest request)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                if (string.IsNullOrWhiteSpace(request.To))
                {
                    return Json(new { success = false, message = "El destinatario es obligatorio" });
                }

                // Construir enlace mailto con CC/BCC
                var mailtoBuilder = new System.Text.StringBuilder("mailto:");
                mailtoBuilder.Append(Uri.EscapeDataString(request.To));

                var queryParams = new List<string>();

                // Agregar CC si existe
                if (!string.IsNullOrWhiteSpace(request.Cc))
                {
                    queryParams.Add($"cc={Uri.EscapeDataString(request.Cc)}");
                }

                // Agregar BCC si existe
                if (!string.IsNullOrWhiteSpace(request.Bcc))
                {
                    queryParams.Add($"bcc={Uri.EscapeDataString(request.Bcc)}");
                }

                // Agregar Subject
                if (!string.IsNullOrWhiteSpace(request.Subject))
                {
                    queryParams.Add($"subject={Uri.EscapeDataString(request.Subject)}");
                }

                // Agregar Body
                if (!string.IsNullOrWhiteSpace(request.Body))
                {
                    queryParams.Add($"body={Uri.EscapeDataString(request.Body)}");
                }

                if (queryParams.Count > 0)
                {
                    mailtoBuilder.Append("?");
                    mailtoBuilder.Append(string.Join("&", queryParams));
                }

                var mailtoLink = mailtoBuilder.ToString();

                _logger?.LogInformation(
                    "Enlace mailto generado para {To} (CC: {Cc}, BCC: {Bcc}) con asunto '{Subject}'",
                    request.To,
                    request.Cc ?? "ninguno",
                    request.Bcc ?? "ninguno",
                    request.Subject
                );

                return Json(new
                {
                    success = true,
                    message = "Se abrirá Outlook local con el borrador",
                    mailtoLink = mailtoLink,
                    provider = "Outlook Desktop (Cliente Local)",
                    note = "⚠️ Debes seleccionar manualmente la cuenta remitente y adjuntar archivos manualmente."
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al generar enlace mailto");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Genera un enlace deeplink para abrir Outlook Web con el borrador prellenado
        /// </summary>
        [HttpPost]
        public JsonResult GenerateOutlookWebLink([FromBody] MailtoRequest request)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                if (string.IsNullOrWhiteSpace(request.To))
                {
                    return Json(new { success = false, message = "El destinatario es obligatorio" });
                }

                // Construir deeplink de Outlook Web con parámetros escapados
                var urlBuilder = new System.Text.StringBuilder("https://outlook.office.com/mail/deeplink/compose");
                urlBuilder.Append($"?to={Uri.EscapeDataString(request.To)}");

                // Agregar CC si existe
                if (!string.IsNullOrWhiteSpace(request.Cc))
                {
                    urlBuilder.Append($"&cc={Uri.EscapeDataString(request.Cc)}");
                }

                // Agregar BCC si existe
                if (!string.IsNullOrWhiteSpace(request.Bcc))
                {
                    urlBuilder.Append($"&bcc={Uri.EscapeDataString(request.Bcc)}");
                }

                // Agregar Subject
                if (!string.IsNullOrWhiteSpace(request.Subject))
                {
                    urlBuilder.Append($"&subject={Uri.EscapeDataString(request.Subject)}");
                }

                // Agregar Body
                if (!string.IsNullOrWhiteSpace(request.Body))
                {
                    urlBuilder.Append($"&body={Uri.EscapeDataString(request.Body)}");
                }

                var outlookWebUrl = urlBuilder.ToString();

                _logger?.LogInformation(
                    "Enlace de Outlook Web generado para {To} (CC: {Cc}, BCC: {Bcc}) con asunto '{Subject}'",
                    request.To,
                    request.Cc ?? "ninguno",
                    request.Bcc ?? "ninguno",
                    request.Subject
                );

                return Json(new
                {
                    success = true,
                    message = "Se abrirá Outlook Web en una nueva pestaña",
                    outlookWebLink = outlookWebUrl,
                    provider = "Outlook Web (Microsoft 365)",
                    note = "⚠️ Requiere sesión activa en Microsoft 365. Los adjuntos deben agregarse manualmente."
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al generar enlace de Outlook Web");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Genera un enlace de WhatsApp Web con el mensaje prellenado (método gratuito)
        /// </summary>
        [HttpPost]
        public JsonResult GenerateWhatsAppWebLink([FromBody] WhatsAppRequest request)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { success = false, message = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(request.To))
                {
                    return Json(new { success = false, message = "El número de teléfono es obligatorio" });
                }

                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return Json(new { success = false, message = "El mensaje es obligatorio" });
                }

                // Limpiar el número (quitar espacios, guiones, paréntesis)
                var cleanNumber = new string(request.To.Where(c => char.IsDigit(c) || c == '+').ToArray());

                // Validar que tenga código de país
                if (!cleanNumber.StartsWith("+"))
                {
                    return Json(new
                    {
                        success = false,
                        message = "El número debe incluir el código de país (ej: +5491123456789 para Argentina)"
                    });
                }

                // Remover el símbolo + para la URL de WhatsApp
                var numberForUrl = cleanNumber.TrimStart('+');

                // Validar longitud del mensaje (máximo 5000 caracteres recomendado)
                if (request.Message.Length > 5000)
                {
                    return Json(new
                    {
                        success = false,
                        message = "El mensaje es muy largo. Máximo recomendado: 5000 caracteres."
                    });
                }

                // Construir URL de WhatsApp Web (API oficial - gratuita)
                var whatsappWebUrl = $"https://wa.me/{numberForUrl}?text={Uri.EscapeDataString(request.Message)}";

                _logger?.LogInformation(
                    "Enlace de WhatsApp Web generado para {To} con mensaje de {Length} caracteres",
                    cleanNumber,
                    request.Message.Length
                );

                return Json(new
                {
                    success = true,
                    message = "Se abrirá WhatsApp Web en una nueva pestaña",
                    whatsappWebLink = whatsappWebUrl,
                    to = cleanNumber,
                    provider = "WhatsApp Web (API Oficial - Gratis)",
                    note = "⚠️ Confirma el envío en WhatsApp. Los archivos deben adjuntarse manualmente."
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al generar enlace de WhatsApp Web");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Clase DTO para solicitudes de mailto y Outlook Web
        /// </summary>
        public class MailtoRequest
        {
            public string To { get; set; } = string.Empty;
            public string Cc { get; set; } = string.Empty;
            public string Bcc { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public string Body { get; set; } = string.Empty;
        }

        /// <summary>
        /// Clase DTO para solicitudes de WhatsApp
        /// </summary>
        public class WhatsAppRequest
        {
            public string To { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }

        public class ArchivoSendDto
        {
            public string archivoBase64 { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
        }

    }
}
