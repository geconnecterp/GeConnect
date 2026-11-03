using System.Data.Common;
using System.Diagnostics;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gc.api.infra.Datos.Implementacion.Cajas;

// Diagnóstico local de Custodia: observa el lector, sin modificar parámetros ni resultados SQL.
internal sealed class CustodiaSpDiagnostico
{
    private static readonly JsonSerializerSettings Json = new() { DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffffffK", NullValueHandling = NullValueHandling.Include };
    private readonly ILoggerHelper logger;
    private readonly SqlCommand comando;
    private readonly string contexto;
    private readonly Stopwatch reloj = Stopwatch.StartNew();
    private readonly JArray columnas = [];
    private readonly JArray filas = [];
    private readonly List<string> claves = [];
    private bool incompleto;

    internal CustodiaSpDiagnostico(ILoggerHelper logger, SqlCommand comando, string fase, Guid? solicitud = null)
    {
        this.logger = logger; this.comando = comando;
        contexto = $"{comando.CommandText} | Llamada={Guid.NewGuid():N}" +
            (solicitud.HasValue ? $" | Solicitud={solicitud}" : "") +
            $" | Fase={fase} | Servidor={comando.Connection?.DataSource} | Base={comando.Connection?.Database}";
        Emitir(TraceEventType.Information, "REQUEST", () =>
        {
            var parametros = new JObject();
            foreach (SqlParameter p in comando.Parameters)
            {
                var dato = p.Value is null or DBNull ? JValue.CreateNull() : JToken.FromObject(p.Value, JsonSerializer.Create(Json));
                if (p.ParameterName.Equals("@json_rend", StringComparison.OrdinalIgnoreCase) && p.Value is string texto)
                {
                    try
                    {
                        using var sr = new StringReader(texto);
                        using var reader = new JsonTextReader(sr) { DateParseHandling = DateParseHandling.None, FloatParseHandling = FloatParseHandling.Decimal };
                        var json = JToken.ReadFrom(reader);
                        if (json is JArray or JObject && !reader.Read()) dato = json;
                    }
                    catch (JsonException) { /* Conservar el parámetro literal si el JSON no es válido. */ }
                }
                parametros.Add(p.ParameterName, dato);
            }
            return new JObject {
                ["parametros"] = parametros,
                ["tipos_parametros"] = new JArray(comando.Parameters.Cast<SqlParameter>().Select(p => new JObject {
                    ["nombre"] = p.ParameterName, ["tipo_sql"] = p.SqlDbType.ToString(), ["longitud"] = p.Size,
                    ["precision"] = p.Precision, ["escala"] = p.Scale, ["direccion"] = p.Direction.ToString()
                }))
            };
        });
    }

    internal void Columnas(DbDataReader reader) => Observar(() =>
    {
        if (claves.Count != 0 || reader.FieldCount == 0) return;
        var usadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var nombre = reader.GetName(i);
            var clave = nombre; int repeticion = 1;
            while (!usadas.Add(clave)) clave = nombre + "#" + ++repeticion;
            claves.Add(clave);
            columnas.Add(new JObject { ["ordinal"] = i, ["nombre"] = nombre, ["tipo_sql"] = reader.GetDataTypeName(i), ["clave_en_log"] = clave });
        }
    });

    internal void Fila(DbDataReader reader) => Observar(() =>
    {
        Columnas(reader);
        var fila = new JObject();
        for (int i = 0; i < reader.FieldCount; i++)
            fila.Add(claves[i], reader.IsDBNull(i) ? JValue.CreateNull() : JToken.FromObject(reader.GetValue(i), JsonSerializer.Create(Json)));
        filas.Add(fila);
    });

    internal void Respuesta() => Emitir(TraceEventType.Information, "RESPONSE", () => new JObject {
        ["diagnostico_incompleto"] = incompleto, ["columnas"] = columnas, ["filas"] = filas
    }, $" | DuracionMs={reloj.ElapsedMilliseconds} | Filas={filas.Count}");

    internal void Error(Exception ex) => Emitir(TraceEventType.Error, "ERROR", () => new JObject {
        ["tipo"] = ex.GetType().FullName, ["mensaje"] = ex.Message, ["detalle"] = ex.ToString(),
        ["sql_errores"] = ex is SqlException sql
            ? new JArray(sql.Errors.Cast<SqlError>().Select(e => new JObject {
                ["numero"] = e.Number, ["estado"] = e.State, ["clase"] = e.Class,
                ["procedimiento"] = e.Procedure, ["linea"] = e.LineNumber, ["mensaje"] = e.Message
            })) : new JArray(),
        ["lectura_parcial"] = true, ["diagnostico_incompleto"] = incompleto, ["columnas"] = columnas, ["filas_leidas"] = filas
    }, $" | DuracionMs={reloj.ElapsedMilliseconds} | FilasLeidas={filas.Count}");

    private void Observar(Action leer)
    {
        try { leer(); }
        catch (Exception ex)
        {
            incompleto = true;
            try { logger.Log(TraceEventType.Warning, $"[CUSTODIA][LOG ERROR] {contexto} | {ex.GetType().Name}: {ex.Message}"); } catch { }
        }
    }

    private void Emitir(TraceEventType nivel, string fase, Func<JObject> crear, string extra = "")
    {
        try
        {
            var json = crear().ToString(Formatting.None);
            logger.Log(nivel, $"[CUSTODIA][SP {fase}] {contexto}{extra}\n" +
                CajaJsonDiagnostico.Formatear(json, expandirJsonAnidado: false));
        }
        catch (Exception ex)
        {
            // Una falla del diagnóstico no debe cancelar ni alterar una operación de negocio.
            try { logger.Log(TraceEventType.Warning, $"[CUSTODIA][LOG ERROR] {contexto} | FaseLog={fase} | {ex.GetType().Name}: {ex.Message}"); } catch { }
        }
    }
}
