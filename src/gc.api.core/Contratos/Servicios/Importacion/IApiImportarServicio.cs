using gc.infraestructura.Dtos.Importacion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Contratos.Servicios.Importacion
{
    public interface IApiImportarServicio
    {
        List<PrecioFileDatos> ObtenerPrecioFileDatos();
    }
}
