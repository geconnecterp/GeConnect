using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.ABM
{
    public class ABMZonaDto:ZonaDto
    {
        public int Total_Registros { get; set; }
        public int Total_Paginas { get; set; }
    }   
}
