using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    /// <summary>
    /// DTO para guardar cupón/orden de empresa (Mutuales)
    /// FASE 3: Requiere selección de mutual antes de detallar comprobante
    /// </summary>
    public class CuponEmpresaReqDto
    {
        /// <summary>
        /// ID de la mutual/empresa emisora
        /// </summary>
        [Required(ErrorMessage = "La mutual/empresa es obligatoria")]
        public string MutualId { get; set; } = string.Empty;

        /// <summary>
        /// Titular del cupón/orden
        /// Restricción: Alfanumérico, mayor a 5 caracteres (autocompleta con nombre del cliente)
        /// Mapeo: rb_dato1_valor
        /// </summary>
        [Required(ErrorMessage = "El titular es obligatorio")]
        [MinLength(6, ErrorMessage = "El titular debe tener más de 5 caracteres")]
        public string Titular { get; set; } = string.Empty;

        /// <summary>
        /// Número de orden/cupón
        /// Restricción: Numérico, máximo 10 caracteres, rellenado con ceros a la izquierda
        /// Mapeo: rb_dato2_valor
        /// </summary>
        [Required(ErrorMessage = "El número de orden es obligatorio")]
        [MaxLength(10, ErrorMessage = "El número de orden debe tener máximo 10 caracteres")]
        [RegularExpression(@"^\d{1,10}$", ErrorMessage = "El número de orden debe ser numérico")]
        public string NumeroOrden { get; set; } = string.Empty;

        /// <summary>
        /// CUIT del titular
        /// Restricción: Valida formato estándar de CUIT
        /// Mapeo: rb_dato3_valor
        /// </summary>
        [Required(ErrorMessage = "El CUIT es obligatorio")]
        [MinLength(11, ErrorMessage = "El CUIT debe tener formato válido")]
        public string Cuit { get; set; } = string.Empty;

        /// <summary>
        /// Monto del cupón/orden
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
        /// ID del instrumento (cuenta de la mutual)
        /// </summary>
        [Required(ErrorMessage = "El instrumento es obligatorio")]
        public string InsId { get; set; } = string.Empty;
    }
}