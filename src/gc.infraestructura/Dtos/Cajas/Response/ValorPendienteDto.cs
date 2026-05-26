namespace gc.infraestructura.Dtos.Cajas.Response
{
    /// <summary>
    /// DTO de respuesta con un valor pendiente de pago
    /// </summary>
    public class ValorPendienteDto
    {
        /// <summary>
        /// ID del comprobante
        /// </summary>
        public string CompteId { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de comprobante
        /// </summary>
        public string TipoCompte { get; set; } = string.Empty;

        /// <summary>
        /// Número de comprobante
        /// </summary>
        public string NumeroCompte { get; set; } = string.Empty;

        /// <summary>
        /// Fecha del comprobante
        /// </summary>
        public DateTime? FechaCompte { get; set; }

        /// <summary>
        /// Fecha de vencimiento
        /// </summary>
        public DateTime? FechaVencimiento { get; set; }

        /// <summary>
        /// Importe total del comprobante
        /// </summary>
        public decimal ImporteTotal { get; set; }

        /// <summary>
        /// Importe pendiente de pago
        /// </summary>
        public decimal ImportePendiente { get; set; }

        /// <summary>
        /// Descripción adicional
        /// </summary>
        public string? Descripcion { get; set; }
    }
}