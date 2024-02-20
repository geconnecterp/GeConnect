using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class TestServicio:Servicio<Test>,ITestServicio
    {
        public TestServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public override PagedList<Test> GetAll(QueryFilters filters)
        {
            filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var tests = GetAllIq();
            tests = tests.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    tests = tests.Where(r => r.Id == (int)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                tests = tests.Where(r => r.DatoStr.Contains(filters.Search));
            }

            var paginas = PagedList<Test>.Create(tests, filters.PageNumber, filters.PageSize);

            return paginas;
        }
    }
}
