using gc.infraestructura.Dtos.DocManager;
using gc.sitio.core.Servicios.Contratos.DocManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion.DocManager
{
    public class DocManagerServicio : IDocManagerServicio
    {
        public string GenerarArchivoPDF<T>(PrintRequestDto<T> request)
        {
            throw new NotImplementedException();
        }

        public string GenerarDocumento<T>(PrintRequestDto<T> request)
        {
            throw new NotImplementedException();
        }
    }
}
