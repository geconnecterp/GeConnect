using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenReparto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IORServicio
    {
        Task<RespuestaGenerica<OrdenRepartoListDto>> ObtenerOrdenesReparto(ORRequestDto request, string token);
    }
}
