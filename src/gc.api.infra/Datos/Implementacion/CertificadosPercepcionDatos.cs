using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace gc.api.infra.Datos.Implementacion;

public class CertificadosPercepcionDatos(GeConnectContext context) : ICertificadosPercepcionDatos
{
    public const string SpConsulta = "dbo.SPGECO_Imp_Certificdos_AP";
    public static string SpDdjj(string impuesto) => impuesto switch
    {
        "IB" => "dbo.SPGECO_Imp_AP_DDJJ_IB", "IV" => "dbo.SPGECO_Imp_AP_DDJJ_IV",
        _ => throw new ArgumentException("Impuesto de DDJJ no soportado.")
    };
    public static SqlParameter[] ParametrosDdjj(CertificadosPercepcionDdjjPedido pedido)
    {
        pedido.Validar();
        return [new("@periodo", SqlDbType.VarChar, 4) { Value = pedido.Periodo.ToString("0000", CultureInfo.InvariantCulture) },
            new("@mes", SqlDbType.VarChar, 2) { Value = pedido.Mes.ToString("00", CultureInfo.InvariantCulture) }];
    }
    public async IAsyncEnumerable<object?[]> Ddjj(CertificadosPercepcionDdjjPedido pedido, [EnumeratorCancellation] CancellationToken ct)
    {
        var parametros = ParametrosDdjj(pedido);
        await using var cn = new SqlConnection(context.Database.GetConnectionString());
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand(SpDdjj(pedido.Impuesto), cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 540 };
        cmd.Parameters.AddRange(parametros);
        await using var dr = await cmd.ExecuteReaderAsync(ct);
        CertificadosPercepcionDdjjArchivos.ValidarColumnas(dr, pedido.Impuesto);
        while (await dr.ReadAsync(ct))
        {
            var fila = new object?[dr.FieldCount];
            for (var c = 0; c < fila.Length; c++) fila[c] = dr.IsDBNull(c) ? null : dr.GetValue(c);
            yield return fila;
        }
        if (await dr.NextResultAsync(ct)) throw new InvalidOperationException("La DDJJ AP devolvió resultados adicionales. No se entregó un archivo parcial.");
    }
    public static SqlParameter[] Parametros(CertificadosPercepcionFiltro f)
    {
        f.Validar(); var inicio = new DateTime(f.Periodo, f.Mes, 1);
        return [new("@imp_id", SqlDbType.VarChar, 2) { Value = f.Impuesto },
            new("@periodo", SqlDbType.VarChar, 4) { Value = f.Periodo.ToString("0000", CultureInfo.InvariantCulture) },
            new("@mes", SqlDbType.VarChar, 2) { Value = f.Mes.ToString("00", CultureInfo.InvariantCulture) },
            new("@f", SqlDbType.Bit) { Value = f.Fechas },
            new("@fecha_d", SqlDbType.DateTime) { Value = f.Fechas ? f.Desde!.Value.Date : inicio },
            new("@fecha_h", SqlDbType.DateTime) { Value = f.Fechas ? f.Hasta!.Value.Date : inicio.AddMonths(1).AddDays(-1) },
            new("@cuit", SqlDbType.Bit) { Value = f.PorCuit },
            new("@cuit_like", SqlDbType.VarChar, 15) { Value = f.PorCuit ? f.Cuit! : "%" }];
    }
    public static Func<DbDataReader, CertificadoPercepcionFila> CrearMapeador(DbDataReader dr)
    {
        var propiedades = typeof(CertificadoPercepcionFila).GetProperties();
        if (dr.FieldCount != propiedades.Length) throw new InvalidOperationException("La consulta AP devolvió una cantidad de columnas distinta al contrato.");
        var mapa = propiedades.Select(p => (Propiedad: p, Ordinal: dr.GetOrdinal(p.Name))).ToArray();
        foreach (var c in mapa)
        {
            var tipo = Nullable.GetUnderlyingType(c.Propiedad.PropertyType) ?? c.Propiedad.PropertyType;
            if (dr.GetFieldType(c.Ordinal) != tipo) throw new InvalidOperationException($"El contrato de {SpConsulta} cambió en {c.Propiedad.Name}.");
        }
        return reader =>
        {
            var fila = new CertificadoPercepcionFila();
            foreach (var c in mapa) c.Propiedad.SetValue(fila, reader.IsDBNull(c.Ordinal) ? null : reader.GetValue(c.Ordinal));
            return fila;
        };
    }
    public async Task<List<PercepcionImpuesto>> Impuestos(CancellationToken ct)
    {
        await using var cn = new SqlConnection(context.Database.GetConnectionString());
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand("dbo.SPGECO_Tipos_Impuestos", cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 60 };
        await using var dr = await cmd.ExecuteReaderAsync(ct);
        var id = dr.GetOrdinal("imp_id"); var texto = dr.GetOrdinal("imp_descripcion"); var agente = dr.GetOrdinal("agente_per");
        var lista = new List<PercepcionImpuesto>();
        while (await dr.ReadAsync(ct))
            lista.Add(new() { Id = dr.GetString(id).TrimEnd(), Texto = dr.GetString(texto).TrimEnd(), AgentePer = dr.GetString(agente).TrimEnd() });
        return lista;
    }
    public async Task<List<CertificadoPercepcionFila>> Consultar(CertificadosPercepcionFiltro filtro, CancellationToken ct)
    {
        var parametros = Parametros(filtro);
        await using var cn = new SqlConnection(context.Database.GetConnectionString());
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand(SpConsulta, cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 540 };
        cmd.Parameters.AddRange(parametros);
        await using var dr = await cmd.ExecuteReaderAsync(ct);
        var mapear = CrearMapeador(dr); var filas = new List<CertificadoPercepcionFila>();
        while (await dr.ReadAsync(ct)) filas.Add(mapear(dr));
        if (await dr.NextResultAsync(ct)) throw new InvalidOperationException("La consulta AP devolvió más de un resultado; no se muestra una salida parcial.");
        // Preservar orden, multiplicidad y NULL del SP. No corregir las subconsultas de origen/LH.
        return filas;
    }
}
