using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Deposito;
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
            filters.Pagina = filters.Pagina == default ? _pagSet.DefaultPageNumber : filters.Pagina;
            filters.Registros = filters.Registros == default ? _pagSet.DefaultPageSize : filters.Registros;

            var depositoss = GetAllIq();
            depositoss = depositoss.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    depositoss = depositoss.Where(r => r.Depo_Id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                depositoss = depositoss.Where(r => r.Depo_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                depositoss = depositoss.Where(r => r.Depo_Nombre.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                depositoss = depositoss.Where(r => r.Adm_Id.Contains(filters.Buscar));
            }

            var paginas = PagedList<Deposito>.Create(depositoss, filters.Pagina ?? 1, filters.Registros ?? 20);

            return paginas;
        }

        public  List<Deposito> ObtenerDepositosDeAdministracion(string adm_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_DEPOSITO_LISTA;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@adm_id",adm_id)
            };

            var res = _repository.EjecutarLstSpExt<Deposito>(sp, ps,true);

            return res;
        }

        public List<DepositoInfoBoxDto> ObtenerDepositioInfoBox(string depo_id,bool soloLibre) {

            var sp = Constantes.ConstantesGC.StoredProcedures.SP_DEPOSITO_INFO_BOX;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@depo_id",depo_id),
                new SqlParameter("@solo_libre",soloLibre),
            };

            var res = _repository.EjecutarLstSpExt<DepositoInfoBoxDto>(sp, ps, true);

            return res;
        }
      
        public List<DepositoInfoStkDto> ObtenerDepositoInfoStk(string depo_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_DEPOSITO_INFO_STK;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@depo_id",depo_id),
            };

            var res = _repository.EjecutarLstSpExt<DepositoInfoStkDto>(sp, ps, true);

            return res;
        }

        public List<DepositoInfoStkValDto> ObtenerDepositoInfoStkValorizado(string adm_id, string depo_id,string concepto)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_DEPOSITO_INFO_STK_VAL;

            var ps = new List<SqlParameter>() {
                new SqlParameter("@adm_id",adm_id),
                new SqlParameter("@depo_id",depo_id),
                new SqlParameter("@concepto",concepto),
            };

            var res = _repository.EjecutarLstSpExt<DepositoInfoStkValDto>(sp, ps, true);

            return res;
        }

    }
}
