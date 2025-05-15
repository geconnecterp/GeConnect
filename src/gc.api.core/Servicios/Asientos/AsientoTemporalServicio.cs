using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Asientos;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Asientos;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.Asientos
{
    public class AsientoTemporalServicio : Servicio<EntidadBase>, IAsientoTemporalServicio
    {
        public AsientoTemporalServicio(IUnitOfWork uow) : base(uow)
        {
        }

        public List<AsientoGridDto> ObtenerAsientos(QueryAsiento query)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CONTA_ASIENTO_TMP;
            var ps = new List<SqlParameter>();

            // Evaluar y agregar parámetros al procedimiento almacenado
            ps.Add(new SqlParameter("@eje_nro", query.Eje_nro > 0 ? query.Eje_nro : 0));
            ps.Add(new SqlParameter("@movi", query.Movi));
            ps.Add(new SqlParameter("@movi_like", !string.IsNullOrWhiteSpace(query.Movi_like) ? query.Movi_like : string.Empty));
            ps.Add(new SqlParameter("@usu", query.Usu));
            ps.Add(new SqlParameter("@usu_like", !string.IsNullOrWhiteSpace(query.Usu_like) ? query.Usu_like : string.Empty));
            ps.Add(new SqlParameter("@tipo", query.Tipo));
            ps.Add(new SqlParameter("@tipo_like", !string.IsNullOrWhiteSpace(query.Tipo_like) ? query.Tipo_like : string.Empty));
            ps.Add(new SqlParameter("@rango", query.Rango));
            ps.Add(new SqlParameter("@desde", query.Rango && query.Desde != default ? query.Desde : new DateTime(1900,1,1)));
            ps.Add(new SqlParameter("@hasta", query.Rango && query.Hasta != default ? query.Hasta : new DateTime(1900,1,1)));
            ps.Add(new SqlParameter("@registros", query.TotalRegistros)); // Valor por defecto: 10
            ps.Add(new SqlParameter("@pagina", query.Paginas )); // Valor por defecto: 1
            ps.Add(new SqlParameter("@ordenar", !string.IsNullOrWhiteSpace(query.Sort) ? query.Sort : "dia_fecha"));

            // Ejecutar el procedimiento almacenado y devolver los resultados
            return _repository.EjecutarLstSpExt<AsientoGridDto>(sp, ps, true);
        }
    }
}
