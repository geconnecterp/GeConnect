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
        public PrintPackageOptions PrintPackage { get; set; } = new();
        public List<AppModulo> Modulos { get; set; } = new();
    }

    /// <summary>
    /// Límites operativos para la generación de paquetes de impresión.
    /// Los valores se pueden ajustar sin modificar los generadores individuales.
    /// </summary>
    public class PrintPackageOptions
    {
        public int MaxDocumentos { get; set; } = 8;
        public int MaxPaginas { get; set; } = 300;
        public int MaxTamanoMb { get; set; } = 50;
        public int TimeoutMinutos { get; set; } = 5;
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
        /// <summary>
        /// Permite incluir el reporte en un PDF consolidado con otros reportes.
        /// Es true por defecto para mantener compatibilidad con la configuración existente.
        /// </summary>
        public bool PermiteImpresionGlobal { get; set; } = true;

        /// <summary>
        /// Permite crear un enlace anónimo y temporal para este reporte.
        /// Se mantiene en true por compatibilidad; los documentos sensibles
        /// deben declararse explícitamente en false en la configuración.
        /// </summary>
        public bool PermiteEnlacePublico { get; set; } = true;

        /// <summary>
        /// Identifica reportes que, si se habilitan para enlaces, requieren
        /// auditoría reforzada en la capa de persistencia.
        /// </summary>
        public bool RequiereAuditoriaEnlace { get; set; }

        public bool ImprimeDuplicado { get; set; }
        public bool ImprimeSoloDuplicado { get; set; }
    }
}
