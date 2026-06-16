using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class FactPendienteRequestDto
    {
        public string? caja_nro_proceso { get; set; }
        public string? caja_nro_cierre { get; set; }
        public string cta_id { get; set; } = string.Empty;
        public string tdoc_id { get; set; } = string.Empty;
        public string cta_documento { get; set; } = string.Empty;
        public string carga { get; set; } = string.Empty;  //para Cobranza diferida tiene que ir "T"
    }
}
