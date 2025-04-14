using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class ZonaServicio : Servicio<Zona>, IZonaServicio
    {
        public ZonaServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
        }

        public List<ZonaDto> GetZonaLista()
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ZONAS_LISTA;
            var ps = new List<SqlParameter>();
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new ZonaDto()
                {
                    #region Campos
                    zn_id= x.zn_id,
                    zn_desc= x.zn_desc,
                    zn_lista= x.zn_lista,
                    #endregion
                }).ToList();
        }

        public List<ABMZonaDto> ObtenerZonas(QueryFilters filtros)
        {
            var sp = ConstantesGC.StoredProcedures.SP_ABM_ZONA_LISTA;

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

            List<ABMZonaDto> res = _repository.EjecutarLstSpExt<ABMZonaDto>(sp, ps, true);
            return res;
        }

        public List<ZonaDto> ObtenerZonaPorId(string rp_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_ABM_ZONA_DATO;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@zn_id",rp_id) ,

            };

            List<ZonaDto> res = _repository.EjecutarLstSpExt<ZonaDto>(sp, ps, true);
            return res;
        }
    }
}
