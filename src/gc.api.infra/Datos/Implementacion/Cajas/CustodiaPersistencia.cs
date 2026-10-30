using System.Data;
using System.Diagnostics;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Servicios.LineaCaja;
using gc.api.infra.Datos.Contratos;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Dtos.Cajas;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gc.api.infra.Datos.Implementacion.Cajas;

public partial class CustodiaPersistencia(IDataConnectionContext context, ILoggerHelper logger) : ICustodiaPersistencia
{
    private static readonly JsonSerializerSettings Json = new() { DateFormatString = "yyyy-MM-dd HH:mm:ss.fff", NullValueHandling = NullValueHandling.Include };
    private SqlConnection Conexion() => (SqlConnection)((ICloneable)context.ObtenerConexionSql()).Clone();

    public List<CustodiaRendicionDto> Pendientes(string sucursal, string instrumento)
    {
        using var cnn = Conexion(); cnn.Open();
        using var cmd = ConsultaPendientes(cnn, null, sucursal, instrumento);
        using var plazo = new CancellationTokenSource(TimeSpan.FromSeconds(25));
        using var cancelar = plazo.Token.Register(cmd.Cancel);
        logger.Log(TraceEventType.Information, $"[CUSTODIA][SP REQUEST] {cmd.CommandText}\n{JsonConvert.SerializeObject(new { adm_id = sucursal, ins_id = instrumento }, Formatting.Indented)}");
        try
        {
            using var reader = cmd.ExecuteReader();
            List<CustodiaRendicionDto> filas;
            try { filas = CustodiaRendicionReader.Leer(reader, sucursal, instrumento, plazo.Token); }
            catch { cmd.Cancel(); throw; }
            logger.Log(TraceEventType.Information, $"[CUSTODIA][SP RESPONSE] {cmd.CommandText} | Sucursal={sucursal} | Instrumento={instrumento} | Filas={filas.Count}");
            return filas;
        }
        catch { cmd.Cancel(); throw; }
    }

    private static SqlCommand ConsultaPendientes(SqlConnection cnn, SqlTransaction? tx, string sucursal, string instrumento)
    {
        var cmd = new SqlCommand(CustodiaServicio.SpPendientes, cnn, tx) { CommandType = CommandType.StoredProcedure, CommandTimeout = 25 };
        cmd.Parameters.Add(new("@adm_id", SqlDbType.VarChar, 4) { Value = sucursal });
        cmd.Parameters.Add(new("@ins_id", SqlDbType.VarChar, 3) { Value = instrumento });
        return cmd;
    }

    // La lectura y los SP de todos los grupos comparten una transacción. Los COMMIT internos del SP no confirman este nivel exterior.
    public CustodiaSpResultado Corregir(IReadOnlyList<CustodiaCambio> cambios, string sucursal, string usuario, Guid solicitud)
    {
        using var cnn = Conexion();
        SqlTransaction? tx = null;
        bool escribiendo = false, confirmando = false;
        using var plazoOperacion = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        try
        {
            cnn.Open(); tx = cnn.BeginTransaction(IsolationLevel.Serializable);
            ValidarVigencia(cnn, tx, cambios, sucursal, plazoOperacion.Token);
            foreach (var grupo in cambios.GroupBy(c => (c.Despues.caja_nro_proceso, c.Despues.caja_nro_cierre, c.Despues.caja_nro_rend)))
            {
                using var cmd = new SqlCommand(CustodiaServicio.SpCorrecciones, cnn, tx) { CommandType = CommandType.StoredProcedure, CommandTimeout = 60 };
                plazoOperacion.Token.ThrowIfCancellationRequested();
                using var cancelar = plazoOperacion.Token.Register(cmd.Cancel);
                cmd.Parameters.AddRange(CrearParametrosCorreccion(grupo.Select(c => c.Despues), sucursal, usuario).ToArray());
                escribiendo = true;
                var resultado = Ejecutar(cmd, solicitud);
                if (resultado.Codigo != 0) { Revertir(tx); return resultado; }
            }
            plazoOperacion.Token.ThrowIfCancellationRequested();
            confirmando = true; tx.Commit();
            return new(0, "Las correcciones se guardaron correctamente.");
        }
        catch (ArgumentException) when (!escribiendo) { Revertir(tx); throw; }
        catch (Exception ex)
        {
            var revertido = Revertir(tx);
            throw new CustodiaPersistenciaException("No se pudieron guardar las correcciones.", confirmando || (escribiendo && !revertido), ex);
        }
        finally { tx?.Dispose(); }
    }

