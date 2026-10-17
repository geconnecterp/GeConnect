using System.IO.Compression;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Servicios.Libros;
using gc.api.infra.Datos.Implementacion;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using gc.sitio.core.Servicios.Implementacion.Libros;
using Newtonsoft.Json;

if (args.SequenceEqual(new[] { "--sql-lectura" })) { await IvaVentasCierreSqlLectura.Run(); return; }

// Pruebas sintéticas: no se abre ninguna conexión SQL ni se cierra un libro real.
int passed = 0;
void Check(bool ok, string name) { if (!ok) throw new Exception("FAIL: " + name); Console.WriteLine("OK: " + name); passed++; }
async Task Reject<T>(Func<Task> action, string name) where T : Exception
{ try { await action(); } catch (T) { Check(true, name); return; } throw new Exception("FAIL: " + name); }
foreach (var p in new[] { "202512", "202601", "200002", "202511" }) { IvaVentasCierrePeriodo.Validar(p); Check(IvaVentasCierrePeriodo.Texto(p) == p[..4] + "-" + p[4..], "Período válido " + p); }
foreach (var p in new[] { "", "202500", "202513", "2025-11", "２０２５１１", "202511 ", "000001", "2025'1" })
    await Reject<ArgumentException>(() => { new IvaVentasCierrePedido { Periodo = p }.Validar(); return Task.CompletedTask; }, "Rechazo de período inválido");
var parametros = IvaVentasCierreDatos.ParametrosPeriodo("202511");
Check(parametros.Length == 2 && (string)parametros[0].Value == "2025" && (string)parametros[1].Value == "11" && parametros[0].Size == 4 && parametros[1].Size == 2, "Parámetros SP exactos, sin concatenar SQL");
var row = new IvaVentasCierreFila { periodo = "202511", tipo_registro = "2", importe = 999999999999999.99m, importe_gravado = 0.01m };
Check(row.Importes[^1] == row.importe && row.Importes[^1] != row.importe_gravado, "Mostrar el total almacenado sin recalcularlo");
Check(IvaVentasCierreFila.Numero(row.importe) == "999.999.999.999.999,99", "Máximo decimal(17,2) completo con centavos");
var recovered = JsonConvert.DeserializeObject<IvaVentasCierreFila>(JsonConvert.SerializeObject(row))!;
Check(recovered.importe == row.importe, "Newtonsoft conserva los 17 dígitos decimales");
new IvaVentasCierreEstado { ProximoPeriodo = "202512" }.Validar();
await Reject<InvalidOperationException>(() => { new IvaVentasCierreEstado { ProximoPeriodo = "202512", Libros = [row, row] }.Validar(); return Task.CompletedTask; }, "No aceptar filas duplicadas en el historial");
var datos = new DatosFake();
var users = DispatchProxy.Create<IApiUsuarioServicio, UsuarioProxy>();
var proxy = (UsuarioProxy)(object)users;
var svc = new IvaVentasCierreServicio(datos, users);
ClaimsPrincipal User(string adm = "0000", bool forced = false) => new(new ClaimsIdentity(new[] {
    new Claim("user", "operador"), new Claim("AdmId", adm + "#Sucursal"),
    new Claim("perfiles", "[{\"perfil_id\":\"P1\"}]"), new Claim("cambio_clave_obligatorio", forced ? "true" : "false") }, "test"));
