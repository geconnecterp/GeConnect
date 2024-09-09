using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen.Rpr;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class AdministracionServicio : Servicio<Administracion>, IAdministracionServicio
    {
        public AdministracionServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public bool ActualizaMePaId(AdmUpdateMePaDto datos)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ADMINISTRACION_ACTUALIZA_MEPAID;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@adm_id",datos.AdmId),
                new SqlParameter("@mepaid",datos.AdmMePaId)};

            var res = InvokarSpNQuery(sp, ps);

            return true;
        }

        public override Administracion Find(object id)
        {
            var adm = GetAllIq().Where(x => x.Adm_id.Equals(id.ToString()));
            return adm.FirstOrDefaultAsync().GetAwaiter().GetResult();
        }

       

        public override PagedList<Administracion> GetAll(QueryFilters filters)
        {
            filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var administraciones = GetAllIq();
            if (string.IsNullOrEmpty(filters.Sort)) { filters.Sort = "Adm_id"; }
            if (string.IsNullOrEmpty(filters.SortDir)) { filters.SortDir = "ASD"; }
            administraciones = administraciones.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    administraciones = administraciones.Where(r => r.Adm_id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Adm_id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Adm_nombre.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Adm_direccion.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Usu_id_encargado.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Cx_profile.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Cx_base.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Cx_login.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Cx_pass.Contains(filters.Search));
            }

            var paginas = PagedList<Administracion>.Create(administraciones, filters.PageNumber ?? 1, filters.PageSize ?? 20);

            return paginas;
        }

        public ResponseBaseDto ValidaUsuario(string tipo, string id, string usuId)
        {


            var sp = Constantes.ConstantesGC.StoredProcedures.SP_TI_VALIDA_USUARIO;

            var ps = new List<SqlParameter>()
            {
                new SqlParameter("@tipo",tipo),
                new SqlParameter("@id",id),
                new SqlParameter("@usu_id",usuId)
            };

            List<ResponseBaseDto> response = _repository.EjecutarLstSpExt<ResponseBaseDto>(sp, ps, true);

            return response[0];
        }
    }
}
