using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    /// <summary>
    /// DTO para guardar transferencia bancaria
    /// FASE 2: Requiere validaciones de fecha y texto con formato específico
    /// </summary>
    public class TransferenciaReqDto
    {
        /// <summary>
        /// ID del banco destino de la transferencia
        /// </summary>
        [Required(ErrorMessage = "El banco es obligatorio")]
        public string BancoId { get; set; } = string.Empty;

        /// <summary>
        /// Número de transferencia
        /// Restricción: Numérico, mínimo 15 caracteres, relleno con ceros a la izquierda
        /// Mapeo: rb_dato3_valor
        /// </summary>
        [Required(ErrorMessage = "El número de transferencia es obligatorio")]
        [MinLength(15, ErrorMessage = "El número de transferencia debe tener al menos 15 caracteres")]
        [RegularExpression(@"^\d{15,}$", ErrorMessage = "El número de transferencia debe ser numérico")]
        public string NumeroTransferencia { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de la transferencia
        /// Restricción: Debe ser menor o igual al día de hoy, y no más antigua que un día atrás
        /// Mapeo: rb_fecha_valor
        /// </summary>
        [Required(ErrorMessage = "La fecha de transferencia es obligatoria")]
        public DateTime FechaTransferencia { get; set; }

        /// <summary>
        /// Monto de la transferencia
        /// Restricción: > 0 y <= Saldo a cancelar
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
        /// ID del instrumento (cuenta bancaria)
        /// </summary>
        [Required(ErrorMessage = "El instrumento es obligatorio")]
        public string InsId { get; set; } = string.Empty;
    }
}