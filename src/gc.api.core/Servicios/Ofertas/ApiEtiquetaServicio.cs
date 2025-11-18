using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using X.PagedList;

namespace gc.api.core.Servicios.Ofertas
{
    public class ApiEtiquetaServicio : Servicio<EntidadBase>, IApiEtiquetaServicio
    {
        private readonly ILogger<ApiEtiquetaServicio> _logger;
        public ApiEtiquetaServicio(IUnitOfWork uow,
            ILogger<ApiEtiquetaServicio> logger) : base(uow)
        {
            _logger = logger;
        }
        public List<CargaPreviaDto> ObtenerCargaPreviaUsuario(string adm_id)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CARGA_PREVIA;

            //var ps = new List<SqlParameter>() {
            //    new SqlParameter("@adm_id", adm_id)
            //};

            List<SqlParameter> ps = [];
            var p = new SqlParameter("@adm_id", SqlDbType.VarChar, 4);
            p.Value = adm_id;
            //ps.Add(new SqlParameter("@adm_id", adm_id));
            ps.Add(p);

            var datos = _repository.EjecutarLstSpExt<CargaPreviaDto>(sp, ps, true);

            return datos;
        }

        public List<IEDetalleDto> ObtenerDetalleEtiquetas(QueryFilters filters)
        {

            var sp = ConstantesGC.StoredProcedures.SP_IE_LISTA;

            List<SqlParameter> ps = [];

            if (filters.FechaD.HasValue && filters.FechaD.Value != DateTime.MinValue &&
                filters.FechaH.HasValue && filters.FechaH.Value != DateTime.MinValue)
            {
                ps.Add(new SqlParameter("@mod", true));
                ps.Add(new SqlParameter("@mod_d", filters.FechaD));
                ps.Add(new SqlParameter("@mod_h", filters.FechaD));
            }
            else
            {
                ps.Add(new SqlParameter("@mod", false));
            }
            ps.Add(new SqlParameter("@sin_imprimir", filters.Opt1));
            ps.Add(new SqlParameter("@oferta", filters.Opt2));

            //previa de usuario
            if (!string.IsNullOrEmpty(filters.Tipo))
            {
                ps.Add(new SqlParameter("@previa", true));
                ps.Add(new SqlParameter("@previa_usu_id", filters.Tipo));
            }
            else
            {
                ps.Add(new SqlParameter("@previa", false));
                ps.Add(new SqlParameter("@previa_usu_id", ""));
            }

            //proveedores 
            if (filters.Rel01 != null && filters.Rel01.Count > 0)
            {
                var provs = string.Join(",", filters.Rel01);
                ps.Add(new SqlParameter("@prov", true));
                ps.Add(new SqlParameter("@prov_list", provs));
            }
            else
            {
                ps.Add(new SqlParameter("@prov", false));
            }

            if (filters.Rel02 != null && filters.Rel02.Count > 0)
            {
                var pgs = string.Join(",", filters.Rel02);
                ps.Add(new SqlParameter("@pg", true));
                ps.Add(new SqlParameter("@pg_list", pgs));
            }
            else
            {
                ps.Add(new SqlParameter("@pg", false));
            }

            if (filters.Rel03 != null && filters.Rel03.Count > 0)
            {
                var rubs = string.Join(",", filters.Rel03.Select(x => x.Id));
                ps.Add(new SqlParameter("@rub", true));
                ps.Add(new SqlParameter("@rub_list", rubs));
            }
            else
            {
                ps.Add(new SqlParameter("@rub", false));
            }

            ps.Add(new SqlParameter("@adm_id", filters.Adm_id));
            ps.Add(new SqlParameter("@usu_id", filters.Usu_id));


            var datos = _repository.EjecutarLstSpExt<IEDetalleDto>(sp, ps, true);
            return datos;
        }

        
    }
}
