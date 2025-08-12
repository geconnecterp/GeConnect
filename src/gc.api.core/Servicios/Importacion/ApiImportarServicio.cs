using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Importacion;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Importacion;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.Importacion
{
    public class ApiImportarServicio : Servicio<EntidadBase>, IApiImportarServicio
    {
        public ApiImportarServicio(IUnitOfWork uow) : base(uow)
        {
            
        }
        public List<PrecioFileDatos> ObtenerPrecioFileDatos()
        {
            var sp = ConstantesGC.StoredProcedures.SP_PROD_PRECIO_FILE_DATOS;
            var ps = new List<SqlParameter>();
            List<PrecioFileDatos> resp = _repository.EjecutarLstSpExt<PrecioFileDatos>(sp, ps, true);
            return resp;
        }
    }
}
