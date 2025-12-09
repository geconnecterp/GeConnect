using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Discontinuo;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Text.Json.Nodes;

namespace gc.api.core.Servicios
{
    public class ApiDiscontinuoServicio : Servicio<EntidadBase>, IApiDiscontinuoServicio
    {
        public ApiDiscontinuoServicio(IUnitOfWork uow) : base(uow)
        {

        }

        public RespuestaDto ConfirmarDiscontinuo(AbmGenDto req)
        {
            var sp = ConstantesGC.StoredProcedures.SP_DISCONTINUO_CONFIRMAR;

            var ps = new List<SqlParameter> {
                new SqlParameter("@opcion", req.Objeto),
                new SqlParameter("@json_p", req.Json),
                new SqlParameter("adm_id", req.Administracion),
                new SqlParameter("usu_id", req.Usuario)
            };

            var result = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (result == null || result.Count == 0)
            {
                return new() { resultado = -1, resultado_msj = "No se logró obtener confirmación de la operación." };
            }
            return result[0];
        }

        public List<DiscontinuoDetalleDto> ObtenerProductosDiscontinuos(QueryFilters filters)
        {
            var sp = ConstantesGC.StoredProcedures.SP_DISCONTINUO_DETALLE;
            List<SqlParameter> ps = new List<SqlParameter>();

            ps.Add(new SqlParameter("@Opcion", filters.Id?.ToInt()));
            if (filters.Rel01!=null && filters.Rel01.Count > 0)
            {
                var p_id = filters.Rel01.Select(x => new { p_id = x }).ToList();
                ps.Add(new SqlParameter("@json_p", JsonConvert.SerializeObject(p_id)));
            }
            ps.Add(new SqlParameter("@adm_id", filters.Adm_id ?? string.Empty));
            ps.Add(new SqlParameter("@usu_id", filters.Usu_id ?? string.Empty));

            var datos = _repository.EjecutarLstSpExt<DiscontinuoDetalleDto>(sp, ps, true);
            return datos;
        }
    }
}
