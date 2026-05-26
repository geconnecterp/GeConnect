using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    /// <summary>
    /// DTO para guardar valores directos (Efectivo o Vales de Compra)
    /// FASE 1: Método más simple, solo requiere monto
    /// </summary>
    public class ValorDirectoReqDto
    {
        /// <summary>
        /// ID del tipo de cuenta financiera (EF = Efectivo, VA = Vales)
        /// </summary>
        [Required(ErrorMessage = "El tipo de cuenta financiera es obligatorio")]
        public string TcfId { get; set; } = string.Empty;

        /// <summary>
        /// ID del instrumento seleccionado
        /// </summary>
        [Required(ErrorMessage = "El instrumento es obligatorio")]
        public string InsId { get; set; } = string.Empty;

        /// <summary>
        /// Monto del pago
        /// Restricción: Debe ser mayor a 0 y no puede superar el saldo a cancelar
        /// </summary>
        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        public decimal Monto { get; set; }

        /// <summary>
        /// ID de la cuenta del cliente
        /// </summary>
        [Required(ErrorMessage = "El ID de cuenta es obligatorio")]
        public string CuentaId { get; set; } = string.Empty;

        /// <summary>
        /// ID de la administración
        /// </summary>
        [Required(ErrorMessage = "El ID de administración es obligatorio")]
        public string AdmId { get; set; } = string.Empty;

        /// <summary>
        /// ID de la caja activa
        /// </summary>
        [Required(ErrorMessage = "El ID de caja es obligatorio")]
        public string CajaId { get; set; } = string.Empty;

        /// <summary>
        /// Saldo pendiente a cancelar (para validación)
        /// </summary>
        public decimal? SaldoPendiente { get; set; }
    }
}