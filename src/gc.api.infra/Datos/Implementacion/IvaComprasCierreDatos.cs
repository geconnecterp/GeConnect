using gc.api.core.Contratos.Servicios.Libros;
using gc.infraestructura.Dtos.Libros;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace gc.api.infra.Datos.Implementacion;

public class IvaComprasCierreDatos(GeConnectContext context) : IIvaComprasCierreDatos
{
    private SqlConnection Conexion() => new(context.Database.GetConnectionString());
    private static SqlCommand Sp(string nombre, SqlConnection cn, SqlTransaction? tx = null) =>
        new(nombre, cn, tx) { CommandType = CommandType.StoredProcedure, CommandTimeout = 600 };

    public static SqlParameter[] ParametrosPeriodo(string periodo)
    {
        IvaComprasCierrePeriodo.Validar(periodo);
        return [new("@periodo", SqlDbType.VarChar, 4) { Value = periodo[..4] },
            new("@mes", SqlDbType.VarChar, 2) { Value = periodo[4..] }];
    }

    private static async Task<string> Proximo(SqlConnection cn, SqlTransaction? tx, CancellationToken ct)
    {
        await using var cmd = new SqlCommand("SELECT dbo.SF_Libro_IVA_Periodo(@libro)", cn, tx) { CommandTimeout = 60 };
        cmd.Parameters.Add("@libro", SqlDbType.Char, 1).Value = "C";
        var periodo = (await cmd.ExecuteScalarAsync(ct)) as string;
        try { IvaComprasCierrePeriodo.Validar(periodo); }
        catch (ArgumentException ex) { throw new InvalidOperationException("La función SQL no devolvió un período abierto válido.", ex); }
        return periodo!;
    }

    // Nombres y nulabilidad del sp_help de Compras. No convertir NULL admitidos en cero.
    public static IvaComprasCierreFila LeerFila(DbDataReader dr)
    {
        string Texto(string columna)
        {
            var i = dr.GetOrdinal(columna);
            if (dr.IsDBNull(i)) throw new InvalidOperationException("El historial devolvió NULL en " + columna + ".");
            return dr.GetString(i).TrimEnd();
        }
        decimal Importe(string columna)
        {
            var i = dr.GetOrdinal(columna);
            if (dr.IsDBNull(i)) throw new InvalidOperationException("El historial devolvió NULL en " + columna + ".");
            return dr.GetDecimal(i);
        }
        var cantidad = dr.GetOrdinal("cant_registros");
        var cuit = dr.GetOrdinal("cuit");
        return new()
        {
            tipo_registro = Texto("tipo_registro"), periodo = Texto("periodo"),
            cant_registros = dr.IsDBNull(cantidad) ? null : dr.GetInt32(cantidad),
            cuit = dr.IsDBNull(cuit) ? null : dr.GetString(cuit),
            importe = Importe("importe"), importe_gravado = Importe("importe_gravado"),
            importe_no_gravado = Importe("importe_no_gravado"), importe_exento = Importe("importe_exento"),
            importe_iva = Importe("importe_iva"), percep_imp_nacionales = Importe("percep_imp_nacionales"),
            percep_ib = Importe("percep_ib"), percep_municipales = Importe("percep_municipales"),
            importe_imp_internos = Importe("importe_imp_internos"),
            importe_otro_ng = Importe("importe_otro_ng"), importe_mt = Importe("importe_mt")
        };
    }

    public async Task<IvaComprasCierreEstado> Consultar(CancellationToken ct)
    {
        await using var cn = Conexion(); await cn.OpenAsync(ct);
        var estado = new IvaComprasCierreEstado();
        await using (var cmd = Sp("dbo.SPGECO_IVA_C_Libros", cn))
        await using (var dr = await cmd.ExecuteReaderAsync(ct))
        {
            while (await dr.ReadAsync(ct))
            {
                estado.Libros.Add(LeerFila(dr));
                if (estado.Libros.Count > 72) throw new InvalidOperationException("El historial excedió el contrato de 72 registros.");
            }
            if (await dr.NextResultAsync(ct)) throw new InvalidOperationException("El historial devolvió más de un resultado.");
        }
        estado.ProximoPeriodo = await Proximo(cn, null, ct);
        estado.Validar();
        return estado;
    }

