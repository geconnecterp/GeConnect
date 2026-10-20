using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace gc.api.infra.Datos.Implementacion;

public class IvaComprasResumenDatos(GeConnectContext context) : IIvaComprasResumenDatos
{
    public static SqlParameter[] Parametros(IvaComprasResumenFiltro filtro)
    {
        filtro.Validar();
        return [new("@periodo", SqlDbType.VarChar, 4) { Value = filtro.Periodo.ToString("0000", CultureInfo.InvariantCulture) },
            new("@mes", SqlDbType.VarChar, 2) { Value = filtro.Mes.ToString("00", CultureInfo.InvariantCulture) }];
    }

    public async Task<IvaComprasResumenLibro> Consultar(IvaComprasResumenFiltro filtro, CancellationToken ct)
    {
        filtro.Validar();
        // Presupuesto total para las cuatro lecturas; ninguna salida parcial llega al consumidor.
        using var plazo = CancellationTokenSource.CreateLinkedTokenSource(ct);
        plazo.CancelAfter(TimeSpan.FromMinutes(9));
        var token = plazo.Token;
        await using var cn = new SqlConnection(context.Database.GetConnectionString());
        await cn.OpenAsync(token);
        var libro = new IvaComprasResumenLibro { Filtro = filtro };
        libro.Bienes.Control = await Leer<IvaComprasResumenControl>("dbo.SPGECO_IVA_C_Conta_BI", cn, filtro, token);
        libro.Bienes.Resumen = await Leer<IvaComprasResumenConcepto>("dbo.SPGECO_IVA_C_R_BI", cn, filtro, token);
        libro.Otros.Control = await Leer<IvaComprasResumenControl>("dbo.SPGECO_IVA_C_Conta_OT", cn, filtro, token);
        libro.Otros.Resumen = await Leer<IvaComprasResumenConcepto>("dbo.SPGECO_IVA_C_R_OT", cn, filtro, token);
        libro.Validar();
        return libro;
    }

    private static async Task<List<T>> Leer<T>(string sp, SqlConnection cn, IvaComprasResumenFiltro filtro, CancellationToken ct) where T : new()
    {
        await using var cmd = new SqlCommand(sp, cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 540 };
        cmd.Parameters.AddRange(Parametros(filtro));
        await using var dr = await cmd.ExecuteReaderAsync(ct);
        // Las propiedades calculadas no forman parte del contrato SQL.
        var columnas = typeof(T).GetProperties().Where(p => p.CanWrite)
            .Select(p => (Prop: p, Ordinal: dr.GetOrdinal(p.Name))).ToArray();
        var filas = new List<T>();
        while (await dr.ReadAsync(ct))
        {
            var fila = new T();
            foreach (var c in columnas)
            {
                if (dr.IsDBNull(c.Ordinal))
                    throw new InvalidOperationException($"{sp} devolvió NULL en {c.Prop.Name}; no se reemplaza por cero.");
                var valor = Convert.ChangeType(dr.GetValue(c.Ordinal), c.Prop.PropertyType, CultureInfo.InvariantCulture);
                c.Prop.SetValue(fila, valor is string texto ? texto.TrimEnd() : valor);
            }
            filas.Add(fila);
        }
        return filas;
    }
}
