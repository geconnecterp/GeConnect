using gc.infraestructura.Dtos.Cajas.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Cajas
{
    /// <summary>
    /// ✅ NUEVO v20.2.1: DTO para recibir valores y uniones de pago desde AJAX
    /// </summary>
    public class PagoCompletoDto
    {
        /// <summary>
        /// Lista de valores de pago (efectivo, cheques, transferencias, etc.)
        /// </summary>
        public List<Json_Valor> Valores { get; set; } = new List<Json_Valor>();

        /// <summary>
        /// Lista de uniones entre facturas (para aplicar créditos, saldos a favor, etc.)
        /// </summary>
        public List<Json_Union> Uniones { get; set; } = new List<Json_Union>();
    }
}
