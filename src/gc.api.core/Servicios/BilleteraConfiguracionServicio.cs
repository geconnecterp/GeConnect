using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Servicios;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace geco_0000.Core.Servicios
{
    public class BilleteraConfiguracionServicio : Servicio<BilleteraConfiguracion>, IBilleteraConfiguracionServicio
   {
       public BilleteraConfiguracionServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
       {

       }

       public override PagedList<BilleteraConfiguracion> GetAll(QueryFilters filters)
       {
            filters.Pagina = filters.Pagina == default ? _pagSet.DefaultPageNumber : filters.Pagina;
            filters.Registros = filters.Registros == default ? _pagSet.DefaultPageSize : filters.Registros;

            var billetera_configuracions = GetAllIq();
            billetera_configuracions = billetera_configuracions.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    billetera_configuracions = billetera_configuracions.Where(r => r.Bc_Id == (string)filters.Id);
                }
            }           

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billetera_configuracions = billetera_configuracions.Where(r => r.Bc_Ruta_Publickey.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billetera_configuracions = billetera_configuracions.Where(r => r.Bc_Ruta_Privatekey.Contains(filters.Buscar));
            }

            var paginas = PagedList<BilleteraConfiguracion>.Create(billetera_configuracions, filters.Pagina??1, filters.Registros??20);

            return paginas;
        }
    }
}
