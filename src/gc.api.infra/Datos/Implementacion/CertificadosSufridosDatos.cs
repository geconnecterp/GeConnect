using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace gc.api.infra.Datos.Implementacion;

public class CertificadosSufridosDatos(GeConnectContext context) : ICertificadosSufridosDatos
{
    public const string SpConsulta = "dbo.SPGECO_Imp_Reten_Percep";
    public const string SpPorCuit = "dbo.SPGECO_Imp_Reten_Percep_x_Cuit";
    public static SqlParameter[] Parametros(CertificadosSufridosFiltro f)
    {
        f.Validar(); var inicio = new DateTime(f.Periodo, f.Mes, 1);
        return [new("@imp_id", SqlDbType.Char, 2) { Value = f.Impuesto },
            new("@periodo", SqlDbType.VarChar, 4) { Value = f.Periodo.ToString("0000", CultureInfo.InvariantCulture) },
            new("@mes", SqlDbType.VarChar, 2) { Value = f.Mes.ToString("00", CultureInfo.InvariantCulture) },
            new("@f", SqlDbType.Bit) { Value = f.Fechas },
            new("@fecha_d", SqlDbType.DateTime) { Value = f.Fechas ? f.Desde!.Value.Date : inicio },
            new("@fecha_h", SqlDbType.DateTime) { Value = f.Fechas ? f.Hasta!.Value.Date : inicio.AddMonths(1).AddDays(-1) },
            new("@cuit", SqlDbType.Bit) { Value = f.PorCuit },
            new("@cuit_like", SqlDbType.VarChar, 15) { Value = f.PorCuit ? f.Cuit! : "%" }];
    }
    public static Func<DbDataReader, CertificadoSufridoFila> CrearMapeador(DbDataReader dr)
        => CrearMapeador<CertificadoSufridoFila>(dr, SpConsulta);
    public static Func<DbDataReader, CertificadoSufridoPorCuitFila> CrearMapeadorPorCuit(DbDataReader dr)
        => CrearMapeador<CertificadoSufridoPorCuitFila>(dr, SpPorCuit);
    private static Func<DbDataReader, T> CrearMapeador<T>(DbDataReader dr, string sp) where T : new()
    {
        var propiedades = typeof(T).GetProperties();
        if (dr.FieldCount != propiedades.Length) throw new InvalidOperationException($"{sp} devolvió una cantidad de columnas distinta al contrato de {propiedades.Length} alias.");
        var mapa = propiedades.Select(p => (Propiedad: p, Ordinal: dr.GetOrdinal(p.Name))).ToArray();
        foreach (var c in mapa)
        {
            var tipo = Nullable.GetUnderlyingType(c.Propiedad.PropertyType) ?? c.Propiedad.PropertyType;
            if (dr.GetFieldType(c.Ordinal) != tipo) throw new InvalidOperationException($"El contrato de {sp} difiere en {c.Propiedad.Name}.");
        }
        return reader =>
        {
            var fila = new T();
            foreach (var c in mapa) c.Propiedad.SetValue(fila, reader.IsDBNull(c.Ordinal) ? null : reader.GetValue(c.Ordinal));
            return fila;
        };
    }
    public async Task<List<SufridoImpuesto>> Impuestos(CancellationToken ct)
    {
        await using var cn = new SqlConnection(context.Database.GetConnectionString());
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand("dbo.SPGECO_Tipos_Impuestos", cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 60 };
        await using var dr = await cmd.ExecuteReaderAsync(ct);
        var id = dr.GetOrdinal("imp_id"); var texto = dr.GetOrdinal("imp_descripcion"); var cont = dr.GetOrdinal("cont");
        var lista = new List<SufridoImpuesto>();
        while (await dr.ReadAsync(ct)) lista.Add(new() { Id = dr.GetString(id).TrimEnd(), Texto = dr.GetString(texto).TrimEnd(), Contribuyente = dr.GetString(cont).TrimEnd() });
        if (await dr.NextResultAsync(ct)) throw new InvalidOperationException("El catálogo devolvió resultados adicionales.");
        return lista;
    }
    public async Task<List<CertificadoSufridoFila>> Consultar(CertificadosSufridosFiltro filtro, CancellationToken ct)
    {
        if (filtro.AgrupadoPorCuit) throw new ArgumentException("El modo agrupado requiere el SP x_Cuit y su contrato específico.");
        return await Leer<CertificadoSufridoFila>(SpConsulta, filtro, CrearMapeador, ct);
    }
    public async Task<List<CertificadoSufridoPorCuitFila>> ConsultarPorCuit(CertificadosSufridosFiltro filtro, CancellationToken ct)
    {
        if (!filtro.AgrupadoPorCuit) throw new ArgumentException("El modo detallado requiere el SP de detalle.");
        return await Leer<CertificadoSufridoPorCuitFila>(SpPorCuit, filtro, CrearMapeadorPorCuit, ct);
    }
    private async Task<List<T>> Leer<T>(string sp, CertificadosSufridosFiltro filtro, Func<DbDataReader, Func<DbDataReader, T>> crearMapeador, CancellationToken ct)
    {
        var parametros = Parametros(filtro);
        await using var cn = new SqlConnection(context.Database.GetConnectionString());
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sp, cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 540 };
        cmd.Parameters.AddRange(parametros);
        await using var dr = await cmd.ExecuteReaderAsync(ct);
        var mapear = crearMapeador(dr); var filas = new List<T>();
        while (await dr.ReadAsync(ct)) filas.Add(mapear(dr));
        if (await dr.NextResultAsync(ct)) throw new InvalidOperationException("El SP devolvió resultados adicionales; no se muestra una salida parcial.");
        return filas;
    }
}
