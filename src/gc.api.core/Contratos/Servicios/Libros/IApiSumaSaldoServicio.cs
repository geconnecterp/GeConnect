using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Contratos.Servicios.Libros
{
    public interface IApiSumaSaldoServicio:IServicio<EntidadBase>
    {
        List<BSumaSaldoRegDto> ObtenerBalanceSumaSaldos(LibroFiltroDto req);
    }
}
