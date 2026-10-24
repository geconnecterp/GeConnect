using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace gc.api.infra.Datos.Implementacion;

public class CertificadosRetencionDatos(GeConnectContext context) : ICertificadosRetencionDatos
{
    public const string SpConsulta = "dbo.SPGECO_Imp_Certificdos_AR";
    public static string SpDdjj(string impuesto) => impuesto switch
    {
        "IB" => "dbo.SPGECO_Imp_AR_DDJJ_IB", "IV" => "dbo.SPGECO_Imp_AR_DDJJ_IV", "GA" => "dbo.SPGECO_Imp_AR_DDJJ_GA",
        _ => throw new ArgumentException("Impuesto de DDJJ no soportado.")
    };
    public static SqlParameter[] ParametrosDdjj(CertificadosRetencionDdjjPedido p)
    {
        p.Validar();
        return [new("@periodo", SqlDbType.VarChar, 4) { Value = p.Periodo.ToString("0000", CultureInfo.InvariantCulture) },
            new("@mes", SqlDbType.VarChar, 2) { Value = p.Mes.ToString("00", CultureInfo.InvariantCulture) }];
    }
    public async IAsyncEnumerable<object?[]> Ddjj(CertificadosRetencionDdjjPedido pedido, [EnumeratorCancellation] CancellationToken ct)
    {
        var parametros = ParametrosDdjj(pedido);
        var columnas = CertificadosRetencionDdjjArchivos.Columnas(pedido.Impuesto);
        await using var cn = new SqlConnection(context.Database.GetConnectionString());
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand(SpDdjj(pedido.Impuesto), cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 540 };
        cmd.Parameters.AddRange(parametros);
        await using var dr = await cmd.ExecuteReaderAsync(ct);
        if (dr.FieldCount != columnas.Length) throw new InvalidOperationException("La DDJJ devolvió una cantidad de columnas diferente al contrato.");
        for (var c = 0; c < columnas.Length; c++)
            if (dr.GetFieldType(c) != columnas[c].Tipo || (pedido.Impuesto != "GA" && !string.Equals(dr.GetName(c), columnas[c].Campo, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("La DDJJ devolvió columnas o tipos diferentes al contrato.");
        while (await dr.ReadAsync(ct))
        {
            var fila = new object?[columnas.Length];
            for (var c = 0; c < fila.Length; c++) fila[c] = dr.IsDBNull(c) ? null : dr.GetValue(c);
            yield return fila;
        }
        if (await dr.NextResultAsync(ct)) throw new InvalidOperationException("La DDJJ devolvió resultados adicionales. No se entregó un archivo parcial.");
    }
    public static SqlParameter[] Parametros(CertificadosRetencionFiltro f)
    {
        f.Validar();
        var inicio = new DateTime(f.Periodo, f.Mes, 1);
        return [new("@imp_id", SqlDbType.VarChar, 2) { Value = f.Impuesto },
            new("@periodo", SqlDbType.VarChar, 4) { Value = f.Periodo.ToString("0000", CultureInfo.InvariantCulture) },
            new("@mes", SqlDbType.VarChar, 2) { Value = f.Mes.ToString("00", CultureInfo.InvariantCulture) },
            new("@f", SqlDbType.Bit) { Value = f.Fechas },
            new("@fecha_d", SqlDbType.DateTime) { Value = f.Fechas ? f.Desde!.Value.Date : inicio },
            new("@fecha_h", SqlDbType.DateTime) { Value = f.Fechas ? f.Hasta!.Value.Date : inicio.AddMonths(1).AddDays(-1) },
            new("@cuit", SqlDbType.Bit) { Value = f.PorCuit },
            new("@cuit_like", SqlDbType.VarChar, 15) { Value = f.PorCuit ? f.Cuit! : "%" }];
    }
    public async Task<List<RetencionImpuesto>> Impuestos(CancellationToken ct)
    {
        await using var cn = new SqlConnection(context.Database.GetConnectionString());
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand("dbo.SPGECO_Tipos_Impuestos", cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 60 };
        await using var dr = await cmd.ExecuteReaderAsync(ct);
        var id = dr.GetOrdinal("imp_id"); var texto = dr.GetOrdinal("imp_descripcion"); var agente = dr.GetOrdinal("agente_ret");
        var lista = new List<RetencionImpuesto>();
        while (await dr.ReadAsync(ct))
            lista.Add(new() { Id = dr.GetString(id).TrimEnd(), Texto = dr.GetString(texto).TrimEnd(), AgenteRet = dr.GetString(agente).TrimEnd() });
        return lista;
    }
    public async Task<List<CertificadoRetencionFila>> Consultar(CertificadosRetencionFiltro filtro, CancellationToken ct)
    {
        var parametros = Parametros(filtro);
        using var plazo = CancellationTokenSource.CreateLinkedTokenSource(ct);
        plazo.CancelAfter(TimeSpan.FromMinutes(9));
        await using var cn = new SqlConnection(context.Database.GetConnectionString());
        await cn.OpenAsync(plazo.Token);
        await using var cmd = new SqlCommand(SpConsulta, cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 540 };
        cmd.Parameters.AddRange(parametros);
        await using var dr = await cmd.ExecuteReaderAsync(plazo.Token);
        var columnas = typeof(CertificadoRetencionFila).GetProperties().Select(p => (Propiedad: p, Ordinal: dr.GetOrdinal(p.Name))).ToArray();
        var filas = new List<CertificadoRetencionFila>();
        while (await dr.ReadAsync(plazo.Token))
        {
            var fila = new CertificadoRetencionFila();
            foreach (var c in columnas)
            {
                if (dr.IsDBNull(c.Ordinal)) { c.Propiedad.SetValue(fila, null); continue; }
                var tipo = Nullable.GetUnderlyingType(c.Propiedad.PropertyType) ?? c.Propiedad.PropertyType;
                // Exigir el tipo SQL esperado: no truncar ni redondear importes durante el mapeo.
                var valor = dr.GetValue(c.Ordinal);
                if (!tipo.IsInstanceOfType(valor)) throw new InvalidOperationException($"El contrato de {SpConsulta} cambió en {c.Propiedad.Name}.");
                c.Propiedad.SetValue(fila, valor);
            }
            filas.Add(fila);
        }
        if (await dr.NextResultAsync(plazo.Token)) throw new InvalidOperationException("La consulta devolvió más de un resultado; no se muestra una salida parcial.");
        return filas;
    }
}
