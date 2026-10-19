using gc.api.core.Contratos.Servicios.Libros;
using gc.api.infra.Datos;
using gc.infraestructura.Dtos.Libros;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace gc.api.infra.Datos.Implementacion;

public class IvaComprasDatos(GeConnectContext context) : IIvaComprasDatos
{
    // Se reutiliza la conexión configurada de GECO. No se registran filtros fiscales ni comandos SQL.
    private SqlConnection Conexion() => new(context.Database.GetConnectionString());

    public static List<SqlParameter> Parametros(IvaComprasFiltro f, bool completo)
    {
        f.Validar(completo);
        return
        [
            new("@periodo", SqlDbType.VarChar, 4) { Value = f.Periodo.ToString("0000", CultureInfo.InvariantCulture) },
            new("@mes", SqlDbType.VarChar, 2) { Value = f.Mes.ToString("00", CultureInfo.InvariantCulture) },
            new("@f", SqlDbType.Bit) { Value = f.Fechas },
            new("@fecha_d", SqlDbType.DateTime) { Value = f.Fechas ? f.Desde!.Value.Date : new DateTime(f.Periodo, f.Mes, 1) },
            new("@fecha_h", SqlDbType.DateTime) { Value = f.Fechas ? f.Hasta!.Value.Date : new DateTime(f.Periodo, f.Mes, DateTime.DaysInMonth(f.Periodo, f.Mes)) },
            new("@cuit", SqlDbType.Bit) { Value = f.PorCuit },
            new("@cuit_like", SqlDbType.VarChar, 15) { Value = f.PorCuit ? f.Cuit! : "%" },
            new("@tco", SqlDbType.Bit) { Value = f.PorComprobante },
            new("@tco_list", SqlDbType.VarChar, -1) { Value = f.PorComprobante ? string.Join(",", f.Comprobantes.Distinct()) : "" },
            new("@ope", SqlDbType.Bit) { Value = f.PorTipoProveedor },
            new("@ope_list", SqlDbType.VarChar, -1) { Value = f.PorTipoProveedor ? string.Join(",", f.TiposProveedor.Distinct()) : "" },
            new("@registros", SqlDbType.Int) { Value = completo ? int.MaxValue : f.Registros },
            new("@pagina", SqlDbType.Int) { Value = completo ? 1 : f.Pagina },
            new("@ordenar", SqlDbType.VarChar, 30) { Value = "cm_fecha" }
        ];
    }

    public async Task<List<IvaComprasDetalleDto>> Consultar(IvaComprasFiltro filtro, bool completo, CancellationToken ct)
    {
        await using var cn = Conexion();
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand("dbo.SPGECO_IVA_C_D", cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 600 };
        cmd.Parameters.AddRange(Parametros(filtro, completo).ToArray());
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        // Resolver el contrato una vez; no consultar GetSchemaTable por cada celda.
        var propiedades = typeof(IvaComprasDetalleDto).GetProperties();
        var columnas = propiedades.Select(p => (Prop: p, Ordinal: reader.GetOrdinal(p.Name), Tipo: Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType)).ToArray();
        var filas = new List<IvaComprasDetalleDto>();
        while (await reader.ReadAsync(ct))
        {
            var fila = new IvaComprasDetalleDto();
            foreach (var c in columnas)
            {
                if (!reader.IsDBNull(c.Ordinal))
                    c.Prop.SetValue(fila, ConvertirValor(c.Prop.Name, reader.GetValue(c.Ordinal), c.Tipo));
            }
            filas.Add(fila);
        }
        if (await reader.NextResultAsync(ct)) throw new InvalidOperationException("El SP devolvió resultados adicionales fuera del contrato.");
        return filas;
    }

    public static object? ConvertirValor(string campo, object? valor, Type tipo)
    {
        if (valor is null or DBNull) return null;
        try
        {
            if (tipo == typeof(DateTime))
            {
                if (valor is DateTime fechaSql) return fechaSql;
                // CAE opcional: conservar NULL y ausencia textual; rechazar fechas ambiguas.
                if (campo == "cm_cae_vto" && valor is string ausente && string.IsNullOrWhiteSpace(ausente)) return null;
                // cm_cae_vto puede ser CHAR(8). No usar cultura ni
                // conversiones ambiguas (dd/MM vs MM/dd), ni inventar fechas.
                if (valor is string texto && DateTime.TryParseExact(texto.Trim(), "yyyyMMdd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha)) return fecha;
                throw new IvaComprasDatoInvalidoException(campo);
            }
            return Convert.ChangeType(valor, tipo, CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        {
            // No adjuntar la excepción original: podría contener valores fiscales.
            throw new IvaComprasDatoInvalidoException(campo);
        }
    }

    public async Task<IvaComprasOpciones> Opciones(CancellationToken ct) => new()
    {
        Comprobantes = await Lista("dbo.SPGECO_Tipos_Comprobantes", "tco_id", ["tco_desc_libro", "tco_desc"], [new("@afip_id", "%"), new("@opt_id", "LB")], ct),
        TiposProveedor = await Lista("dbo.SPGECO_Tipos_Ope_IVA", "ope_iva", ["ope_iva_descripcion"], [], ct)
    };

    private async Task<List<IvaOpcion>> Lista(string sp, string campoId, string[] descripciones, SqlParameter[] parametros, CancellationToken ct)
    {
        await using var cn = Conexion();
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sp, cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 120 };
        cmd.Parameters.AddRange(parametros);
        await using var dr = await cmd.ExecuteReaderAsync(ct);
        var nombres = Enumerable.Range(0, dr.FieldCount).Select(dr.GetName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var id = dr.GetOrdinal(campoId);
        var desc = descripciones.FirstOrDefault(nombres.Contains);
        var resultado = new List<IvaOpcion>();
        while (await dr.ReadAsync(ct))
        {
            var clave = Convert.ToString(dr.GetValue(id), CultureInfo.InvariantCulture)?.Trim() ?? "";
            if (clave.Length == 0) continue;
            var etiqueta = desc == null || dr[desc] is DBNull ? clave : Convert.ToString(dr[desc])?.Trim();
            resultado.Add(new(clave, etiqueta == clave ? clave : $"{etiqueta} ({clave})"));
        }
        return resultado.DistinctBy(x => x.Id).ToList();
    }
}
