using gc.infraestructura.Dtos.Asientos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Contratos.Servicios.Asientos
{
    public interface IAsientoLibroDiarioServicio
    {
        List<AsientoDetalleLDDto> ObtenerAsientoLibroDiario(
            int eje_nro,
            bool periodo,
            DateTime desde,
            DateTime hasta,
            string movimientos,
            bool conTemporales,
            int regs,
            int pag,
            string orden);
    }
}
