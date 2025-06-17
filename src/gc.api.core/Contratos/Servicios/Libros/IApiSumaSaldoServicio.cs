using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Libros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Contratos.Servicios.Libros
{
    public interface IApiSumaSaldoServicio:IServicio<EntidadBase>
    {
        List<BSumaSaldoRegDto> ObtenerBalanceSumaSaldos(BSSRequestDto req);
    }
}