    internal static List<SqlParameter> CrearParametrosCorreccion(IEnumerable<CustodiaLineaDto> lineas, string sucursal, string usuario)
    {
        var grupo = lineas.ToList();
        if (grupo.Count == 0) throw new ArgumentException("No hay correcciones en el grupo.");
        var primera = grupo[0];
        if (grupo.Any(f => f.caja_nro_proceso != primera.caja_nro_proceso || f.caja_nro_cierre != primera.caja_nro_cierre || f.caja_nro_rend != primera.caja_nro_rend || f.adm_id != sucursal))
            throw new ArgumentException("Las correcciones de un grupo deben corresponder al mismo proceso, cierre, rendición y sucursal.");
        return [
            new("@caja_nro_proceso", SqlDbType.VarChar, 20) { Value = primera.caja_nro_proceso },
            new("@caja_nro_cierre", SqlDbType.SmallInt) { Value = primera.caja_nro_cierre },
            new("@caja_nro_rend", SqlDbType.SmallInt) { Value = primera.caja_nro_rend },
            new("@tcf_id", SqlDbType.VarChar, 2) { Value = "EF" },
            new("@usu_id", SqlDbType.VarChar, 10) { Value = usuario },
            new("@adm_id", SqlDbType.VarChar, 10) { Value = sucursal },
            new("@json_rend", SqlDbType.VarChar, -1) { Value = JsonConvert.SerializeObject(grupo, Formatting.None, Json) },
            new("@app", SqlDbType.VarChar, 10) { Value = "CAJA" }
        ];
    }

    private static bool Revertir(SqlTransaction? tx)
    {
        if (tx == null) return false;
        try { tx.Rollback(); return true; } catch { return false; }
    }

    private static void ValidarVigencia(SqlConnection cnn, SqlTransaction tx, IReadOnlyList<CustodiaCambio> cambios, string sucursal, CancellationToken cancellationToken)
    {
        var disponibles = new Dictionary<string, CustodiaRendicionDto>();
        foreach (var instrumento in cambios.Select(c => c.Despues.ins_id).Distinct())
        {
            using var cmd = ConsultaPendientes(cnn, tx, sucursal, instrumento);
            using var plazo = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken); plazo.CancelAfter(TimeSpan.FromSeconds(25));
            using var cancelar = plazo.Token.Register(cmd.Cancel);
            try
            {
                using var reader = cmd.ExecuteReader();
                try
                {
                    foreach (var fila in CustodiaRendicionReader.Leer(reader, sucursal, instrumento, plazo.Token))
                        if (!disponibles.TryAdd(fila.clave, fila)) throw new ArgumentException("Hay identificadores de rendición duplicados en la consulta.");
                }
                catch { cmd.Cancel(); throw; }
            }
            catch { cmd.Cancel(); throw; }
        }
        foreach (var cambio in cambios)
            if (!disponibles.TryGetValue(cambio.Antes.clave, out var fila) || fila.version != cambio.Antes.version)
                throw new ArgumentException("Una rendición cambió o ya fue entregada. Actualice la consulta antes de continuar.");
    }

    private CustodiaSpResultado Ejecutar(SqlCommand cmd, Guid solicitud)
    {
        var parametros = JObject.FromObject(cmd.Parameters.Cast<SqlParameter>().ToDictionary(p => p.ParameterName, p => p.Value));
        if (cmd.Parameters.Contains("@json_rend")) parametros["@json_rend"] = JArray.Parse((string)cmd.Parameters["@json_rend"].Value);
        logger.Log(TraceEventType.Information, $"[CUSTODIA][SP REQUEST] {cmd.CommandText} | Solicitud={solicitud}\n{parametros.ToString(Formatting.Indented)}");
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) throw new InvalidOperationException("El SP no devolvió un resultado explícito.");
        var codigo = reader.GetOrdinal("resultado");
        if (reader.IsDBNull(codigo)) throw new InvalidOperationException("El SP devolvió resultado nulo.");
        var mensaje = reader["resultado_msj"]?.ToString() ?? "";
        var id = "";
        for (var i = 0; i < reader.FieldCount; i++) if (reader.GetName(i).Equals("resultado_id", StringComparison.OrdinalIgnoreCase)) id = reader.IsDBNull(i) ? "" : reader.GetValue(i).ToString() ?? "";
        var resultado = new CustodiaSpResultado(Convert.ToInt16(reader.GetValue(codigo)), mensaje, id);
        logger.Log(TraceEventType.Information, $"[CUSTODIA][SP RESPONSE] {cmd.CommandText} | Solicitud={solicitud}\n{JsonConvert.SerializeObject(new { resultado = resultado.Codigo, resultado_msj = resultado.Mensaje, resultado_id = resultado.Id }, Formatting.Indented)}");
        return resultado;
    }
}
