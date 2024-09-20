using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios
{
    public class TipoMotivoServicio : Servicio<TipoMotivo>, ITipoMotivoServicio
    {
        public TipoMotivoServicio(IUnitOfWork uow):base(uow)
        {
                
        }
        public List<TipoMotivo> ObtenerTiposMotivo()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_INFOPROD_TM;
            var ps = new List<SqlParameter>();

            List<TipoMotivo> tm = _repository.EjecutarLstSpExt<TipoMotivo>(sp, ps, true);

            return tm;
        }
    }
}
