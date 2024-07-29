using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class ProveedorServicio : Servicio<Proveedor>, IProveedorServicio
    {
        public ProveedorServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public override PagedList<Proveedor> GetAll(QueryFilters filters)
        {
            filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var proveedoress = GetAllIq();
            proveedoress = proveedoress.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    proveedoress = proveedoress.Where(r => r.Cta_Id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                proveedoress = proveedoress.Where(r => r.Cta_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Ean.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Id_Externo.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Viajante.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Viajante_Ce.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Viajante_Email.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Valores_A_Nombre.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                proveedoress = proveedoress.Where(r => r.Rgan_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                proveedoress = proveedoress.Where(r => r.Ope_Iva.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                proveedoress = proveedoress.Where(r => r.Ctag_Id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Obs_Op.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Obs_Precios.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                proveedoress = proveedoress.Where(r => r.Id_Old.Contains(filters.Search));
            }

            var paginas = PagedList<Proveedor>.Create(proveedoress, filters.PageNumber ?? 1, filters.PageSize ?? 20);

            return paginas;
        }

        
    }
}
