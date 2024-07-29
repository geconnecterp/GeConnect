using gc.infraestructura.Dtos.Almacen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface ICuentaServicio: IServicio<CuentaDto>
    {
        List<ProveedorListaDto> ObtenerListaProveedores(string token);
    }
}
