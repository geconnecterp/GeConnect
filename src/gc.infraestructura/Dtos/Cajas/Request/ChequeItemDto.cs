using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    /// <summary>
    /// DTO para un ítem de cheque individual
    /// FASE 3: Método más complejo con carga "uno por uno"
    /// </summary>
    public class ChequeItemDto
    {
        /// <summary>
        /// ID del banco emisor
        /// Restricción: Debe existir en la lista de bancos (SP SPGECO_ABM_BCO_CH_Lista)
        /// </summary>
        [Required(ErrorMessage = "El banco es obligatorio")]
        public string BancoId { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del banco
        /// Mapeo: rb_dato1_valor
        /// </summary>
        [Required(ErrorMessage = "La descripción del banco es obligatoria")]
        public string BancoDescripcion { get; set; } = string.Empty;

        /// <summary>
        /// Número de cheque
        /// Restricción: Máximo 8 caracteres numéricos, relleno con ceros a la izquierda
        /// Mapeo: rb_dato2_valor
        /// </summary>
        [Required(ErrorMessage = "El número de cheque es obligatorio")]
        [MaxLength(8, ErrorMessage = "El número de cheque debe tener máximo 8 caracteres")]
        [RegularExpression(@"^\d{1,8}$", ErrorMessage = "El número de cheque debe ser numérico")]
        public string NumeroCheque { get; set; } = string.Empty;

        /// <summary>
        /// Plaza del banco
        /// Restricción: Numérico hasta 6 caracteres, relleno con ceros
        /// Mapeo: rb_dato3_valor
        /// </summary>
        [MaxLength(6, ErrorMessage = "La plaza debe tener máximo 6 caracteres")]
        public string? Plaza { get; set; }

        /// <summary>
        /// Fecha de vencimiento del cheque
        /// Restricción: Mayor o igual a hoy, topeado por días máximos del SP inicial
        /// Mapeo: rb_fecha_valor
        /// </summary>
        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        public DateTime FechaVencimiento { get; set; }

        /// <summary>
        /// Monto del cheque
        /// Restricción: > 0
        /// </summary>
        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        public decimal Monto { get; set; }
    }
}