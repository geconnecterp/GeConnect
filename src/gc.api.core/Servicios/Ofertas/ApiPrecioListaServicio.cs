using DocumentFormat.OpenXml.Spreadsheet;
using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Productos.Precio;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace gc.api.core.Servicios.Ofertas
{
    public class ApiPrecioListaServicio : Servicio<EntidadBase>, IApiPrecioListaServicio
    {
        private readonly ILogger<ApiPrecioListaServicio> _logger;
        public ApiPrecioListaServicio(IUnitOfWork uow,
            ILogger<ApiPrecioListaServicio> logger) : base(uow)
        {
            _logger = logger;
        }

        public List<PrecioListaDto> ObtenerListaPrecios()
        {
            var sp = ConstantesGC.StoredProcedures.SP_LP_LISTA;
            var ps = new List<SqlParameter>();

            var regs = _repository.EjecutarLstSpExt<PrecioListaDto>(sp, ps,true);
            return [.. regs.Where(x=>!string.IsNullOrEmpty(x.lp_id))];
        }

        public List<PrecioListaDetalleDto> ObtenerDetallePrecios(QueryFilters filters)
        {
            var sp = ConstantesGC.StoredProcedures.SP_LP_DETALLE;
            List<SqlParameter> ps = [];

            if (filters.FechaD.HasValue && filters.FechaD.Value != DateTime.MinValue &&
                filters.FechaH.HasValue && filters.FechaH.Value != DateTime.MinValue)
            {
                var f = filters.FechaD.Value;
                string d = $"{f.Year}/{f.Month}/{f.Day}";
                f = filters.FechaH.Value;
                string h = $"{f.Year}/{f.Month}/{f.Day}";
                ps.Add(new SqlParameter("@mod", true));
                ps.Add(new SqlParameter("@mod_d", d));
                ps.Add(new SqlParameter("@mod_h", h));
            }
            else
            {
                ps.Add(new SqlParameter("@mod", false));
            }
            ps.Add(new SqlParameter("@costo", filters.Opt1));

            //en _filtro base manejarlo como familia
            var lps = filters.Rel04.ToArray();
            var cont = 0;
            for (var i = 0; i < lps.Length; i++)
            {
                cont++;
                ps.Add(new SqlParameter($"@lp_id{cont}", lps[i].Id));
                if(i==4) break; //max 4 listas
            }
            for (int i = cont; i < 4; i++)
            {
                ps.Add(new SqlParameter($"@lp_id{i+1}", ""));
                if(i==4) break; //max 4 listas
            }

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

            if (filters.Rel03 != null && filters.Rel03.Count > 0)
            {
                var pgs = string.Join(",", filters.Rel03.Select(x => x.Id));
                ps.Add(new SqlParameter("@pg", true));
                ps.Add(new SqlParameter("@pg_list", pgs));
            }
            else
            {
                ps.Add(new SqlParameter("@pg", false));
            }

            if (filters.Rel02 != null && filters.Rel02.Count > 0)
            {
                var rubs = string.Join(",", filters.Rel02);
                ps.Add(new SqlParameter("@rub", true));
                ps.Add(new SqlParameter("@rub_list", rubs));
            }
            else
            {
                ps.Add(new SqlParameter("@rub", false));
            }


            ps.Add(new SqlParameter("@adm_id", filters.Adm_id));
            ps.Add(new SqlParameter("@usu_id", filters.Usu_id));

            var res =  _repository.EjecutarLstSpExt<PrecioListaDetalleDto>(sp, ps, true);
            return res;

        }
    }
}
