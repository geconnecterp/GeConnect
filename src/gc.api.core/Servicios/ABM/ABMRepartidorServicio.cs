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

        public List<ABMRepartidorDto> ObtenerRepartidores(QueryFilters filtros)
        {
            var sp = ConstantesGC.StoredProcedures.SP_ABM_REPARTIDOR_LISTA;

            var ps = new List<SqlParameter>();
            //debo cargar aca todos los filtros sobre los parametros a utilizar
            if (!string.IsNullOrEmpty(filtros.Id))
            {
                ps.Add(new SqlParameter("@id", true));
                //hay un id de producto. se habilita la seccion de productos
                ps.Add(new SqlParameter("@id_d", filtros.Id));

                if (!string.IsNullOrEmpty(filtros.Id2))
                {
                    ps.Add(new SqlParameter("@id_h", filtros.Id2));
                }
                else
                {
                    ps.Add(new SqlParameter("@id_h", filtros.Id));
                }
            }
            else
            {
                ps.Add(new SqlParameter("@id", false));
            }

            //se carga si es necesario los parametros del sp
            if (!string.IsNullOrEmpty(filtros.Buscar))
            {
                ps.Add(new SqlParameter("@deno", true));
                ps.Add(new SqlParameter("@deno_like", filtros.Buscar));
            }

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
