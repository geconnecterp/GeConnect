using gc.infraestructura.Dtos.Seguridad;
using System.ComponentModel.DataAnnotations;

namespace gc.sitio.Models.Configuracion
{
    public class CambioClaveViewModel
    {
        [Required]
        [StringLength(128)]
        public string ClaveActual { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        public string ClaveNueva { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        [Compare(nameof(ClaveNueva))]
        public string ConfirmacionClave { get; set; } = string.Empty;

        public PoliticaClaveDto Politica { get; set; } = new();
    }

    public class CambioClaveObligatoriaViewModel
    {
        [Required]
        [StringLength(128)]
        public string ClaveNueva { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        [Compare(nameof(ClaveNueva))]
        public string ConfirmacionClave { get; set; } = string.Empty;

        public PoliticaClaveDto Politica { get; set; } = new();
        public string Motivo { get; set; } = "BLANQUEO";
    }
}
