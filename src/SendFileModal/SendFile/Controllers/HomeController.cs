using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SendFile.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SendFile.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Configuración SMTP hardcoded para laboratorio - SOLO GMAIL
        private static readonly Dictionary<string, EmailProviderConfig> EmailProviders = new()
        {
            ["gmail"] = new EmailProviderConfig
            {
                ProviderName = "Gmail",
                Email = "j2b3d.sys@gmail.com",
                Password = "yhnd bkml dfce bogv",
                SmtpServer = "smtp.gmail.com",
                Port = 587,
                UseSsl = true,
                UseStartTls = true
            }
        };

        // Configuración de Twilio para WhatsApp
        private const string TwilioAccountSid = "TU_ACCOUNT_SID"; // Obtener de twilio.com/console
        private const string TwilioAuthToken = "TU_AUTH_TOKEN";
        private const string TwilioWhatsAppNumber = "whatsapp:+14155238886"; // Número sandbox de Twilio

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail([FromForm] SendEmailRequest request, IFormFile? emailFile)
        {
            try
            {
                // Validar que el proveedor exista
                if (!EmailProviders.ContainsKey(request.Provider.ToLower()))
                {
                    return Json(new { success = false, message = "Proveedor de correo no válido" });
                }

                // Obtener configuración del proveedor
                var providerConfig = EmailProviders[request.Provider.ToLower()];

                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Subject))
                {
                    return Json(new { success = false, message = "El destinatario y el asunto son obligatorios" });
                }

                // Crear el mensaje de email
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(providerConfig.ProviderName, providerConfig.Email));
                emailMessage.To.Add(MailboxAddress.Parse(request.To));
                emailMessage.Subject = request.Subject;

                // Crear el cuerpo del mensaje
                var bodyBuilder = new BodyBuilder
                {
                    TextBody = request.Message ?? string.Empty
                };

                // Adjuntar archivo si existe
                if (emailFile != null && emailFile.Length > 0)
                {
                    // Validar tamaño (25 MB máximo)
                    if (emailFile.Length > 25 * 1024 * 1024)
                    {
                        return Json(new { success = false, message = "El archivo excede el tamaño máximo de 25 MB" });
                    }

                    using var memoryStream = new MemoryStream();
                    await emailFile.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    bodyBuilder.Attachments.Add(emailFile.FileName, memoryStream.ToArray());
                }

                emailMessage.Body = bodyBuilder.ToMessageBody();

                // Enviar el email
                using var smtpClient = new SmtpClient();
                
                try
                {
                    // Conectar al servidor SMTP
                    await smtpClient.ConnectAsync(
                        providerConfig.SmtpServer,
                        providerConfig.Port,
                        providerConfig.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto
                    );

                    // Autenticar
                    await smtpClient.AuthenticateAsync(providerConfig.Email, providerConfig.Password);

                    // Enviar mensaje
                    await smtpClient.SendAsync(emailMessage);

                    // Desconectar
                    await smtpClient.DisconnectAsync(true);

                    _logger.LogInformation(
                        "Email enviado exitosamente vía {Provider} a {To}",
                        providerConfig.ProviderName,
                        request.To
                    );

                    return Json(new
                    {
                        success = true,
                        message = $"Email enviado exitosamente vía {providerConfig.ProviderName}",
                        provider = providerConfig.ProviderName,
                        to = request.To
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar email vía {Provider}", providerConfig.ProviderName);
                    return Json(new
                    {
                        success = false,
                        message = $"Error al conectar con {providerConfig.ProviderName}: {ex.Message}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general al procesar solicitud de envío de email");
                return Json(new { success = false, message = $"Error al procesar la solicitud: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendWhatsApp([FromForm] SendWhatsAppRequest request, IFormFile? whatsappFile)
        {
            try
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Message))
                {
                    return Json(new { success = false, message = "El número de teléfono y el mensaje son obligatorios" });
                }

                // Validar formato de número (debe incluir código de país)
                if (!request.To.StartsWith("+"))
                {
                    return Json(new { success = false, message = "El número debe incluir el código de país (ej: +521234567890)" });
                }

                // Inicializar cliente de Twilio
                TwilioClient.Init(TwilioAccountSid, TwilioAuthToken);

                MessageResource message;

                // Si hay archivo adjunto
                if (whatsappFile != null && whatsappFile.Length > 0)
                {
                    // Validar tamaño (16 MB máximo para WhatsApp)
                    if (whatsappFile.Length > 16 * 1024 * 1024)
                    {
                        return Json(new { success = false, message = "El archivo excede el tamaño máximo de 16 MB para WhatsApp" });
                    }

                    // Nota: Para archivos necesitas hospedarlos en una URL pública
                    // Por ahora solo enviamos texto
                    return Json(new 
                    { 
                        success = false, 
                        message = "El envío de archivos por WhatsApp requiere una URL pública. Implementación pendiente." 
                    });
                }
                else
                {
                    // Enviar mensaje de texto
                    message = await MessageResource.CreateAsync(
                        from: new PhoneNumber(TwilioWhatsAppNumber),
                        to: new PhoneNumber($"whatsapp:{request.To}"),
                        body: request.Message
                    );
                }

                _logger.LogInformation(
                    "WhatsApp enviado exitosamente a {To}. SID: {MessageSid}",
                    request.To,
                    message.Sid
                );

                return Json(new
                {
                    success = true,
                    message = "Mensaje de WhatsApp enviado exitosamente",
                    to = request.To,
                    messageSid = message.Sid,
                    status = message.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar WhatsApp");
                return Json(new 
                { 
                    success = false, 
                    message = $"Error al enviar WhatsApp: {ex.Message}" 
                });
            }
        }

        // ============ NUEVO MÉTODO: Outlook Web Deeplink ============
        [HttpPost]
        public IActionResult GenerateOutlookWebLink([FromBody] SendEmailRequest request)
        {
            try
            {
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
                if (!string.IsNullOrWhiteSpace(request.Message))
                {
                    urlBuilder.Append($"&body={Uri.EscapeDataString(request.Message)}");
                }

                var outlookWebUrl = urlBuilder.ToString();

                _logger.LogInformation(
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
                _logger.LogError(ex, "Error al generar enlace de Outlook Web");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        // ============ FIN NUEVO MÉTODO ============

        // ============ NUEVO MÉTODO: Enlace mailto ============
        [HttpPost]
        public IActionResult GenerateMailtoLink([FromBody] SendEmailRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.To))
                {
                    return Json(new { success = false, message = "El destinatario es obligatorio" });
                }

                // Construir enlace mailto con CC/BCC
                var mailtoBuilder = new MailtoLinkBuilder
                {
                    To = request.To,
                    Cc = request.Cc,
                    Bcc = request.Bcc,
                    Subject = request.Subject,
                    Body = request.Message ?? string.Empty
                };

                var mailtoLink = mailtoBuilder.Build();

                _logger.LogInformation(
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
                    note = "⚠️ Debes seleccionar manualmente la cuenta remitente (juanjobe@msn.com o tu@cafeamerica.com.ar) y adjuntar archivos manualmente."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar enlace mailto");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        // ============ FIN NUEVO MÉTODO ============

        [HttpPost]
        public IActionResult GenerateWhatsAppWebLink([FromBody] SendWhatsAppRequest request)
        {
            try
            {
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
                        message = "El número debe incluir el código de país (ej: +5491123456789)"
                    });
                }

                // Remover el símbolo + para la URL de WhatsApp
                var numberForUrl = cleanNumber.TrimStart('+');

                // Validar longitud del mensaje (recomendamos máximo 5000 caracteres)
                if (request.Message.Length > 5000)
                {
                    return Json(new
                    {
                        success = false,
                        message = "El mensaje es muy largo. Recomendamos máximo 5000 caracteres."
                    });
                }

                // Construir URL de WhatsApp Web
                var whatsappWebUrl = $"https://wa.me/{numberForUrl}?text={Uri.EscapeDataString(request.Message)}";

                _logger.LogInformation(
                    "Enlace de WhatsApp Web generado para {To}",
                    cleanNumber
                );

                return Json(new
                {
                    success = true,
                    message = "Se abrirá WhatsApp Web en una nueva pestaña",
                    whatsappWebLink = whatsappWebUrl,
                    provider = "WhatsApp Web",
                    to = cleanNumber,
                    note = "⚠️ Debes confirmar el envío en WhatsApp. Los archivos deben adjuntarse manualmente."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar enlace de WhatsApp Web");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Clase interna para configuración de proveedores de email
        private class EmailProviderConfig
        {
            public string ProviderName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string SmtpServer { get; set; } = string.Empty;
            public int Port { get; set; }
            public bool UseSsl { get; set; }
            public bool UseStartTls { get; set; }
        }
    }
}
