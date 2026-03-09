using gc.api.core.Entidades;
using gc.infraestructura.Dtos.OrdenReparto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Contratos.Servicios
{
    public interface IOrdenRepartoServicio:IServicio<EntidadBase>
    {
        List<OrdenRepartoListDto> ObtenerOrdenesReparto(ORRequestDto request);
         OrdenRepartoDto ObtenerOrdenRepartoPorId(int id);
    }
}
