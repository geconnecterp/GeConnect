using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    /// <summary>
    /// DTO para guardar crédito impositivo (retenciones/certificados)
    /// FASE 2: Requiere validaciones de fecha y autocompletado de datos del cliente
    /// </summary>
    public class CreditoImpositivoReqDto
    {
        /// <summary>
        /// Número de certificado
        /// Restricción: Alfanumérico, estrictamente mayor a 3 caracteres
        /// Mapeo: rb_dato1_valor
        /// </summary>
        [Required(ErrorMessage = "El número de certificado es obligatorio")]
        [MinLength(4, ErrorMessage = "El número de certificado debe tener más de 3 caracteres")]
        public string NumeroCertificado { get; set; } = string.Empty;

        /// <summary>
        /// CUIT del cliente
        /// Restricción: Se autocompleta desde datos del cliente en sesión
        /// Mapeo: rb_dato2_valor
        /// </summary>
        [Required(ErrorMessage = "El CUIT es obligatorio")]
        [MinLength(11, ErrorMessage = "El CUIT debe tener al menos 11 caracteres")]
        public string Cuit { get; set; } = string.Empty;

        /// <summary>
        /// Razón Social del cliente
        /// Restricción: Se autocompleta desde datos del cliente en sesión
        /// Mapeo: rb_dato3_valor
        /// </summary>
        [Required(ErrorMessage = "La razón social es obligatoria")]
        public string RazonSocial { get; set; } = string.Empty;

        /// <summary>
        /// Fecha del certificado
        /// Restricción: Debe ser menor o igual a la fecha actual, antigüedad máxima 15 días
        /// Mapeo: rb_fecha_valor
        /// </summary>
        [Required(ErrorMessage = "La fecha del certificado es obligatoria")]
        public DateTime FechaCertificado { get; set; }

        /// <summary>
        /// Monto del crédito impositivo
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
        /// ID del instrumento
        /// </summary>
        [Required(ErrorMessage = "El instrumento es obligatorio")]
        public string InsId { get; set; } = string.Empty;
    }
}