Check(!svc.TieneAcceso(new ClaimsPrincipal(), "P1", "M"), "Anónimo denegado");
Check(!svc.TieneAcceso(User(), "P2", "M") && proxy.Calls == 0, "Perfil no perteneciente al token denegado antes de consultar menú");
await Reject<UnauthorizedAccessException>(() => svc.Cerrar(new() { Periodo = "202511" }, User(), "P1", "M", default), "Sin imp_lv_cierr no se cierra");
Check(datos.CloseCalls == 0, "Permiso rechazado no llega al SP");
await Reject<UnauthorizedAccessException>(() => svc.GenerarDuplicados(new() { Periodo = "202511" }, User(), "P1", "M", new MemoryStream(), default), "Duplicados exige el permiso real antes de consultar datos");
Check(datos.DuplicadosCalls == 0, "Sin permiso no se invoca el SP de Duplicados");
proxy.Allow = true;
Check(svc.TieneAcceso(User(), "P1", "M"), "Identificador real del menú imp_lv_cierr habilita el módulo");
proxy.MenuItemId = "imp_lv_cierre";
Check(!svc.TieneAcceso(User(), "P1", "M"), "Un identificador distinto no concede el permiso de cierre");
proxy.MenuItemId = "imp_lv_cierr";
Check(!svc.TieneAcceso(User(forced:true), "P1", "M"), "Cambio obligatorio de contraseña bloquea cierre");
var estado = await svc.Consultar(User("0001"), "P1", "M", default);
Check(!estado.PuedeCerrar && estado.ProximoPeriodo == "202511", "Sucursal no Central puede consultar, con cierre deshabilitado");
await Reject<UnauthorizedAccessException>(() => svc.Cerrar(new() { Periodo = "202511" }, User("0001"), "P1", "M", default), "Sucursal no Central no ejecuta cierre por API");
Check(datos.CloseCalls == 0, "Restricción de sucursal aplicada antes del repositorio");
var respuesta = await svc.Cerrar(new() { Periodo = "202511" }, User(), "P1", "M", default);
Check(respuesta.resultado == 0 && datos.CloseCalls == 1 && datos.Actor == "operador" && datos.Adm == "0000", "Cierre usa identidad y sucursal autenticadas");
await Reject<IvaVentasCierreConflictoException>(() => svc.Cerrar(new() { Periodo = "202511" }, User(), "P1", "M", default), "Confirmación vieja no cierra el nuevo período");
Check(datos.Writes == 1 && datos.CloseCalls == 2, "Sin reintento ni segundo cierre tras conflicto");
datos.NonZero = true;
respuesta = await svc.Cerrar(new() { Periodo = "202512" }, User(), "P1", "M", default);
Check(respuesta.resultado == 2 && datos.Writes == 1, "Respuesta de negocio del SP no se convierte en éxito");
await using var paquete = new MemoryStream();
await svc.GenerarRg3685(new() { Periodo = "202511" }, User("0001"), "P1", "M", paquete, default);
paquete.Position = 0;
using (var zip = new ZipArchive(paquete, ZipArchiveMode.Read, leaveOpen:true))
{
    Check(zip.Entries.Count == 2 && zip.Entries[0].FullName.EndsWith("_Cabecera.txt") && zip.Entries[1].FullName.EndsWith("_Detalle.txt"), "ZIP contiene exactamente los dos TXT del período");
    foreach (var entry in zip.Entries)
    {
        using var ms = new MemoryStream(); using (var s = entry.Open()) s.CopyTo(ms);
        var bytes = ms.ToArray();
        Check(!bytes.Take(3).SequenceEqual(new byte[]{0xef,0xbb,0xbf}), "Sin BOM en " + entry.Name);
        Check(Encoding.UTF8.GetString(bytes) == "0001 CAFÉ ÑANDÚ  0000000000123  \r\n0002  \r\n", "Conservar ceros, espacios finales, acentos y CRLF en " + entry.Name);
    }
}
Check(datos.Types.SequenceEqual(new[] { 'C', 'D' }), "SP invocado una vez por tipo, C y D");
await Reject<IvaVentasCierreConflictoException>(() => svc.GenerarRg3685(new() { Periodo = "202601" }, User(), "P1", "M", new MemoryStream(), default), "No exportar período sin cierre registrado");
await Reject<InvalidOperationException>(() => IvaVentasCierreArchivos.EscribirZip(new MemoryStream(), "202511", _ => DatosFake.Values([null]), default), "Línea SQL nula no se omite silenciosamente");
await Reject<InvalidOperationException>(() => IvaVentasCierreArchivos.EscribirZip(new MemoryStream(), "202511", _ => DatosFake.Values(["roto\nregistro"]), default), "Registro con salto interno se detecta sin transformarlo");
await using var vacio = new MemoryStream();
await IvaVentasCierreArchivos.EscribirZip(vacio, "202511", _ => DatosFake.Values([]), default);
vacio.Position = 0;
using (var zip = new ZipArchive(vacio, ZipArchiveMode.Read)) Check(zip.Entries.Count == 2 && zip.Entries.All(e => e.Length == 0), "Período sin líneas conserva los dos archivos vacíos");
var cts = new CancellationTokenSource(); cts.Cancel();
await Reject<OperationCanceledException>(() => IvaVentasCierreArchivos.EscribirZip(new MemoryStream(), "202511", _ => DatosFake.Values(["0000"]), cts.Token), "Cancelación impide emitir el paquete completo");
Check(IvaVentasCliente.Url("https://api.example/gcservicios", "api/iva-ventas-cierre/rg3685").AbsoluteUri == "https://api.example/gcservicios/api/iva-ventas-cierre/rg3685", "URL preserva subaplicación IIS");

