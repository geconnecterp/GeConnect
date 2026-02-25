namespace SendFile.Models
{
    public class SendEmailRequest
    {
        public string Provider { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string? Cc { get; set; } // Con Copia
        public string? Bcc { get; set; } // Copia Oculta
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // Nueva clase para WhatsApp
    public class SendWhatsAppRequest
    {
        public string To { get; set; } = string.Empty; // Formato: +52XXXXXXXXXX
        public string Message { get; set; } = string.Empty;
    }

    // Clase helper para generar enlaces mailto con soporte CC/BCC
    public class MailtoLinkBuilder
    {
        public string To { get; set; } = string.Empty;
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public string Build()
        {
            var parameters = new List<string>();

            if (!string.IsNullOrWhiteSpace(Cc))
                parameters.Add($"cc={Uri.EscapeDataString(Cc)}");

            if (!string.IsNullOrWhiteSpace(Bcc))
                parameters.Add($"bcc={Uri.EscapeDataString(Bcc)}");

            if (!string.IsNullOrWhiteSpace(Subject))
                parameters.Add($"subject={Uri.EscapeDataString(Subject)}");

            if (!string.IsNullOrWhiteSpace(Body))
                parameters.Add($"body={Uri.EscapeDataString(Body)}");

            var queryString = parameters.Count > 0 ? "?" + string.Join("&", parameters) : string.Empty;
            return $"mailto:{Uri.EscapeDataString(To)}{queryString}";
        }
    }
}