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
            filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var billetera_configuracions = GetAllIq();
            billetera_configuracions = billetera_configuracions.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    billetera_configuracions = billetera_configuracions.Where(r => r.Bc_Id == (string)filters.Id);
                }
            }           

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billetera_configuracions = billetera_configuracions.Where(r => r.Bc_Url_Base_Notificacion.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billetera_configuracions = billetera_configuracions.Where(r => r.Bc_Url_Base_Servicio.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billetera_configuracions = billetera_configuracions.Where(r => r.Bc_Ruta_Publickey.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billetera_configuracions = billetera_configuracions.Where(r => r.Bc_Ruta_Privatekey.Contains(filters.Search));
            }

            var paginas = PagedList<BilleteraConfiguracion>.Create(billetera_configuracions, filters.PageNumber??1, filters.PageSize??20);

            return paginas;
        }
    }
}
