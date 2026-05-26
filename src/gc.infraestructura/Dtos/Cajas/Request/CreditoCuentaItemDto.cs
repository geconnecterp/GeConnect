using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    /// <summary>
    /// DTO para un ítem de crédito en cuenta (Nota de Crédito)
    /// FASE 4: Método automático con reglas de negocio especiales
    /// </summary>
    public class CreditoCuentaItemDto
    {
        /// <summary>
        /// ID del comprobante (Nota de Crédito, Recibo, etc.)
        /// </summary>
        [Required(ErrorMessage = "El ID del comprobante es obligatorio")]
        public string ComprobanteId { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de comprobante
        /// </summary>
        public string? TipoComprobante { get; set; }

        /// <summary>
        /// Número de comprobante
        /// </summary>
        public string? NumeroComprobante { get; set; }

        /// <summary>
        /// Fecha del comprobante
        /// </summary>
        public DateTime? FechaComprobante { get; set; }

        /// <summary>
        /// Crédito disponible total
        /// </summary>
        [Required(ErrorMessage = "El crédito disponible es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El crédito disponible debe ser mayor a cero")]
        public decimal CreditoDisponible { get; set; }

        /// <summary>
        /// Monto a imputar de este crédito
        /// Restricción: Debe ser <= CreditoDisponible y <= Saldo a cancelar
        /// </summary>
        [Required(ErrorMessage = "El monto imputado es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto imputado debe ser mayor a cero")]
        public decimal MontoImputado { get; set; }

        /// <summary>
        /// Indica si la carga es obligatoria (no se puede deseleccionar)
        /// Bandera del SP: carga_obligatoria = 'S'
        /// </summary>
        public bool CargaObligatoria { get; set; }

        /// <summary>
        /// Fecha de vencimiento del crédito
        /// </summary>
        public DateTime? FechaVencimiento { get; set; }
    }
}