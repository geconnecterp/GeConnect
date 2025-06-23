using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Libros
{
    public class LDiarioRequest
    {
        public int Eje_nro { get; set; }
        public bool Periodo { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public bool RangoFC { get; set; }
        public DateTime DesdeFC { get; set; }
        public DateTime HastaFC { get; set; }
        public string Movimientos { get; set; } = string.Empty;
        public bool ConTemporales { get; set; }
        public int Regs { get; set; }
        public int Pagina { get; set; }
        public string Orden { get; set; } = string.Empty;
    }
}
