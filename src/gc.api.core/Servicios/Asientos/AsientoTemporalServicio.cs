using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Asientos;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
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
            ps.Add(new SqlParameter("@desde", query.Rango && query.Desde != default ? query.Desde : new DateTime(1900, 1, 1)));
            ps.Add(new SqlParameter("@hasta", query.Rango && query.Hasta != default ? query.Hasta : new DateTime(1900, 1, 1)));
            ps.Add(new SqlParameter("@registros", query.TotalRegistros)); // Valor por defecto: 10
            ps.Add(new SqlParameter("@pagina", query.Paginas)); // Valor por defecto: 1
            ps.Add(new SqlParameter("@ordenar", !string.IsNullOrWhiteSpace(query.Sort) ? query.Sort : "dia_fecha"));

            // Ejecutar el procedimiento almacenado y devolver los resultados
            return _repository.EjecutarLstSpExt<AsientoGridDto>(sp, ps, true);
        }

        public RespuestaDto PasarAsientosTmpAContabilidad(AsientoPasaDto asientoPasa)
        {
            var sp = ConstantesGC.StoredProcedures.SP_ASIENTO_TMP_PASA;
            var ps = new List<SqlParameter>
                {
                    new SqlParameter("@movi_id", asientoPasa.Movi_id),
                    new SqlParameter("@usu_id", asientoPasa.Usu_id),
                    new SqlParameter("@adm_id", asientoPasa.Adm_id)
                };
            // Ejecutar el procedimiento almacenado y devolver la respuesta
            var res = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            if (res != null && res.Count > 0)
            {
                return res.FirstOrDefault();
            }
            else
            {
                return new RespuestaDto
                {
                    resultado = -1,
                    resultado_msj = "No se obtuvo respuesta del procedimiento almacenado."
                };
            }
        }

        public AsientoDetalleDto ObtenerAsientoDetalle(string moviId)
        {
            if (string.IsNullOrWhiteSpace(moviId))
            {
                return null;
            }

            var sp = ConstantesGC.StoredProcedures.SP_ASIENTO_TMP_DETALLE;// "spgeco_conta_asiento_tmp_datos"; // Usar el SP que mencionaste

            var ps = new List<SqlParameter>
                {
                    new SqlParameter("@dia_movi", moviId)
                };

            // Ejecutar el procedimiento almacenado
            var resultados = _repository.EjecutarLstSpExt<AsientoPlanoDto>(sp, ps, true);

            if (resultados == null || resultados.Count == 0)
            {
                return null;
            }

            // Crear el objeto AsientoDetalleDto a partir del primer registro
            var primerRegistro = resultados.First();
            var asientoDetalle = new AsientoDetalleDto()
            {
                Dia_movi = primerRegistro.dia_movi,
                Dia_fecha = primerRegistro.dia_fecha,
                Dia_tipo = primerRegistro.dia_tipo,
                Dia_lista = primerRegistro.dia_lista,
                Dia_desc_asiento = primerRegistro.dia_desc_asiento,
                Detalles = new List<AsientoLineaDto>(),
                TotalDebe = 0,
                TotalHaber = 0
            };

            // Agregar cada registro como una línea de detalle
            foreach (var registro in resultados)
            {
                var linea = new AsientoLineaDto
                {
                    Dia_movi = registro.dia_movi,
                    Dia_nro = registro.dia_nro,
                    Ccb_id = registro.ccb_id,
                    Ccb_desc = registro.ccb_desc,
                    Dia_desc = registro.dia_desc,
                    Debe = registro.debe,
                    Haber = registro.haber
                };

                asientoDetalle.Detalles.Add(linea);
                asientoDetalle.TotalDebe += linea.Debe;
                asientoDetalle.TotalHaber += linea.Haber;
            }

            return asientoDetalle;
        }
    }
}
