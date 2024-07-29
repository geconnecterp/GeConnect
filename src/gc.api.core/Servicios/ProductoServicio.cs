using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class ProductoServicio : Servicio<Producto>, IProductoServicio
    {
        public ProductoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public override PagedList<Producto> GetAll(QueryFilters filters)
        {
            filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var productoss = GetAllIq();
            productoss = productoss.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    productoss = productoss.Where(r => r.P_Id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.P_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.P_M_Marca.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.P_M_Desc.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.P_M_Capacidad.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.P_Id_Prov.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.P_Desc.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.P_Desc_Ticket.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.Up_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.Rub_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.Cta_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.Pg_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.P_Boni.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.Usu_Id_Alta.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.Usu_Id_Modi.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.P_Obs.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.P_Balanza_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                productoss = productoss.Where(r => r.Lp_Id_Default.Contains(filters.Search));
            }

            var paginas = PagedList<Producto>.Create(productoss, filters.PageNumber ?? 1, filters.PageSize ?? 20);

            return paginas;
        }
    }
}
