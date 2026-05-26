using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    /// <summary>
    /// DTO para guardar múltiples cheques
    /// FASE 3: Permite carga iterativa de cheques en una sola operación
    /// </summary>
    public class ChequesReqDto
    {
        /// <summary>
        /// Lista de cheques a guardar
        /// </summary>
        [Required(ErrorMessage = "Debe ingresar al menos un cheque")]
        [MinLength(1, ErrorMessage = "Debe ingresar al menos un cheque")]
        public List<ChequeItemDto> Cheques { get; set; } = new List<ChequeItemDto>();

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
        /// ID del instrumento (tipo de cheque)
        /// </summary>
        [Required(ErrorMessage = "El instrumento es obligatorio")]
        public string InsId { get; set; } = string.Empty;

        /// <summary>
        /// Días máximos de vencimiento permitidos (desde SP inicial)
        /// </summary>
        public int? DiasMaximosVencimiento { get; set; }
    }
}