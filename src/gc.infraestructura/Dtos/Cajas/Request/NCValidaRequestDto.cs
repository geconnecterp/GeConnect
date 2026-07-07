using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class NCValidaRequestDto
    {
        public string tco_id { get; set; } = string.Empty;  
        public string cm_compte { get; set; } = string.Empty;
        public string caja_nro_proceso { get; set; } = string.Empty;
        public short caja_nro_cierre { get; set; }
    }
}
