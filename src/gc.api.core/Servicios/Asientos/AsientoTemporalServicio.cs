using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Asientos;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace gc.api.core.Servicios.Asientos
{
    public class AsientoTemporalServicio : Servicio<EntidadBase>, IAsientoTemporalServicio
    {
        public AsientoTemporalServicio(IUnitOfWork uow) : base(uow)
        {
        }

        public List<AsientoGridDto> ObtenerAsientos(QueryAsiento query)
        {
            var sp = ConstantesGC.StoredProcedures.SP_CONTA_ASIENTOS_TMP;
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

        /// <summary>
        /// Pasa varios asientos temporales a contabilidad definitiva
        /// </summary>
        /// <param name="asientoPasa">Datos necesarios para el traspaso de asientos</param>
        /// <returns>Lista de resultados de la operación para cada asiento</returns>
        public List<RespuestaDto> PasarAsientosTmpAContabilidad(AsientoPasaDto asientoPasa)
        {
            List<RespuestaDto> resultados = new List<RespuestaDto>();

            try
            {
                var sp = ConstantesGC.StoredProcedures.SP_ASIENTO_TMP_PASA;

                // Validación básica
                if (asientoPasa == null || string.IsNullOrEmpty(asientoPasa.JsonDiaMovi))
                {
                    resultados.Add(new RespuestaDto
                    {
                        resultado = -1,
                        resultado_msj = "No se recibieron asientos para procesar"
                    });
                    return resultados;
                }

                // Deserializar el JSON a un array de IDs de asientos
                string[] asientosIds = JsonConvert.DeserializeObject<string[]>(asientoPasa.JsonDiaMovi);

                if (asientosIds == null || asientosIds.Length == 0)
                {
                    resultados.Add(new RespuestaDto
                    {
                        resultado = -1,
                        resultado_msj = "No se encontraron asientos válidos en los datos proporcionados"
                    });
                    return resultados;
                }

                // parametros del sp


                // Iterar a través de los IDs y procesar cada asiento
                foreach (string moviId in asientosIds)
                {
                    try
                    {
                        var ps = new List<SqlParameter>{
                                new SqlParameter("@eje_nro", asientoPasa.Eje_nro),
                                new SqlParameter("@dia_movi", moviId),
                                new SqlParameter("@usu_id", asientoPasa.Usu_id),
                                new SqlParameter("@adm_id", asientoPasa.Adm_id)
                        };

                        // Ejecutar el procedimiento almacenado y devolver la respuesta
                        var res = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);

                        if (res == null || res.Count == 0)
                        {
                            resultados.Add(new RespuestaDto
                            {
                                resultado = -1,
                                resultado_id = moviId,
                                resultado_msj = $"No se obtuvo respuesta del procedimiento almacenado para el asiento {moviId}"
                            });
                            continue;
                        }
                        var result = res.First();
                        var respuesta = new RespuestaDto
                        {
                            resultado = result.resultado,
                            resultado_id = moviId, // Guardamos el ID del asiento procesado
                            resultado_msj = result.resultado_msj
                        };

                        // Agregar la respuesta a la lista de resultados
                        resultados.Add(respuesta);
                    }
                    catch (Exception ex)
                    {
                        // Si ocurre un error durante el procesamiento de un asiento, 
                        // lo registramos y continuamos con el siguiente                      

                        resultados.Add(new RespuestaDto
                        {
                            resultado = -1,
                            resultado_id = moviId,
                            resultado_msj = $"Error al procesar el asiento {moviId}: {ex.Message}"
                        });
                    }
                }

                return resultados;
            }
            catch (Exception ex)
            {
                resultados.Add(new RespuestaDto
                {
                    resultado = -1,
                    resultado_msj = $"Error general al procesar los asientos: {ex.Message}"
                });

                return resultados;
            }
        }

        public AsientoDetalleDto ObtenerAsientoDetalle(string moviId, bool esReporte = false)
        {
            if (string.IsNullOrWhiteSpace(moviId))
            {
                return null;
            }

            string sp;
            if (esReporte)
            {
                sp = ConstantesGC.StoredProcedures.SP_ASIENTO_DETALLE_REPO; 
            }
            else
            {
                sp = ConstantesGC.StoredProcedures.SP_ASIENTO_TMP_DETALLE;// "spgeco_conta_asiento_tmp_datos"; // Usar el SP que mencionaste
            }
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
                esTemporal = primerRegistro.temporal,
                usu_id = primerRegistro.usu_id,
                usu_apellidoynombre = primerRegistro.usu_apellidoynombre,
                eje_nro = primerRegistro.eje_nro,
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
