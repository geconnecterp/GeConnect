using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class ProductoDepositoServicio : Servicio<ProductoDeposito>, IProductoDepositoServicio
    {
        public ProductoDepositoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {
            
        }

        

        public override PagedList<ProductoDeposito> GetAll(QueryFilters filters)
        {
            filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var productos_depositoss = GetAllIq();
            productos_depositoss = productos_depositoss.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    productos_depositoss = productos_depositoss.Where(r => r.P_Id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productos_depositoss = productos_depositoss.Where(r => r.P_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productos_depositoss = productos_depositoss.Where(r => r.Box_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productos_depositoss = productos_depositoss.Where(r => r.Depo_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productos_depositoss = productos_depositoss.Where(r => r.Ps_Fv.Contains(filters.Search));
            }

            var paginas = PagedList<ProductoDeposito>.Create(productos_depositoss, filters.PageNumber ?? 1, filters.PageSize ?? 20);

            return paginas;
        }

        public ProductoDeposito ObtenerFechaVencimiento(string pId, string bId)
        {
            var producto = GetAllIq().SingleOrDefault(x=>x.P_Id.Equals(pId) && x.Box_Id.Equals(bId));
            if (producto == null)
            {
                return null;
            }
            return producto;
        }
    }
}
