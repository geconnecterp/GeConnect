namespace gc.infraestructura.Dtos.Cajas.Response
{
    /// <summary>
    /// DTO de respuesta con un valor de Nota de Crédito disponible
    /// FASE 4: Pre-carga automática desde SP
    /// </summary>
    public class ValorNCDto
    {
        /// <summary>
        /// ID del comprobante (Nota de Crédito, Recibo, etc.)
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
        /// Importe total del crédito
        /// </summary>
        public decimal ImporteTotal { get; set; }

        /// <summary>
        /// Importe disponible para aplicar
        /// </summary>
        public decimal ImporteDisponible { get; set; }

        /// <summary>
        /// Bandera de carga obligatoria (del SP)
        /// Si = 'S', no se puede deseleccionar
        /// </summary>
        public bool CargaObligatoria { get; set; }

        /// <summary>
        /// Descripción adicional
        /// </summary>
        public string? Descripcion { get; set; }
    }
}