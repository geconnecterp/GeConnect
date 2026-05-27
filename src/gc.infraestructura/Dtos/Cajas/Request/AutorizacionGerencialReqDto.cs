using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    /// <summary>
    /// DTO para validar autorización gerencial
    /// Requerido cuando un consumidor final intenta usar créditos a favor
    /// </summary>
    public class AutorizacionGerencialReqDto
    {
        /// <summary>
        /// Usuario autorizante (debe ser Administrador de Cajas)
        /// </summary>
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string Usuario { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario autorizante
        /// </summary>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// ID de la cuenta del cliente que requiere autorización
        /// </summary>
        [Required(ErrorMessage = "El ID de cuenta es obligatorio")]
        public string CuentaId { get; set; } = string.Empty;

        /// <summary>
        /// ID de la administración
        /// </summary>
        [Required(ErrorMessage = "El ID de administración es obligatorio")]
        public string AdmId { get; set; } = string.Empty;

        /// <summary>
        /// Motivo de la solicitud de autorización
        /// </summary>
        public string? Motivo { get; set; }
    }
}