using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Almacen.Rpr
{
    public class RprABRequest
    {
        public string UL { get; set; } = string.Empty;
        public string Box { get; set; } = string.Empty;
        public string AdmId { get; set; } = string.Empty;
    }
}
