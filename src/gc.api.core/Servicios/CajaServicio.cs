using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Cajas;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class CajaServicio : Servicio<Caja>, ICajaServicio
    {
        public CajaServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public override PagedList<Caja> GetAll(QueryFilters filters)
        {
            filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var cajas = GetAllIq();
            cajas = cajas.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    cajas = cajas.Where(r => r.Caja_Id == (string)filters.Id);
                }
            }
          
            if (!string.IsNullOrEmpty(filters.Search))
            {
                cajas = cajas.Where(r => r.Adm_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                cajas = cajas.Where(r => r.Depo_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                cajas = cajas.Where(r => r.Dia_Movi.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                cajas = cajas.Where(r => r.Usu_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                cajas = cajas.Where(r => r.Caja_Nombre.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                cajas = cajas.Where(r => r.Caja_Modalidad.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                cajas = cajas.Where(r => r.Caja_Maquina.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                cajas = cajas.Where(r => r.Caja_Nro_Proceso.Contains(filters.Search));
            }


            if (!string.IsNullOrEmpty(filters.Search))
            {
                cajas = cajas.Where(r => r.Caja_Mepa_Categoria.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                cajas = cajas.Where(r => r.Caja_Mepa_Id.Contains(filters.Search));
            }


            var paginas = PagedList<Caja>.Create(cajas, filters.PageNumber ?? 1, filters.PageSize ?? 20);

            return paginas;
        }


        public Caja Find(string sucId, string cajaId)
        {
            var cajas = GetAllIq().Where(x => x.Caja_Id.Equals(cajaId.ToString()) && x.Adm_Id.Equals(sucId));
            return cajas.FirstOrDefaultAsync().GetAwaiter().GetResult();
        }

        public bool ActualizaMePaId(CajaUpMePaId datos)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_CAJA_ACTUALIZA_MEPAID;

            var ps = new List<SqlParameter>() {
                new("@caja_id",datos.CajaId),
                new("@adm_id",datos.SucId),
                new("@mepa_id",datos.PosMePaId)};
            var res = InvokarSpNQuery(sp, ps);

            var caja = Find(datos.SucId,datos.CajaId);
            if(caja == null)
            {
                throw new NegocioException("No pudo encontrarse la caja. Verifique.");
            }

            if (caja.Caja_Mepa_Id.Equals(datos.PosMePaId))
            {
                return true;
            }
            else
            {
                throw new NegocioException("No se pudo actualizar el MePaId en la Caja");
            }
        }
    }
}