    public async Task<IvaComprasCierreRespuesta> Cerrar(string periodoEsperado, string usuario, string sucursal, CancellationToken ct)
    {
        IvaComprasCierrePeriodo.Validar(periodoEsperado);
        if (sucursal != "0000" || string.IsNullOrWhiteSpace(usuario) || usuario.Length > 10)
            throw new UnauthorizedAccessException("El contexto de sesión no permite cerrar el libro.");
        await using var cn = Conexion(); await cn.OpenAsync(ct);
        await using var tx = (SqlTransaction)await cn.BeginTransactionAsync(ct);
        try
        {
            // Exclusión por base y libro entre instancias que comparten esta convención.
            await using (var gate = new SqlCommand("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=@recurso, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=0; SELECT @r;", cn, tx) { CommandTimeout = 60 })
            {
                gate.Parameters.Add("@recurso", SqlDbType.NVarChar, 255).Value = "GECO:IVA:C:Cierre";
                if (Convert.ToInt32(await gate.ExecuteScalarAsync(ct), CultureInfo.InvariantCulture) < 0)
                    throw new IvaComprasCierreConflictoException("Hay otro cierre en proceso. Actualice el historial antes de volver a intentar.");
            }
            if (await Proximo(cn, tx, ct) != periodoEsperado)
                throw new IvaComprasCierreConflictoException("El período abierto cambió. Actualice el historial y revise el próximo cierre.");
            IvaComprasCierreRespuesta respuesta;
            await using (var cmd = Sp("dbo.SPGECO_IVA_C_Cerrar", cn, tx))
            {
                cmd.Parameters.AddRange(ParametrosPeriodo(periodoEsperado));
                cmd.Parameters.Add("@usu_id", SqlDbType.VarChar, 10).Value = usuario;
                cmd.Parameters.Add("@adm_id", SqlDbType.VarChar, 4).Value = sucursal;
                await using var dr = await cmd.ExecuteReaderAsync(ct);
                if (!await dr.ReadAsync(ct) || dr.IsDBNull(dr.GetOrdinal("resultado")) || dr.IsDBNull(dr.GetOrdinal("resultado_msj")))
                    throw new InvalidOperationException("El cierre no devolvió una respuesta válida.");
                respuesta = new() { resultado = dr.GetInt16(dr.GetOrdinal("resultado")),
                    resultado_msj = dr.GetString(dr.GetOrdinal("resultado_msj")) };
                if (await dr.ReadAsync(ct) || await dr.NextResultAsync(ct) || string.IsNullOrWhiteSpace(respuesta.resultado_msj))
                    throw new InvalidOperationException("El cierre devolvió una respuesta inesperada.");
            }
            // COMMIT interno no confirma la transacción externa. ROLLBACK interno puede revertirla.
            if (respuesta.resultado == 0) await tx.CommitAsync(ct);
            else await Rollback(tx);
            return respuesta;
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627 or 1205)
        {
            await Rollback(tx);
            throw new IvaComprasCierreConflictoException("Otro proceso intervino sobre el cierre. Actualice el historial para comprobar su estado.");
        }
        catch { await Rollback(tx); throw; }
    }

    private static async Task Rollback(SqlTransaction tx)
    {
        try { if (tx.Connection != null) await tx.RollbackAsync(CancellationToken.None); }
        catch (InvalidOperationException) { /* El SP pudo revertir también la transacción externa. */ }
        catch (SqlException) { /* Preservar el error original; Dispose libera la conexión. */ }
    }

    public async Task<bool> ExisteCierre(string periodo, string tipoRegistro, CancellationToken ct)
    {
        new IvaComprasCierreDescargaPedido { Periodo = periodo, TipoRegistro = tipoRegistro }.Validar();
        await using var cn = Conexion(); await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand("SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.comptes_comp_iva_mensual WHERE periodo=@p AND tipo_registro=@t) THEN 1 ELSE 0 END", cn) { CommandTimeout = 60 };
        cmd.Parameters.Add("@p", SqlDbType.Char, 6).Value = periodo;
        cmd.Parameters.Add("@t", SqlDbType.Char, 1).Value = tipoRegistro;
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct), CultureInfo.InvariantCulture) == 1;
    }

    public async IAsyncEnumerable<string?> LineasRg3685(string periodo, char tipo, [EnumeratorCancellation] CancellationToken ct)
    {
        if (tipo is not ('C' or 'D')) throw new ArgumentException("El tipo de archivo debe ser C o D.");
        await using var cn = Conexion(); await cn.OpenAsync(ct);
        await using var cmd = Sp("dbo.SPGECO_IVA_C_3685", cn);
        cmd.Parameters.AddRange(ParametrosPeriodo(periodo));
        cmd.Parameters.Add("@tipo", SqlDbType.Char, 1).Value = tipo.ToString();
        await using var dr = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
        if (dr.FieldCount != 1 || dr.GetFieldType(0) != typeof(string))
            throw new IvaComprasCierreArchivoException("RG 3685 debe devolver una sola columna de texto. Solicite revisar el contrato del SP.");
        while (await dr.ReadAsync(ct)) yield return dr.IsDBNull(0) ? null : dr.GetString(0);
        if (await dr.NextResultAsync(ct))
            throw new IvaComprasCierreArchivoException("RG 3685 devolvió más de un resultado. No se entregó un archivo parcial.");
    }
}

