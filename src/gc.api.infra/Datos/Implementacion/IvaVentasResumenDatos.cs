using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace gc.api.infra.Datos.Implementacion;

public class IvaVentasResumenDatos(GeConnectContext context) : IIvaVentasResumenDatos
{
    public static SqlParameter[] Parametros(IvaVentasResumenFiltro filtro)
    {
        filtro.Validar();
        return [new("@periodo", SqlDbType.VarChar, 4) { Value = filtro.Periodo.ToString("0000", CultureInfo.InvariantCulture) },
            new("@mes", SqlDbType.VarChar, 2) { Value = filtro.Mes.ToString("00", CultureInfo.InvariantCulture) }];
    }
    public async Task<List<IvaVentasResumenFila>> Consultar(IvaVentasResumenFiltro filtro, CancellationToken ct)
    {
        await using var cn = new SqlConnection(context.Database.GetConnectionString());
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand("dbo.SPGECO_IVA_V_R", cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 600 };
        cmd.Parameters.AddRange(Parametros(filtro));
        await using var dr = await cmd.ExecuteReaderAsync(ct);
        var columnas = typeof(IvaVentasResumenFila).GetProperties().Where(p => p.CanWrite)
            .Select(p => (Prop: p, Ordinal: dr.GetOrdinal(p.Name), Tipo: Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType)).ToArray();
        var filas = new List<IvaVentasResumenFila>();
        while (await dr.ReadAsync(ct))
        {
            var fila = new IvaVentasResumenFila();
            foreach (var c in columnas)
            {
                if (dr.IsDBNull(c.Ordinal))
                {
                    if (c.Prop.Name == "ali") continue;
                    throw new InvalidOperationException($"El SP de resumen devolvió NULL en {c.Prop.Name}.");
                }
                var valor = Convert.ChangeType(dr.GetValue(c.Ordinal), c.Tipo, CultureInfo.InvariantCulture);
                c.Prop.SetValue(fila, valor is string texto ? texto.TrimEnd() : valor);
            }
            filas.Add(fila);
        }
        return filas;
    }
}
