using gc.infraestructura.Dtos.DocManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Contratos.DocManager
{
    public interface IDocManagerServicio
    {
        string GenerarArchivoPDF<T>(PrintRequestDto<T> request);
        string GenerarDocumento<T>(PrintRequestDto<T> request);
    }
}
