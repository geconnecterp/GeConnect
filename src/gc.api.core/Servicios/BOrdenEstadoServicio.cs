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
            filters.Pagina = filters.Pagina == default ? _pagSet.DefaultPageNumber : filters.Pagina;
            filters.Registros = filters.Registros == default ? _pagSet.DefaultPageSize : filters.Registros;

            var billeteras_ordenes_es = GetAllIq();
            billeteras_ordenes_es = billeteras_ordenes_es.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    billeteras_ordenes_es = billeteras_ordenes_es.Where(r => r.Boe_id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordenes_es = billeteras_ordenes_es.Where(r => r.Boe_id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                billeteras_ordenes_es = billeteras_ordenes_es.Where(r => r.Boe_desc.Contains(filters.Buscar));
            }

            var paginas = PagedList<BOrdenEstado>.Create(billeteras_ordenes_es, filters.Pagina??1, filters.Registros??20);

            return paginas;
        }
    }
}
