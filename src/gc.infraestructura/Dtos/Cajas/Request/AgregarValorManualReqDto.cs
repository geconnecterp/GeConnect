using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    /// <summary>
    /// DTO para agregar un valor manual al checkout
    /// Método genérico para valores no estándar
    /// </summary>
    public class AgregarValorManualReqDto
    {
        /// <summary>
        /// Descripción del valor manual
        /// </summary>
        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Monto del valor manual
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
        /// Observaciones adicionales
        /// </summary>
        public string? Observaciones { get; set; }
    }
}
