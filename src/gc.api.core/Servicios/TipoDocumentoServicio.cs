using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class TipoDocumentoServicio : Servicio<TipoDocumento>, Itipos_documentosServicio
    {
        public TipoDocumentoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public override PagedList<TipoDocumento> GetAll(QueryFilters filters)
        {
            filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var tipos_documentoss = GetAllIq();
            tipos_documentoss = tipos_documentoss.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    tipos_documentoss = tipos_documentoss.Where(r => r.Tdoc_Id == (string)filters.Id);
                }
            }           

            if (!string.IsNullOrEmpty(filters.Search))
            {
                tipos_documentoss = tipos_documentoss.Where(r => r.Tdoc_Desc.Contains(filters.Search));
            }

            var paginas = PagedList<TipoDocumento>.Create(tipos_documentoss, filters.PageNumber ?? 1, filters.PageSize ?? 20);

            return paginas;
        }
    }
}
