using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Importacion;

namespace gc.sitio.core.Servicios.Contratos.Importacion
{
    public interface IImportarServicio
    {
       Task<RespuestaGenerica<PrecioFileDatos>> ObtenerPrecioFileDatos(string token);
    }
}