Check(IvaVentasCierreArchivos.NombreDuplicados("202511") == "IVA_Ventas_Duplicados_2025-11.txt", "Duplicados tiene nombre TXT con período, sin ZIP");
await using var duplicados = new MemoryStream();
await svc.GenerarDuplicados(new() { Periodo = "202511" }, User("0001"), "P1", "M", duplicados, default);
var textoDuplicados = duplicados.ToArray();
Check(datos.DuplicadosCalls == 1 && datos.DuplicadosPeriodo == "202511", "Una llamada al SP de Duplicados con el período histórico seleccionado");
Check(new UTF8Encoding(false, true).GetString(textoDuplicados) == "10001 CAFÉ ÑANDÚ  00000000123  \r\n200000001                                                                     00000000123\r\n", "TXT conserva orden, ceros, acentos, espacios finales y CRLF sin recalcular el total");
Check(!textoDuplicados.Take(3).SequenceEqual(new byte[]{0xef,0xbb,0xbf}) && duplicados.CanWrite, "Duplicados sin BOM y sin cerrar el stream de respuesta");
Check(datos.CloseCalls == 3 && datos.Writes == 1, "Descargar Duplicados no invoca otro cierre ni modifica registros");
await Reject<IvaVentasCierreConflictoException>(() => svc.GenerarDuplicados(new() { Periodo = "202601" }, User(), "P1", "M", new MemoryStream(), default), "Duplicados rechaza un período sin cierre registrado");
await Reject<UnauthorizedAccessException>(() => svc.GenerarDuplicados(new() { Periodo = "202511" }, User(forced:true), "P1", "M", new MemoryStream(), default), "Duplicados rechaza sesión con cambio obligatorio de contraseña");
await Reject<ArgumentException>(() => svc.GenerarDuplicados(new() { Periodo = "202513" }, User(), "P1", "M", new MemoryStream(), default), "Duplicados rechaza mes inválido antes de ejecutar el SP");
Check(datos.DuplicadosCalls == 1, "Los pedidos rechazados no vuelven a ejecutar Duplicados");
foreach (var invalido in new string?[] { null, "", "registro\npartido", "registro\rpartido" })
    await Reject<IvaVentasCierreArchivoException>(() => IvaVentasCierreArchivos.EscribirDuplicados(new MemoryStream(), DatosFake.Values([invalido]), default), "Duplicados detecta registro inválido sin ocultarlo ni repararlo");
await Reject<IvaVentasCierreArchivoException>(() => IvaVentasCierreArchivos.EscribirDuplicados(new MemoryStream(), DatosFake.Values([]), default), "Duplicados no entrega TXT vacío sin registros del SP");
await Reject<OperationCanceledException>(() => IvaVentasCierreArchivos.EscribirDuplicados(new MemoryStream(), DatosFake.Values(["1"]), cts.Token), "Duplicados respeta cancelación antes de escribir");
await Reject<IOException>(() => IvaVentasCierreArchivos.EscribirDuplicados(new MemoryStream(), DatosFake.FallaAlFinal(), default), "Un fallo SQL tardío no se convierte en TXT completo");
await using var masivo = new MemoryStream();
await IvaVentasCierreArchivos.EscribirDuplicados(masivo, DatosFake.Masivo(), default);
Check(masivo.Length == 10_001 * 10L, "Duplicados escribe más de 5000 registros completos, sin paginar ni limitar el archivo");
Check(IvaVentasCliente.Url("https://api.example/gcservicios", "api/iva-ventas-cierre/duplicados").AbsoluteUri == "https://api.example/gcservicios/api/iva-ventas-cierre/duplicados", "Duplicados preserva PathBase de la API en IIS");
Console.WriteLine($"PASS: {passed} verificaciones sintéticas; ningún cierre real ni conexión SQL.");

public class DatosFake : IIvaVentasCierreDatos
{
    public string Next = "202511", Actor = "", Adm = "";
    public int CloseCalls, Writes; public bool NonZero;
    public int DuplicadosCalls; public string DuplicadosPeriodo = "";
    public List<char> Types = [];
    public Task<IvaVentasCierreEstado> Consultar(CancellationToken ct) => Task.FromResult(new IvaVentasCierreEstado { ProximoPeriodo = Next });
    public Task<IvaVentasCierreRespuesta> Cerrar(string p, string usuario, string adm, CancellationToken ct)
    {
        CloseCalls++; Actor = usuario; Adm = adm;
        if (p != Next) throw new IvaVentasCierreConflictoException("Cambió el período.");
        if (NonZero) return Task.FromResult(new IvaVentasCierreRespuesta { resultado = 2, resultado_msj = "No corresponde." });
        Writes++; Next = "202512"; return Task.FromResult(new IvaVentasCierreRespuesta { resultado = 0, resultado_msj = "OK" });
    }
    public Task<bool> ExisteCierre(string p, CancellationToken ct) => Task.FromResult(p == "202511");
    public IAsyncEnumerable<string?> LineasRg3685(string p, char tipo, CancellationToken ct) { Types.Add(tipo); return Values(["0001 CAFÉ ÑANDÚ  0000000000123  ", "0002  "]); }
    public IAsyncEnumerable<string?> LineasDuplicados(string p, CancellationToken ct) { DuplicadosCalls++; DuplicadosPeriodo = p; return Values(["10001 CAFÉ ÑANDÚ  00000000123  ", "200000001                                                                     00000000123"]); }
    public static async IAsyncEnumerable<string?> Values(string?[] lines) { await Task.CompletedTask; foreach (var line in lines) yield return line; }
    public static async IAsyncEnumerable<string?> FallaAlFinal() { await Task.CompletedTask; yield return "1000"; throw new IOException("Fallo simulado de lectura SQL."); }
    public static async IAsyncEnumerable<string?> Masivo() { await Task.CompletedTask; for (var i = 0; i < 10_001; i++) yield return "10000000"; }
}
public class UsuarioProxy : DispatchProxy
{
    public bool Allow; public int Calls;
    public string MenuItemId = "imp_lv_cierr";
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        if (method?.Name != "ObtenerMenu") throw new NotSupportedException();
        Calls++; return Allow ? new List<MenuPpalDto> { new() { mnu_item_id = MenuItemId } } : new List<MenuPpalDto>();
    }
}
