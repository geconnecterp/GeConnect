using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options; // ✅ NUEVO
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
    public class GestorImpresionController : ControladorBase
    {
        private readonly AppSettings _setting;
        private readonly IHttpContextAccessor _accessor;
        private readonly IDocManagerServicio _docMSv;
        private readonly IWebHostEnvironment _env;
        
        // ✅ NUEVO: Usar la infraestructura existente
        private readonly DocsManager _docsManager;
        private readonly EmpresaGeco _empresaGeco;

        public GestorImpresionController(
            IOptions<AppSettings> options, 
            IHttpContextAccessor accessor, 
            ILogger<GestorImpresionController> logger,
            IDocManagerServicio docManager, 
            IWebHostEnvironment env,
            IOptions<DocsManager> docsManager,        // ✅ EXISTENTE
            IOptions<EmpresaGeco> empresaGeco)        // ✅ EXISTENTE
            : base(options, accessor, logger)
        {
            _setting = options.Value;
            _accessor = accessor;
            _docMSv = docManager;
            _env = env;
            
            // ✅ NUEVO: Inicializar usando infraestructura existente
            _docsManager = docsManager.Value;
            _empresaGeco = empresaGeco.Value;
        }

        [HttpPost]
        public IActionResult OrquestadorDeModulos(string modulo, params string[] parametros)
        {
            RespuestaGenerica<EntidadBase> response = new();

            try
            {
                var docMgr = DocumentManager;
                
                // ✅ Buscar configuración del módulo actual
                var moduloConfig = !string.IsNullOrEmpty(modulo)?
                                _docsManager.Modulos.FirstOrDefault(m => m.Id == modulo):
                                _docsManager.Modulos.FirstOrDefault(m => m.Id == docMgr.Id);
                
                if (moduloConfig != null)
                {
                    // ✅ NUEVO: Pasar configuración de MENSAJERÍA (común para Email y WhatsApp)
                    if (moduloConfig.MensajeriaTemplate != null)
                    {
                        ViewBag.MensajeriaTemplate = moduloConfig.MensajeriaTemplate;
                        
                        _logger?.LogInformation(
                            "📧📱 Plantilla de mensajería cargada para módulo {ModuloId}", 
                            moduloConfig.Id);
                    }
                    
                    // ✅ NUEVO: Pasar configuración de EMAIL (solo asunto)
                    if (moduloConfig.EmailTemplate != null && moduloConfig.Email)
                    {
                        ViewBag.EmailTemplate = moduloConfig.EmailTemplate;
                        
                        _logger?.LogInformation(
                            "📧 Plantilla de email cargada para módulo {ModuloId}: {Asunto}", 
                            moduloConfig.Id, 
                            moduloConfig.EmailTemplate.AsuntoTemplate);
                    }
                    
                    // ✅ NUEVO (opcional futuro): Pasar configuración de WHATSAPP
                    if (moduloConfig.WhatsAppTemplate != null && moduloConfig.Whatsapp)
                    {
                        ViewBag.WhatsAppTemplate = moduloConfig.WhatsAppTemplate;
                        
                        _logger?.LogInformation(
                            "📱 Plantilla de WhatsApp cargada para módulo {ModuloId}", 
                            moduloConfig.Id);
                    }
                    
                    // ✅ Pasar ID y título del módulo
                    ViewBag.ModuloId = moduloConfig.Id;
                    ViewBag.ModuloTitulo = moduloConfig.Titulo;
                    
                    // ✅ Pasar datos de la empresa para el pie
                    ViewBag.EmpresaNombre = _empresaGeco.Nombre;
                    ViewBag.EmpresaTelefono = _empresaGeco.Telefono;
                    ViewBag.EmpresaEmail = _empresaGeco.Email;
                    ViewBag.EmpresaDireccion = _empresaGeco.Direccion;
                    ViewBag.EmpresaLocalidad = _empresaGeco.Localidad;
                    ViewBag.EmpresaProvincia = _empresaGeco.Provincia;
                    
                    _logger?.LogInformation(
                        "🏢 Datos de empresa cargados: {Nombre} - {Email}", 
                        _empresaGeco.Nombre, 
                        _empresaGeco.Email);
                }
                else
                {
                    _logger?.LogWarning("⚠️ No se encontró configuración para el módulo: {Modulo}", modulo);
                    
                    // ✅ FALLBACK: Asignar valores por defecto
                    ViewBag.ModuloId = modulo;
                    ViewBag.ModuloTitulo = "Documentación";
                    ViewBag.MensajeriaTemplate = null;
                    ViewBag.EmailTemplate = null;
                    ViewBag.WhatsAppTemplate = null;
                }
                
                return View("~/areas/ControlComun/views/GestorImpresion/_docManagerModal.cshtml", docMgr);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error de negocio en OrquestadorDeModulos");
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
        public JsonResult EnviarEmail([FromBody] EnviarEmailRequest request)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                // ✅ Validaciones con el DTO
                if (request == null)
                {
                    _logger?.LogWarning("⚠️ Request nulo recibido en EnviarEmail");
                    return Json(new { error = true, warn = false, msg = "Datos de solicitud inválidos" });
                }

                if (string.IsNullOrWhiteSpace(request.EmailTo))
                {
                    _logger?.LogWarning("⚠️ EmailTo vacío");
                    return Json(new { error = true, warn = false, msg = "El destinatario es obligatorio" });
                }

                if (string.IsNullOrWhiteSpace(request.EmailSubject))
                {
                    _logger?.LogWarning("⚠️ EmailSubject vacío");
                    return Json(new { error = true, warn = false, msg = "El asunto es obligatorio" });
                }

                if (string.IsNullOrWhiteSpace(request.EmailBody))
                {
                    _logger?.LogWarning("⚠️ EmailBody vacío");
                    return Json(new { error = true, warn = false, msg = "El mensaje es obligatorio" });
                }

                _logger?.LogInformation(
                    "📧 Procesando envío de email: To={EmailTo}, Subject={Subject}, Archivos={Archivos}",
                    request.EmailTo,
                    request.EmailSubject,
                    request.Archivos?.Count ?? 0
                );

                var message = new MailMessage();
                message.From = new MailAddress(_setting.CredUserEmail);
                
                // ✅ Soportar múltiples destinatarios separados por ; o ,
                var destinatarios = request.EmailTo.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                
                int destinatariosAgregados = 0;
                foreach (var destinatario in destinatarios)
                {
                    var emailLimpio = destinatario.Trim();
                    
                    if (!string.IsNullOrWhiteSpace(emailLimpio))
                    {
                        try
                        {
                            message.To.Add(new MailAddress(emailLimpio));
                            destinatariosAgregados++;
                            _logger?.LogDebug("✅ Destinatario agregado: {Email}", emailLimpio);
                        }
                        catch (FormatException ex)
                        {
                            _logger?.LogWarning(ex, "⚠️ Email inválido ignorado: {Email}", emailLimpio);
                        }
                    }
                }
                
                if (destinatariosAgregados == 0)
                {
                    _logger?.LogWarning("⚠️ No se encontraron destinatarios válidos");
                    return Json(new { error = true, warn = false, msg = "No se encontraron destinatarios válidos" });
                }
                
                message.Subject = request.EmailSubject;
                message.Body = request.EmailBody;
                message.IsBodyHtml = true;

                // ✅ Agregar archivos adjuntos
                if (request.Archivos != null && request.Archivos.Count > 0)
                {
                    foreach (var archivo in request.Archivos)
                    {
                        try
                        {
                            var archivoBytes = Convert.FromBase64String(archivo.archivoBase64);
                            var archivoStream = new MemoryStream(archivoBytes);
                            message.Attachments.Add(new Attachment(archivoStream, archivo.nombre, "application/pdf"));
                            
                            _logger?.LogDebug("📎 Adjunto agregado: {Nombre} ({Tamaño} KB)", 
                                archivo.nombre, 
                                archivoBytes.Length / 1024);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "⚠️ Error al adjuntar archivo: {Nombre}", archivo.nombre);
                        }
                    }
                }

                // ✅ Enviar email
                using (var smtp = new SmtpClient())
                {
                    smtp.Host = _setting.ServerSMTP;
                    smtp.Port = _setting.Port.ToInt();
                    smtp.Credentials = new NetworkCredential(_setting.CredUserEmail, _setting.CredPass);
                    smtp.EnableSsl = _setting.EnabledSSL;
                    smtp.Send(message);
                }
                
                _logger?.LogInformation(
                    "✅ Email enviado exitosamente a {Cantidad} destinatario(s): {Destinatarios} con {CantidadArchivos} archivo(s)", 
                    message.To.Count,
                    string.Join(", ", message.To.Select(m => m.Address)),
                    request.Archivos?.Count ?? 0
                );

                return Json(new { 
                    error = false, 
                    warn = false,
                    destinatarios = message.To.Count,
                    archivos = request.Archivos?.Count ?? 0
                });
            }
            catch (SmtpException ex)
            {
                _logger?.LogError(ex, "❌ Error SMTP al enviar email");
                return Json(new { 
                    error = true, 
                    warn = false, 
                    msg = $"Error al enviar email: {ex.Message}\n\nVerifica la configuración SMTP." 
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error general al enviar email");
                return Json(new { 
                    error = true, 
                    warn = false, 
                    msg = $"Error al enviar email: {ex.Message}" 
                });
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
        /// Guarda archivos grandes en el servidor de archivos remoto y retorna enlaces públicos
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GuardarArchivosGrandes([FromBody] GuardarArchivosRequest request)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = true, msg = "Su sesión se ha terminado." });
                }

                if (request.Archivos == null || !request.Archivos.Any())
                {
                    return Json(new { error = true, msg = "No se recibieron archivos para guardar." });
                }

                _logger?.LogInformation("📥 Recibiendo {Cantidad} archivo(s) grande(s) para guardar", request.Archivos.Count);

                var enlaces = new List<EnlaceArchivoDto>();
                var timestamp = DateTime.Now.Ticks;
                var cuentaId = CuentaComercialSeleccionada?.Cta_Id ?? "TEMP";

                foreach (var archivo in request.Archivos)
                {
                    try
                    {
                        // Decodificar Base64
                        var archivoBytes = Convert.FromBase64String(archivo.ArchivoBase64);
                        
                        // Sanitizar nombre de archivo
                        var nombreSinExtension = Path.GetFileNameWithoutExtension(archivo.Nombre);
                        var extension = Path.GetExtension(archivo.Nombre);
                        var caracteresInvalidos = Path.GetInvalidFileNameChars();
                        foreach (var c in caracteresInvalidos)
                        {
                            nombreSinExtension = nombreSinExtension.Replace(c, '_');
                        }

                        // ✅ NUEVO: Reemplazar espacios en blanco por underscore
                        nombreSinExtension = nombreSinExtension.Replace(' ', '_');

                        // Generar nombre único
                        var nombreUnico = $"{nombreSinExtension}_{cuentaId}_{timestamp}{extension}";
                        
                        // Enviar archivo al servidor remoto
                        var urlPublica = await EnviarArchivoAServidorRemoto(archivoBytes, nombreUnico);
                        
                        if (!string.IsNullOrEmpty(urlPublica))
                        {
                            enlaces.Add(new EnlaceArchivoDto
                            {
                                Nombre = archivo.Nombre,
                                Url = urlPublica
                            });
                            
                            _logger?.LogInformation(
                                "✅ Archivo guardado en servidor remoto: {Nombre} ({Tamaño} KB) → {Url}",
                                archivo.Nombre,
                                archivoBytes.Length / 1024,
                                urlPublica
                            );
                        }
                        else
                        {
                            _logger?.LogWarning("⚠️ No se pudo guardar el archivo: {Nombre}", archivo.Nombre);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "❌ Error al procesar archivo: {Nombre}", archivo.Nombre);
                    }
                }

                if (!enlaces.Any())
                {
                    _logger?.LogWarning("⚠️ No se pudo guardar ningún archivo");
                    return Json(new { error = true, msg = "No se pudo guardar ningún archivo. Revisa los logs." });
                }

                _logger?.LogInformation("✅ {Cantidad} archivo(s) guardado(s) exitosamente", enlaces.Count);

                return Json(new { error = false, enlaces });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error crítico en GuardarArchivosGrandes");
                return Json(new { error = true, msg = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Envía un archivo al servidor remoto FileStore vía HTTP POST
        /// ✅ CORREGIDO: Manejo robusto de errores y logging detallado
        /// </summary>
        private async Task<string?> EnviarArchivoAServidorRemoto(byte[] archivoBytes, string nombreArchivo)
        {
            HttpClient? httpClient = null;
            try
            {
                httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(10);

                // ✅ CORREGIDO: Sanitizar URL (quitar barra final duplicada)
                var rutaBase = _setting.RutaFileServer?.TrimEnd('/') ?? "https://localhost:5001";
                var uploadUrl = $"{rutaBase}/api/upload";

                _logger?.LogInformation("📤 [FileStore] Enviando archivo: {Nombre} ({Tamaño} KB) → {Url}",
                    nombreArchivo,
                    archivoBytes.Length / 1024,
                    uploadUrl);

                // ✅ NUEVO: Validar URL
                if (!Uri.TryCreate(uploadUrl, UriKind.Absolute, out var validatedUri))
                {
                    _logger?.LogError("❌ [FileStore] URL inválida: {Url}", uploadUrl);
                    return null;
                }

                // Crear contenido multipart/form-data
                using var content = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(archivoBytes);

                // Detectar tipo MIME
                var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
                var mimeType = extension switch
                {
                    ".pdf" => "application/pdf",
                    ".xls" => "application/vnd.ms-excel",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ".txt" => "text/plain",
                    _ => "application/octet-stream"
                };

                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
                content.Add(fileContent, "file", nombreArchivo);

                _logger?.LogDebug("📤 [FileStore] Enviando POST a {Url}", validatedUri);

                // ✅ NUEVO: Captura de excepciones específicas
                HttpResponseMessage? response = null;
                try
                {
                    response = await httpClient.PostAsync(validatedUri, content);
                }
                catch (HttpRequestException httpEx)
                {
                    _logger?.LogError(httpEx, "❌ [FileStore] Error HTTP: {StatusCode} - {Message}",
                        httpEx.StatusCode?.ToString() ?? "N/A",
                        httpEx.Message);
                    _logger?.LogError("❌ [FileStore] Verifica que el FileStore esté corriendo en: {Url}", rutaBase);
                    return null;
                }
                catch (TaskCanceledException taskEx)
                {
                    _logger?.LogError(taskEx, "❌ [FileStore] Timeout (10 minutos) al enviar a {Url}", validatedUri);
                    return null;
                }

                _logger?.LogDebug("📤 [FileStore] Response: StatusCode={StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger?.LogDebug("📤 [FileStore] Response JSON: {Content}", responseContent);

                    if (string.IsNullOrWhiteSpace(responseContent))
                    {
                        _logger?.LogError("❌ [FileStore] Response vacío");
                        return null;
                    }

                    try
                    {
                        var jsonResponse = System.Text.Json.JsonDocument.Parse(responseContent);

                        if (!jsonResponse.RootElement.TryGetProperty("url", out var urlProperty))
                        {
                            _logger?.LogError("❌ [FileStore] Response sin propiedad 'url': {Response}", responseContent);
                            return null;
                        }

                        var urlPublica = urlProperty.GetString();

                        if (string.IsNullOrWhiteSpace(urlPublica))
                        {
                            _logger?.LogError("❌ [FileStore] URL pública vacía");
                            return null;
                        }

                        _logger?.LogInformation("✅ [FileStore] Archivo subido: {Url}", urlPublica);

                        return urlPublica;
                    }
                    catch (System.Text.Json.JsonException jsonEx)
                    {
                        _logger?.LogError(jsonEx, "❌ [FileStore] Error JSON: {Content}", responseContent);
                        return null;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger?.LogError("❌ [FileStore] HTTP {StatusCode}: {Error}",
                        response.StatusCode,
                        errorContent);

                    // ✅ Diagnóstico específico por código HTTP
                    switch (response.StatusCode)
                    {
                        case System.Net.HttpStatusCode.BadRequest:
                            _logger?.LogError("❌ [FileStore] Bad Request (400) - Problema con multipart/form-data");
                            break;
                        case System.Net.HttpStatusCode.NotFound:
                            _logger?.LogError("❌ [FileStore] Not Found (404) - URL incorrecta: {Url}", validatedUri);
                            _logger?.LogError("❌ [FileStore] Verifica appsettings.json → RutaFileServer: {Ruta}", rutaBase);
                            break;
                        case System.Net.HttpStatusCode.InternalServerError:
                            _logger?.LogError("❌ [FileStore] Internal Server Error (500) - Problema en FileStore");
                            break;
                        case System.Net.HttpStatusCode.ServiceUnavailable:
                            _logger?.LogError("❌ [FileStore] Service Unavailable (503) - FileStore no responde");
                            break;
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ [FileStore] Excepción NO controlada al enviar {Nombre}", nombreArchivo);
                _logger?.LogError("❌ [FileStore] StackTrace: {StackTrace}", ex.StackTrace);
                return null;
            }
            finally
            {
                httpClient?.Dispose();
            }
        }

        // ═══════════════════════════════════════════════════════════
        // DTOs (AGREGAR AL FINAL DE LA CLASE)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// DTO para solicitud de guardar archivos grandes
        /// </summary>
        public class GuardarArchivosRequest
        {
            public List<ArchivoDto> Archivos { get; set; } = new();
        }

        /// <summary>
        /// DTO para representar un archivo en Base64
        /// </summary>
        public class ArchivoDto
        {
            public string ArchivoBase64 { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }

        /// <summary>
        /// DTO para respuesta con enlaces de descarga
        /// </summary>
        public class EnlaceArchivoDto
        {
            public string Nombre { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
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

        // ✅ AGREGAR AL FINAL DE LA CLASE (después de los otros DTOs)

        /// <summary>
        /// DTO para solicitud de envío de email con archivos adjuntos
        /// </summary>
        public class EnviarEmailRequest
        {
            /// <summary>
            /// Lista de archivos a adjuntar (en base64)
            /// </summary>
            public List<ArchivoSendDto> Archivos { get; set; } = new();

            /// <summary>
            /// Destinatario(s) del email (separados por ; o , si son múltiples)
            /// </summary>
            public string EmailTo { get; set; } = string.Empty;

            /// <summary>
            /// Asunto del email
            /// </summary>
            public string EmailSubject { get; set; } = string.Empty;

            /// <summary>
            /// Cuerpo del email (puede contener HTML)
            /// </summary>
            public string EmailBody { get; set; } = string.Empty;
        }

    }

    
}
