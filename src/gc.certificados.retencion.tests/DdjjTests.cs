using ClosedXML.Excel;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Servicios.Libros;
using gc.api.infra.Datos.Implementacion;
using gc.infraestructura.Dtos.Libros;
using System.Reflection;
using System.Security.Claims;
using System.Text;

public static class DdjjTests
{
    public static async Task Run(Action<bool, string> check, Func<Func<Task>, string, Task> rechazar)
    {
        var hoy = IvaVentasFiltro.HoyArgentina();
        CertificadosRetencionDdjjPedido Pedido(string impuesto) => new() { Impuesto = impuesto, Periodo = hoy.Year, Mes = 2 };
        check(typeof(CertificadosRetencionDdjjPedido).GetProperty("Cuit") == null && typeof(CertificadosRetencionDdjjPedido).GetProperty("Desde") == null, "DDJJ tiene contrato mensual sin filtros de pantalla");
        foreach (var impuesto in new[] { "IB", "IV", "GA" })
        {
            var pedido = Pedido(impuesto); var p = CertificadosRetencionDatos.ParametrosDdjj(pedido);
            check(p.Length == 2 && p[0].ParameterName == "@periodo" && p[1].ParameterName == "@mes" && p[0].Size == 4 && p[1].Size == 2 && (string)p[1].Value == "02", "DDJJ parámetros exactos " + impuesto);
            check(CertificadosRetencionDatos.SpDdjj(impuesto) == "dbo.SPGECO_Imp_AR_DDJJ_" + impuesto, "DDJJ SP en whitelist " + impuesto);
            check(pedido.Nombre.EndsWith(impuesto == "GA" ? ".txt" : ".xlsx"), "DDJJ extensión " + impuesto);
        }
        var datos = new DatosFake(); var usuarios = DispatchProxy.Create<IApiUsuarioServicio, UsuarioFake>();
        var proxy = (UsuarioFake)(object)usuarios; var servicio = new CertificadosRetencionServicio(datos, usuarios);
        var usuario = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user", "prueba"), new Claim("AdmId", "0000#Central"), new Claim("perfiles", "[{\"perfil_id\":\"P1\"}]")], "test"));
        using var destino = new MemoryStream();
        try { await servicio.GenerarDdjj(Pedido("IB"), usuario, "P1", "M", destino, default); throw new Exception("DDJJ sin permiso aceptada"); }
        catch (UnauthorizedAccessException) { check(datos.QueryCalls == 0 && datos.OptionsCalls == 0 && destino.Length == 0, "DDJJ imp_ar obligatorio antes de acceder a datos"); }
        proxy.Allow = true; datos.ImpuestosActivos = false;
        try { await servicio.GenerarDdjj(Pedido("IB"), usuario, "P1", "M", destino, default); throw new Exception("DDJJ no agente aceptada"); }
        catch (ArgumentException) { check(datos.QueryCalls == 0, "DDJJ verifica agente_ret S en servidor"); }
        datos.ImpuestosActivos = true;
        datos.Falla = true;
        try { await servicio.GenerarDdjj(Pedido("GA"), usuario, "P1", "M", destino, default); throw new Exception("Fallo DDJJ ignorado"); }
        catch (IOException) { check(true, "Fallo SQL DDJJ no se convierte en una descarga exitosa"); }
        datos.Falla = false;
        foreach (var impuesto in new[] { "IB", "IV" })
        {
            using var stream = new MemoryStream();
            await servicio.GenerarDdjj(Pedido(impuesto), usuario, "P1", "M", stream, default);
            stream.Position = 0; using var libro = new XLWorkbook(stream); var hoja = libro.Worksheet(1);
            check(hoja.LastRowUsed()!.RowNumber() == 2 && hoja.LastColumnUsed()!.ColumnNumber() == 9, "Excel DDJJ nueve columnas " + impuesto);
            check(hoja.Cell(2, impuesto == "IB" ? 5 : 1).GetString() == "00001", "Excel conserva ceros iniciales " + impuesto);
            check(hoja.Cell(2, impuesto == "IB" ? 6 : 5).DataType == XLDataType.DateTime && hoja.Cell(2, impuesto == "IB" ? 7 : 6).GetValue<decimal>() == 9999999999999.99m, "Excel fecha e importe grande tipados " + impuesto);
            check(hoja.Cell(2, impuesto == "IB" ? 9 : 7).GetValue<decimal>() == -2.01m, "Excel conserva importes negativos " + impuesto);
            if (impuesto == "IB")
            {
                check(hoja.Cell(2, 2).GetString() == "00123" && hoja.Cell(2, 4).GetString() == "00999", "Excel IB no colapsa columnas duplicadas del SP");
                check(hoja.Cell(1, 2).GetString() == "CUIT" && hoja.Cell(1, 4).GetString() == "CUIT (2C)", "Excel IB segunda cabecera CUIT (2C), sin cambiar la primera");
            }
            check(!hoja.ShowGridLines && hoja.SheetView.SplitRow == 1, "Excel cabecera inmovilizada y estilo " + impuesto);
        }
        static async IAsyncEnumerable<object?[]> Filas(params object?[][] filas) { foreach (var f in filas) { await Task.Yield(); yield return f; } }
        using var txt = new MemoryStream();
        const string linea = "06 000000001  -2.01";
        await CertificadosRetencionDdjjArchivos.Escribir(txt, "GA", Filas([linea + new string(' ', 100)], ["03 00002  "]), default);
        check(Encoding.UTF8.GetString(txt.ToArray()) == linea + new string(' ',100) + "\r\n03 00002  \r\n", "GA sin encabezado ni BOM, conserva espacios, signos y CRLF");
        foreach (var f in new object?[][] { [null], [""], ["1\n2"] })
            await rechazar(() => CertificadosRetencionDdjjArchivos.Escribir(new MemoryStream(), "GA", Filas(f), default), "GA línea nula/vacía/partida no se entrega");
        foreach (var imp in new[] { "GA", "IB", "IV" })
            await rechazar(() => CertificadosRetencionDdjjArchivos.Escribir(new MemoryStream(), imp, Filas(), default), "DDJJ sin registros da aviso " + imp);
        static async IAsyncEnumerable<object?[]> FallaTardia()
        { yield return ["06 0001  "]; await Task.Yield(); throw new IOException("Lectura interrumpida"); }
        try { await CertificadosRetencionDdjjArchivos.Escribir(new MemoryStream(), "GA", FallaTardia(), default); throw new Exception("Fallo tardío ignorado"); }
        catch (IOException) { check(true, "Fallo tardío GA se propaga y el endpoint descarta el temporal"); }
        foreach (var fila in new object?[][] { ["columna extra", "incorrecta"], [1m] })
        {
            try { await CertificadosRetencionDdjjArchivos.Escribir(new MemoryStream(), "GA", Filas(fila), default); throw new Exception("Cambio de contrato aceptado"); }
            catch (InvalidOperationException) { check(true, "DDJJ rechaza cambio de columnas o tipos"); }
        }
        static async IAsyncEnumerable<object?[]> Muchas(string impuesto)
        {
            for (var n = 0; n < 5001; n++) { if (n % 1000 == 0) await Task.Yield(); yield return impuesto == "GA" ? ["06 " + n + "  "] : ["=NO_ES_FORMULA()", "00123", "Dom", "00999", n.ToString("000000"), new DateTime(2025,11,1), 1.01m, null, -0.01m]; }
        }
        using var completo = new MemoryStream();
        await CertificadosRetencionDdjjArchivos.Escribir(completo, "IB", Muchas("IB"), default); completo.Position = 0;
        using (var wb = new XLWorkbook(completo))
        {
            var h = wb.Worksheet(1);
            check(h.LastRowUsed()!.RowNumber() == 5002 && h.Cell(5002,5).GetString() == "005000", "Excel completo supera 5000 sin paginar ni truncar");
            check(!h.Cell(2,1).HasFormula && h.Cell(2,1).GetString() == "=NO_ES_FORMULA()" && h.Cell(2,8).IsEmpty(), "Texto no se ejecuta como fórmula y NULL permanece vacío");
        }
        using var txtGrande = new MemoryStream();
        await CertificadosRetencionDdjjArchivos.Escribir(txtGrande, "GA", Muchas("GA"), default);
        check(Encoding.UTF8.GetString(txtGrande.ToArray()).Split("\r\n").Length == 5002, "TXT completo de más de 5000 líneas");
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        try { await CertificadosRetencionDdjjArchivos.Escribir(new MemoryStream(), "IB", Muchas("IB"), cancel.Token); throw new Exception("Cancelación ignorada"); }
        catch (OperationCanceledException) { check(true, "DDJJ cancelada no completa exportación"); }
    }
}
