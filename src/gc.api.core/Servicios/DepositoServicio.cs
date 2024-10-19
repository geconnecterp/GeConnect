using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class DepositoServicio : Servicio<Deposito>, IDepositoServicio
    {
        public DepositoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public override PagedList<Deposito> GetAll(QueryFilters filters)
        {
            filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var depositoss = GetAllIq();
            depositoss = depositoss.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    depositoss = depositoss.Where(r => r.Depo_Id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                depositoss = depositoss.Where(r => r.Depo_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                depositoss = depositoss.Where(r => r.Depo_Nombre.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                depositoss = depositoss.Where(r => r.Adm_Id.Contains(filters.Search));
            }

            var paginas = PagedList<Deposito>.Create(depositoss, filters.PageNumber ?? 1, filters.PageSize ?? 20);

            return paginas;
        }

        public  List<Deposito> ObtenerDepositosDeAdministracion(string adm_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_DEPOSITOS;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@adm_id",adm_id)
            };

            var res = _repository.EjecutarLstSpExt<Deposito>(sp, ps,true);

            return res;
        }

      
    }
}
