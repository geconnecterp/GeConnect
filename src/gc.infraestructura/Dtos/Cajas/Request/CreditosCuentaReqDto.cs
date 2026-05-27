using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    /// <summary>
    /// DTO para aplicar créditos en cuenta (Notas de Crédito)
    /// FASE 4: Método automático con autorización gerencial condicional
    /// </summary>
    public class CreditosCuentaReqDto
    {
        /// <summary>
        /// Lista de créditos a aplicar
        /// Los valores van a @json_union, no a @json_valores
        /// </summary>
        [Required(ErrorMessage = "Debe seleccionar al menos un crédito")]
        [MinLength(1, ErrorMessage = "Debe seleccionar al menos un crédito")]
        public List<CreditoCuentaItemDto> Creditos { get; set; } = new List<CreditoCuentaItemDto>();

        /// <summary>
        /// ID de la cuenta del cliente
        /// </summary>
        [Required(ErrorMessage = "El ID de cuenta es obligatorio")]
        public string CuentaId { get; set; } = string.Empty;

        /// <summary>
        /// Origen del cliente (F = Consumidor Final)
        /// Si origen = 'F', requiere autorización gerencial
        /// </summary>
        public string? OrigenCliente { get; set; }

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
        /// Usuario autorizante (solo si origen = 'F')
        /// </summary>
        public string? UsuarioAutorizante { get; set; }

        /// <summary>
        /// Indica si la autorización gerencial fue aprobada
        /// </summary>
        public bool? AutorizacionAprobada { get; set; }
    }
}