using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class BOrdenEstadoServicio : Servicio<BOrdenEstado>, IBOrdenEstadoServicio
    {
        public BOrdenEstadoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public override PagedList<BOrdenEstado> GetAll(QueryFilters filters)
        {
            filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var billeteras_ordenes_es = GetAllIq();
            billeteras_ordenes_es = billeteras_ordenes_es.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    billeteras_ordenes_es = billeteras_ordenes_es.Where(r => r.Boe_id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordenes_es = billeteras_ordenes_es.Where(r => r.Boe_id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                billeteras_ordenes_es = billeteras_ordenes_es.Where(r => r.Boe_desc.Contains(filters.Search));
            }

            var paginas = PagedList<BOrdenEstado>.Create(billeteras_ordenes_es, filters.PageNumber??1, filters.PageSize??20);

            return paginas;
        }
    }
}
