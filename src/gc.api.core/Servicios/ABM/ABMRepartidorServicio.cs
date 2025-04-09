using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.ABM;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.ABM
{
    public class ABMRepartidorServicio : Servicio<Vendedor>, IABMRepartidorServicio
    {
        public ABMRepartidorServicio(IUnitOfWork uow) : base(uow)
        {

        }      

        public List<ABMRepartidorDto> ObtenerRepartidores(QueryFilters filters)
        {
            var sp = ConstantesGC.StoredProcedures.SP_ABM_REPARTIDOR_LISTA;

            var ps = new List<SqlParameter>();
             

            List<ABMRepartidorDto> res = _repository.EjecutarLstSpExt<ABMRepartidorDto>(sp, ps, true);
            return res;
        }

        public List<ABMRepartidorDatoDto> ObtenerRepartidorPorId(string rp_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_ABM_REPARTIDOR_DATO;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@rp_id",rp_id) ,

            };

            List<ABMRepartidorDatoDto> res = _repository.EjecutarLstSpExt<ABMRepartidorDatoDto>(sp, ps, true);
            return res;
        }
    }
}
