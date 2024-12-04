using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
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
            filters.Pagina = filters.Pagina == default ? _pagSet.DefaultPageNumber : filters.Pagina;
            filters.Registros = filters.Registros == default ? _pagSet.DefaultPageSize : filters.Registros;

            var proveedoress = GetAllIq();
            proveedoress = proveedoress.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    proveedoress = proveedoress.Where(r => r.Cta_Id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                proveedoress = proveedoress.Where(r => r.Cta_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Ean.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Id_Externo.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Viajante.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Viajante_Ce.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Viajante_Email.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Valores_A_Nombre.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                proveedoress = proveedoress.Where(r => r.Rgan_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                proveedoress = proveedoress.Where(r => r.Ope_Iva.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                proveedoress = proveedoress.Where(r => r.Ctag_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Obs_Op.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                proveedoress = proveedoress.Where(r => r.Ctap_Obs_Precios.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                proveedoress = proveedoress.Where(r => r.Id_Old.Contains(filters.Buscar));
            }

            var paginas = PagedList<Proveedor>.Create(proveedoress, filters.Pagina ?? 1, filters.Registros ?? 20);

            return paginas;
        }

        
    }
}
