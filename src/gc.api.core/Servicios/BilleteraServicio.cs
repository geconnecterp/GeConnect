using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class BilleteraServicio : Servicio<Billetera>, IBilleteraServicio
    {
        public BilleteraServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public override PagedList<Billetera> GetAll(QueryFilters filters)
        {
            filters.Pagina = filters.Pagina == default ? _pagSet.DefaultPageNumber : filters.Pagina;
            filters.Registros = filters.Registros == default ? _pagSet.DefaultPageSize : filters.Registros;

            var billeterass = GetAllIq();
            billeterass = billeterass.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    billeterass = billeterass.Where(r => r.Bill_id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeterass = billeterass.Where(r => r.Bill_id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeterass = billeterass.Where(r => r.Bill_desc.Contains(filters.Buscar));
            }

            var paginas = PagedList<Billetera>.Create(billeterass, filters.Pagina ?? 1, filters.Registros ?? 20);

            return paginas;
        }

        public (Billetera, string) FindBilletera(string id)
        {
            var bills = GetAllIq().Where(x => x.Bill_id.Equals(id));
            var bill = bills.FirstOrDefaultAsync().GetAwaiter().GetResult();

            var _repPK = _uow.GetRepository<BilleteraConfiguracion>();
            var bcs = _repPK.GetAll().Where(c => c.Bc_Id.Equals("0001"));
            var publicKey = bcs.First().Bc_Ruta_Publickey;
            return (bill, publicKey);
        }
    }
}
