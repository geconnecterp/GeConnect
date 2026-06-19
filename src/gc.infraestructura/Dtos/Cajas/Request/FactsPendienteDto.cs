using gc.infraestructura.Dtos.Cajas.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Dtos.Cajas.Request
{
    public class FactsPendienteDto
    {
       public List<FactPendienteResponseDto> Facturas { get; set; } = new();
    }
}
