using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IEtiquetaServicio
    {
        Task<RespuestaGenerica<CargaPreviaDto>> ObtenerCargaPrevia(string adm_id, string token);
    }
}
