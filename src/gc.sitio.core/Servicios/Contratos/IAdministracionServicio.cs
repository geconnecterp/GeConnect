using gc.infraestructura.Dtos.Administracion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IAdministracionServicio:IServicio<AdministracionDto>
    {
        List<AdministracionLoginDto> GetAdministracionLogin();
    }
}
