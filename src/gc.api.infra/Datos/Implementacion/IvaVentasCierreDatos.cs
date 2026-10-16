using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace gc.api.infra.Datos.Implementacion;

public class IvaVentasCierreDatos(GeConnectContext context) : IIvaVentasCierreDatos
{
    private SqlConnection Conexion() => new(context.Database.GetConnectionString());
    private static SqlCommand Sp(string nombre, SqlConnection cn, SqlTransaction? tx = null) =>
        new(nombre, cn, tx) { CommandType = CommandType.StoredProcedure, CommandTimeout = 600 };
    public static SqlParameter[] ParametrosPeriodo(string periodo)
    {
        IvaVentasCierrePeriodo.Validar(periodo);
        return [new("@periodo", SqlDbType.VarChar, 4) { Value = periodo[..4] }, new("@mes", SqlDbType.VarChar, 2) { Value = periodo[4..] }];
    }
    private static async Task<string> Proximo(SqlConnection cn, SqlTransaction? tx, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("SELECT dbo.SF_Libro_IVA_Periodo(@libro)", cn, tx) { CommandTimeout = 60 };
        cmd.Parameters.Add("@libro", SqlDbType.Char, 1).Value = "V";
        var periodo = (await cmd.ExecuteScalarAsync(ct)) as string;
        try { IvaVentasCierrePeriodo.Validar(periodo); }
        catch (ArgumentException ex) { throw new InvalidOperationException("La función SQL no devolvió un período abierto válido.", ex); }
        return periodo!;
    }
    public async Task<IvaVentasCierreEstado> Consultar(CancellationToken ct)
    {
        await using var cn = Conexion(); await cn.OpenAsync(ct);
        var estado = new IvaVentasCierreEstado();
        await using (var cmd = Sp("dbo.SPGECO_IVA_V_Libros", cn))
        await using (var dr = await cmd.ExecuteReaderAsync(ct))
        {
            var columnas = typeof(IvaVentasCierreFila).GetProperties().Where(p => p.CanWrite)
                .Select(p => (Prop: p, Ordinal: dr.GetOrdinal(p.Name))).ToArray();
            while (await dr.ReadAsync(ct))
            {
                var fila = new IvaVentasCierreFila();
                foreach (var c in columnas)
                {
                    if (dr.IsDBNull(c.Ordinal)) throw new InvalidOperationException($"El historial devolvió NULL en {c.Prop.Name}.");
                    var value = Convert.ChangeType(dr.GetValue(c.Ordinal), c.Prop.PropertyType, CultureInfo.InvariantCulture);
                    c.Prop.SetValue(fila, value is string s ? s.TrimEnd() : value);
                }
                estado.Libros.Add(fila);
            }
        }
        estado.ProximoPeriodo = await Proximo(cn, null, ct);
        estado.Validar();
        return estado;
    }
    public async Task<IvaVentasCierreRespuesta> Cerrar(string periodoEsperado, string usuario, string sucursal, CancellationToken ct)
    {
        IvaVentasCierrePeriodo.Validar(periodoEsperado);
        if (sucursal != "0000" || string.IsNullOrWhiteSpace(usuario) || usuario.Length > 10)
            throw new UnauthorizedAccessException("El contexto de sesión no permite cerrar el libro.");
        await using var cn = Conexion(); await cn.OpenAsync(ct);
        await using var tx = (SqlTransaction)await cn.BeginTransactionAsync(ct);
        try
        {
            // Lock SQL por base, compartido por instancias de la API. No altera objetos del DBA.
            // El INSERT del SP mantiene además la PK como defensa frente a otros consumidores.
            await using (var gate = new SqlCommand("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=@recurso, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=0; SELECT @r;", cn, tx) { CommandTimeout = 60 })
            {
                gate.Parameters.Add("@recurso", SqlDbType.NVarChar, 255).Value = "GECO:IVA:V:Cierre";
                if (Convert.ToInt32(await gate.ExecuteScalarAsync(ct), CultureInfo.InvariantCulture) < 0)
                    throw new IvaVentasCierreConflictoException("Hay otro cierre en proceso. Actualice el historial antes de volver a intentar.");
            }
            if (await Proximo(cn, tx, ct) != periodoEsperado)
                throw new IvaVentasCierreConflictoException("El período abierto cambió. Actualice el historial y revise el próximo cierre.");
            IvaVentasCierreRespuesta respuesta;
            await using (var cmd = Sp("dbo.SPGECO_IVA_V_Cerrar", cn, tx))
            {
                cmd.Parameters.AddRange(ParametrosPeriodo(periodoEsperado));
                cmd.Parameters.Add("@usu_id", SqlDbType.VarChar, 10).Value = usuario;
                cmd.Parameters.Add("@adm_id", SqlDbType.VarChar, 4).Value = sucursal;
                await using var dr = await cmd.ExecuteReaderAsync(ct);
                if (!await dr.ReadAsync(ct) || dr.IsDBNull(dr.GetOrdinal("resultado")))
                    throw new InvalidOperationException("El cierre no devolvió una respuesta válida.");
                respuesta = new() { resultado = Convert.ToInt16(dr["resultado"], CultureInfo.InvariantCulture),
                    resultado_msj = dr["resultado_msj"] as string ?? "El cierre no informó un mensaje." };
                if (await dr.ReadAsync(ct)) throw new InvalidOperationException("El cierre devolvió más de una respuesta.");
            }
            // El COMMIT interno del SP no confirma nuestra transacción externa.
            if (respuesta.resultado == 0) await tx.CommitAsync(ct);
            else await Rollback(tx);
            return respuesta;
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627 or 1205)
        {
            await Rollback(tx);
            throw new IvaVentasCierreConflictoException("Otro proceso intervino sobre el cierre. Actualice el historial para comprobar su estado.");
        }
        catch { await Rollback(tx); throw; }
    }
    private static async Task Rollback(SqlTransaction tx)
    {
        try { if (tx.Connection != null) await tx.RollbackAsync(CancellationToken.None); }
        catch (InvalidOperationException) { /* El SP puede haber revertido también la transacción externa. */ }
        catch (SqlException) { /* Conexión rota: Dispose libera la transacción; preservar el error original. */ }
    }
    public async Task<bool> ExisteCierre(string periodo, CancellationToken ct)
    {
        IvaVentasCierrePeriodo.Validar(periodo);
        await using var cn = Conexion(); await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.comptes_vtas_iva_mensual WHERE periodo=@p) THEN 1 ELSE 0 END", cn) { CommandTimeout = 60 };
        cmd.Parameters.Add("@p", SqlDbType.Char, 6).Value = periodo;
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct), CultureInfo.InvariantCulture) == 1;
    }
    public async IAsyncEnumerable<string?> LineasRg3685(string periodo, char tipo, [EnumeratorCancellation] CancellationToken ct)
    {
        if (tipo is not ('C' or 'D')) throw new ArgumentException("El tipo de archivo debe ser C o D.");
        await using var cn = Conexion(); await cn.OpenAsync(ct);
        await using var cmd = Sp("dbo.SPGECO_IVA_V_3685", cn);
        cmd.Parameters.AddRange(ParametrosPeriodo(periodo));
        cmd.Parameters.Add("@tipo", SqlDbType.Char, 1).Value = tipo.ToString();
        await using var dr = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
        if (dr.FieldCount != 1) throw new InvalidOperationException("RG 3685 debe devolver una sola columna de texto.");
        while (await dr.ReadAsync(ct)) yield return dr.IsDBNull(0) ? null : dr.GetString(0);
    }
}
