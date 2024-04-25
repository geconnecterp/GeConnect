using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class AdministracionesServicio : Servicio<Administracion>, IAdministracionServicio
    {
        public AdministracionesServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public override PagedList<Administracion> GetAll(QueryFilters filters)
        {
            filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
            filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;

            var administraciones = GetAllIq();
            if (string.IsNullOrEmpty(filters.Sort)){ filters.Sort = "Adm_id"; }
            if (string.IsNullOrEmpty(filters.SortDir)){ filters.SortDir = "ASD"; }
            administraciones = administraciones.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    administraciones = administraciones.Where(r => r.Adm_id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Adm_id.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Adm_nombre.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Adm_direccion.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Usu_id_encargado.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Cx_profile.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Cx_base.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Cx_login.Contains(filters.Search));
            }

            if (!string.IsNullOrEmpty(filters.Search))
            {
                administraciones = administraciones.Where(r => r.Cx_pass.Contains(filters.Search));
            }

            var paginas = PagedList<Administracion>.Create(administraciones, filters.PageNumber??1, filters.PageSize??20);

            return paginas;
        }
    }
}
