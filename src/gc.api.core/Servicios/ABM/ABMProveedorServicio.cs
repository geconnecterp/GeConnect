using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Text;

namespace gc.api.core.Servicios.ABM
{
    public class ABMProveedorServicio : Servicio<Proveedor>, IABMProveedorServicio
    {
        public ABMProveedorServicio(IUnitOfWork uow, IOptions<PaginationOptions> pag) : base(uow, pag)
        {
        }

        public List<ABMProveedorSearchDto> Buscar(QueryFilters filtros)
        {
            filtros.Pagina = filtros.Pagina == null || filtros.Pagina <= 0 ? _pagSet.DefaultPageNumber : filtros.Pagina;
            filtros.Registros = filtros.Registros == null || filtros.Registros <= 0 ? _pagSet.DefaultPageSize : filtros.Registros;

            string sp = ConstantesGC.StoredProcedures.SP_ABM_PROV_LISTA;

            var ps = new List<SqlParameter>();

            //debo cargar aca todos los filtros sobre los parametros a utilizar
            if (!string.IsNullOrEmpty(filtros.Id))
            {
                ps.Add(new SqlParameter("@id", true));
                //hay un id de producto. se habilita la seccion de productos
                ps.Add(new SqlParameter("@cta_id_d", filtros.Id));

                if (!string.IsNullOrEmpty(filtros.Id2))
                {
                    ps.Add(new SqlParameter("@cta_id_h", filtros.Id2));
                }
                else
                {
                    ps.Add(new SqlParameter("@cta_id_h", filtros.Id));
                }
            }
            else
            {
                ps.Add(new SqlParameter("@id", false));
            }

            //se carga si es necesario los parametros del sp
            if (!string.IsNullOrEmpty(filtros.Buscar))
            {
                ps.Add(new SqlParameter("@deno", true));
                ps.Add(new SqlParameter("@cta_deno_like", filtros.Buscar));
            }

            if (filtros.Rel01 != null && filtros.Rel01.Count > 0)
            {
                ps.Add(new SqlParameter("@ope", true));
                StringBuilder sb = new();
                bool first = true;
                foreach (var item in filtros.Rel01)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(',');
                    }
                    sb.Append(item);
                }

                ps.Add(new SqlParameter("@ope_list", sb.ToString()));
            }
            else
            {
                ps.Add(new SqlParameter("@ope", false));
            }

            //if (filtros.Rel02 != null && filtros.Rel02.Count > 0)
            //{
            //    ps.Add(new SqlParameter("@zona", true));
            //    StringBuilder sb = new();
            //    bool first = true;
            //    foreach (var item in filtros.Rel02)
            //    {
            //        if (first)
            //        {
            //            first = false;
            //        }
            //        else
            //        {
            //            sb.Append(',');
            //        }
            //        sb.Append(item);
            //    }

            //    ps.Add(new SqlParameter("@zona_list", sb.ToString()));
            //}
            //else
            //{
            //    ps.Add(new SqlParameter("@zona", false));
            //}
            //cantidad de registros
            ps.Add(new SqlParameter("@registros", filtros.Registros));
            //pagina de visualización => Si se filtran producto "Fernet" y se hayan 54 reg.
            //Si los registros de 1 pagina son 200 y la pagina es 1, se presentaran los 54 reg.
            //Si la pagina solicitada es la 2, se devolveran 0 registros.
            ps.Add(new SqlParameter("@pagina", filtros.Pagina));
            ps.Add(new SqlParameter("@ordenar", filtros.Sort ?? ""));

            List<ABMProveedorSearchDto> cliente = _repository.EjecutarLstSpExt<ABMProveedorSearchDto>(sp, ps, true);

            //var items = PagedList<ABMProductoSearchDto>.Create(producto, filtros.Pagina.Value, filtros.Registros.Value);

            return cliente;
        }
    }
}
