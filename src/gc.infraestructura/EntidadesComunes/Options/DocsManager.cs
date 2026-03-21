using System.Collections.Generic;

namespace gc.infraestructura.EntidadesComunes.Options
{
    /// <summary>
    /// Configuración del gestor de documentos
    /// </summary>
    public class DocsManager
    {
        public string ApiReporteUrl { get; set; } = string.Empty;
        public string ApiLink { get; set; } = string.Empty;
        public string Crear { get; set; } = string.Empty;
        public string Obtener { get; set; } = string.Empty;
        public List<AppModulo> Modulos { get; set; } = new();
    }

    /// <summary>
    /// Módulo de documentación
    /// </summary>
    public class AppModulo
    {
        public string Id { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public bool Print { get; set; }
        public bool Export { get; set; }
        public bool Email { get; set; }
        public bool Whatsapp { get; set; }
        
        /// <summary>
        /// ✅ Configuración de mensajería (común para Email y WhatsApp)
        /// </summary>
        public MensajeriaTemplate? MensajeriaTemplate { get; set; }
        
        /// <summary>
        /// Configuración específica de Email (solo asunto)
        /// </summary>
        public EmailTemplate? EmailTemplate { get; set; }
        
        /// <summary>
        /// ✅ (opcional futuro): Configuración específica de WhatsApp
        /// </summary>
        public WhatsAppTemplate? WhatsAppTemplate { get; set; }
        
        public List<AppReporte> Reportes { get; set; } = new();
    }

    /// <summary>
    /// ✅ Configuración de mensajería (común para Email y WhatsApp)
    /// </summary>
    public class MensajeriaTemplate
    {
        /// <summary>
        /// Plantilla del mensaje (usada en Email y WhatsApp)
        /// </summary>
        public string MensajeTemplate { get; set; } = string.Empty;
        
        /// <summary>
        /// Tipo de destinatario: "Cliente", "Proveedor", "Ambos", "Interno"
        /// </summary>
        public string TipoDestinatario { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica si el mensaje debe personalizarse con nombre
        /// </summary>
        public bool EsPersonalizado { get; set; } = false;
        
        /// <summary>
        /// Saludo genérico cuando no hay nombre disponible
        /// </summary>
        public string SaludoGenerico { get; set; } = "Estimado/a";
    }

    /// <summary>
    /// Configuración específica de Email (solo campos exclusivos de email)
    /// </summary>
    public class EmailTemplate
    {
        /// <summary>
        /// Plantilla del asunto (solo para Email)
        /// </summary>
        public string AsuntoTemplate { get; set; } = string.Empty;
    }

    /// <summary>
    /// ✅ (opcional futuro): Configuración específica de WhatsApp
    /// </summary>
    public class WhatsAppTemplate
    {
        /// <summary>
        /// Prefijo opcional para mensajes de WhatsApp (ej: 🔔 Notificación:)
        /// </summary>
        public string? PrefijoMensaje { get; set; }
        
        /// <summary>
        /// Indica si se deben usar emojis en WhatsApp
        /// </summary>
        public bool UsarEmojis { get; set; } = true;
    }

    /// <summary>
    /// Reporte individual
    /// </summary>
    public class AppReporte
    {
        public List<string> Titulos { get; set; } = new();
        public int Id { get; set; }
        public bool ImprimeDuplicado { get; set; }
        public bool ImprimeSoloDuplicado { get; set; }
    }
}
