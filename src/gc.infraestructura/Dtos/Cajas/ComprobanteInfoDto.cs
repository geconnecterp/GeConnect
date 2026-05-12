using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Cajas
{
    /// <summary>
    /// ✅ NUEVO v10.0: DTO para deserializar información de comprobante
    /// Representa el formato JSON retornado en resultado_id por los SPs de diferir pago
    /// 
    /// Formato esperado:
    /// [{"tco_letra":"B","tco_id":"006","cm_compte":"0001-00000008","cm_repetido":"0"}]
    /// </summary>
    public class ComprobanteInfoDto
    {
        /// <summary>
        /// Letra del tipo de comprobante (A, B, C, etc.)
        /// </summary>
        public string tco_letra { get; set; } = string.Empty;

        /// <summary>
        /// ID del tipo de comprobante (006, 007, etc.)
        /// </summary>
        public string tco_id { get; set; } = string.Empty;

        /// <summary>
        /// Número de comprobante (formato: XXXX-XXXXXXXX)
        /// </summary>
        public string cm_compte { get; set; } = string.Empty;

        /// <summary>
        /// Indicador de comprobante repetido ("0" = no, "1" = sí)
        /// </summary>
        public string cm_repetido { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el comprobante es repetido (conversión a booleano)
        /// </summary>
        public bool EsRepetido => cm_repetido == "1";
    }
}
