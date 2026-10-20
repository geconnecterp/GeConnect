using System.Data;
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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ApiController = gc.api.Controllers.Libros.IvaComprasCierreController;

// Sólo dobles y datos sintéticos. Nunca abre SQL ni ejecuta un cierre real.
var checks = 0;
void Check(bool ok, string name) { if (!ok) throw new Exception("FAIL: " + name); checks++; Console.WriteLine("OK: " + name); }
async Task Reject<T>(Func<Task> action, string name) where T : Exception
{ try { await action(); } catch (T) { Check(true, name); return; } throw new Exception("FAIL: " + name); }
Task Run(Action action) { action(); return Task.CompletedTask; }

foreach (var p in new[] { "202512", "202601", "200002", "199901" })
    Check(IvaComprasCierrePeriodo.Texto(p) == p[..4] + "-" + p[4..], "Período válido " + p);
foreach (var p in new string?[] { null, "", "202500", "202513", "000001", "2025-11", "２０２５１１", "202511 ", "2025'1" })
    await Reject<ArgumentException>(() => Run(() => IvaComprasCierrePeriodo.Validar(p)), "Período inválido rechazado");
foreach (var t in new string?[] { null, "", "22", " ", "\n" })
    await Reject<ArgumentException>(() => Run(() => new IvaComprasCierreDescargaPedido { Periodo = "202511", TipoRegistro = t! }.Validar()), "Tipo de registro inválido rechazado");
var par = IvaComprasCierreDatos.ParametrosPeriodo("202510");
Check(par.Length == 2 && par[0].Size == 4 && par[1].Size == 2 && (string)par[0].Value == "2025" && (string)par[1].Value == "10", "Parámetros tipados exactos");
var fila = Fixture().Libros[0];
Check(fila.Importes.Length == 11 && fila.Importes[^1] == fila.importe, "Once importes; total almacenado, no recalculado");
Check(IvaComprasCierreFila.Numero(fila.importe) == "999.999.999.999.999,99", "decimal(17,2) con centavos completos");
var jsonFila = JsonConvert.SerializeObject(fila);
var vuelta = JsonConvert.DeserializeObject<IvaComprasCierreFila>(jsonFila)!;
Check(vuelta.importe == fila.importe && vuelta.cuit == null && vuelta.cant_registros == null, "JSON exacto y nulos válidos");
var jsonApi = JsonConvert.SerializeObject(fila, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
var vueltaApi = JsonConvert.DeserializeObject<IvaComprasCierreFila>(jsonApi)!;
Check(vueltaApi.cant_registros == null && vueltaApi.cuit == null && JObject.Parse(jsonApi).ContainsKey("cant_registros") && JObject.Parse(jsonApi).ContainsKey("cuit"), "DTO incluye nulos aunque API global los omita");
var incompleto = JObject.Parse(jsonFila); incompleto.Remove("importe_iva");
await Reject<JsonSerializationException>(() => Run(() => JsonConvert.DeserializeObject<IvaComprasCierreFila>(incompleto.ToString())), "JSON incompleto no fabrica importes cero");
await Reject<JsonSerializationException>(() => Run(() => JsonConvert.DeserializeObject<IvaComprasCierreRespuesta>("{}")), "Respuesta vacía no se convierte en éxito");
await Reject<JsonSerializationException>(() => Run(() => JsonConvert.DeserializeObject<IvaComprasCierreEstado>("{\"ProximoPeriodo\":\"202511\",\"PuedeCerrar\":true}")), "Estado sin historial rechazado");

var tabla = new DataTable();
foreach (var p in typeof(IvaComprasCierreFila).GetProperties().Where(p => p.CanWrite)) tabla.Columns.Add(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType);
var valores = tabla.NewRow();
foreach (DataColumn c in tabla.Columns) valores[c] = typeof(IvaComprasCierreFila).GetProperty(c.ColumnName)!.GetValue(fila) ?? DBNull.Value;
tabla.Rows.Add(valores);
using (var reader = tabla.CreateDataReader()) { reader.Read(); var mapped = IvaComprasCierreDatos.LeerFila(reader); Check(mapped.importe == fila.importe && mapped.cant_registros == null && mapped.cuit == null && mapped.importe_mt == fila.importe_mt, "Contrato SQL de 15 columnas y nulos admitidos"); }
valores["importe_iva"] = DBNull.Value;
await Reject<InvalidOperationException>(() => Run(() => { using var r = tabla.CreateDataReader(); r.Read(); IvaComprasCierreDatos.LeerFila(r); }), "NULL monetario inesperado rechazado");
valores["importe_iva"] = fila.importe_iva; tabla.Columns.Remove("importe_otro_ng");
await Reject<ArgumentException>(() => Run(() => { using var r = tabla.CreateDataReader(); r.Read(); IvaComprasCierreDatos.LeerFila(r); }), "Columna SQL faltante rechazada");
var dup = Fixture(); dup.Libros.Add(dup.Libros[0]);
await Reject<InvalidOperationException>(() => Run(dup.Validar), "PK duplicada rechazada");
var tipos = Fixture(); tipos.Libros.Add(new() { periodo = fila.periodo, tipo_registro = "1" }); tipos.Validar();
Check(tipos.Libros.Count == 2, "Mismo período y distinto tipo no se agrupan");
var mas72 = Fixture(); for (var i = 0; i < 72; i++) mas72.Libros.Add(new() { periodo = "202410", tipo_registro = "2" });
await Reject<InvalidOperationException>(() => Run(mas72.Validar), "Historial sobre 72 rechazado");
var completo72 = new IvaComprasCierreEstado { ProximoPeriodo = "202511", Libros = Enumerable.Range(0, 72).Select(i => new IvaComprasCierreFila { tipo_registro = "2", periodo = new DateTime(2025, 10, 1).AddMonths(-i).ToString("yyyyMM") }).ToList() };
completo72.Validar(); Check(completo72.Libros.Count == 72, "72 registros distintos válidos sin filtro de seis años");

var usuarios = DispatchProxy.Create<IApiUsuarioServicio, UsuariosFake>(); var permisos = (UsuariosFake)(object)usuarios;
var datos = new DatosFake();
IvaComprasCierreServicio Servicio(DatosFake d) => new(d, usuarios, NullLogger<IvaComprasCierreServicio>.Instance);
var svc = Servicio(datos);
ClaimsPrincipal User(string adm = "0000", bool forced = false, string actor = "QA") => new(new ClaimsIdentity(new[] {
    new Claim("user", actor), new Claim("AdmId", adm + "#QA"), new Claim("perfiles", "[{\"perfil_id\":\"P1\"}]"),
    new Claim("cambio_clave_obligatorio", forced ? "true" : "false") }, "test"));
Check(!svc.TieneAcceso(new ClaimsPrincipal(), "P1", "C"), "Anónimo denegado");
Check(!svc.TieneAcceso(User(), "P2", "C") && permisos.Calls == 0, "Perfil ajeno rechazado antes del menú");
Check(!svc.TieneAcceso(User(), "P1", "C"), "Sin permiso no se habilita módulo");
await Reject<UnauthorizedAccessException>(() => svc.Cerrar(new() { Periodo = "202510" }, User(), "P1", "C", default), "Sin permiso no llega al repositorio");
Check(datos.CloseCalls == 0, "Sin escritura por permiso denegado");
permisos.MenuItem = "imp_lc_cierre"; Check(!svc.TieneAcceso(User(), "P1", "C"), "Errata del permiso no concede acceso");
permisos.MenuItem = "imp_lv_cierr"; Check(!svc.TieneAcceso(User(), "P1", "C"), "Permiso Ventas no concede Compras");
permisos.MenuItem = "imp_lc_cierr"; Check(svc.TieneAcceso(User(), "P1", "C"), "Permiso real habilita Compras");
Check(!svc.TieneAcceso(User(forced: true), "P1", "C"), "Cambio de clave obligatorio bloquea acceso");
Check(!svc.TieneAcceso(User(actor: "actor-demasiado-largo"), "P1", "C"), "Actor fuera de contrato rechazado");
Check(!(await svc.Consultar(User("0001"), "P1", "C", default)).PuedeCerrar, "Sucursal consulta con cierre deshabilitado");
await Reject<UnauthorizedAccessException>(() => svc.Cerrar(new() { Periodo = "202510" }, User("0001"), "P1", "C", default), "Sucursal no Central no cierra por API");
Check(datos.CloseCalls == 0, "Restricción Central anterior al repositorio");
var rsp = await svc.Cerrar(new() { Periodo = "202510" }, User(), "P1", "C", default);
Check(rsp.resultado == 0 && datos.Actor == "QA" && datos.Adm == "0000" && datos.Writes == 1, "Cierre sintético usa identidad autenticada");
await Reject<IvaComprasCierreConflictoException>(() => svc.Cerrar(new() { Periodo = "202510" }, User(), "P1", "C", default), "Confirmación vieja no cierra el siguiente período");
Check(datos.Writes == 1, "No hay segundo cierre tras conflicto");
var rechaza = new DatosFake { Rechazar = true }; var rechazado = await Servicio(rechaza).Cerrar(new() { Periodo = "202510" }, User(), "P1", "C", default);
Check(rechazado.resultado == 2 && rechaza.Writes == 0, "Rechazo del SP conservado sin éxito falso");
var timeout = new DatosFake { FallarCierre = true };
await Reject<TimeoutException>(() => Servicio(timeout).Cerrar(new() { Periodo = "202510" }, User(), "P1", "C", default), "Timeout sin reintento automático");
Check(timeout.CloseCalls == 1, "Una sola invocación cuando se pierde el resultado");
var concurrente = new DatosFake(); var concurrentSvc = Servicio(concurrente);
var intentos = Enumerable.Range(0, 2).Select(async _ => { try { await concurrentSvc.Cerrar(new() { Periodo = "202510" }, User(), "P1", "C", default); return true; } catch (IvaComprasCierreConflictoException) { return false; } });
Check((await Task.WhenAll(intentos)).Count(x => x) == 1 && concurrente.Writes == 1, "Doble solicitud: modelo sintético del guard de período");

var pedido = new IvaComprasCierreDescargaPedido { Periodo = "202510", TipoRegistro = "2" };
await using var paquete = new MemoryStream();
var conteos = await svc.GenerarRg3685(pedido, User("0001"), "P1", "C", paquete, default);
Check(conteos.Cabecera == 1 && conteos.Detalle == 1 && datos.Ramas.SequenceEqual(new[] { 'C', 'D' }), "Ambas ramas explícitas y secuenciales");
Check(datos.TipoExistencia == "2" && datos.PeriodoExistencia == "202510", "Existencia verificada con clave compuesta");
paquete.Position = 0;
using (var zip = new ZipArchive(paquete, ZipArchiveMode.Read, leaveOpen: true))
{
    Check(zip.Entries.Count == 2 && zip.Entries[0].FullName == "IVA_Compras_3685_2025-10_Cabecera.txt" && zip.Entries[1].FullName.EndsWith("_Detalle.txt"), "ZIP con nombres propios y dos archivos");
    using var raw = new MemoryStream(); await zip.Entries[0].Open().CopyToAsync(raw);
    Check(raw.ToArray().SequenceEqual(new UTF8Encoding(false, true).GetBytes(DatosFake.LineaC + "\r\n")), "Texto, acentos, espacios finales, CRLF y sin BOM exactos");
}
var vacioD = new DatosFake { VacioD = true };
using (var ms = new MemoryStream())
{
    var c = await Servicio(vacioD).GenerarRg3685(pedido, User(), "P1", "C", ms, default); ms.Position = 0;
    using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
    Check(c.Detalle == 0 && zip.Entries.Count == 2 && zip.Entries[1].Length == 0, "Detalle vacío válido conserva ambas entradas");
}
using (var ms = new MemoryStream())
{
    var c = await IvaComprasCierreArchivos.EscribirZip(ms, "202510", _ => DatosFake.Lineas([]), default); ms.Position = 0;
    using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
    Check(c.Cabecera == 0 && c.Detalle == 0 && zip.Entries.All(e => e.Length == 0), "Sin filas no se inventan renglones");
}
foreach (var mala in new string?[] { null, "", "abc\rdef", "abc\ndef" })
    await Reject<IvaComprasCierreArchivoException>(() => IvaComprasCierreArchivos.EscribirZip(new MemoryStream(), "202510", _ => DatosFake.Lineas([mala]), default), "Registro inválido no se omite ni corrige");
using (var ms = new MemoryStream())
{
    var c = await IvaComprasCierreArchivos.EscribirZip(ms, "202510", _ => DatosFake.Masivo(), default);
    Check(c.Cabecera == 10001 && c.Detalle == 10001, "Exporta más de 5000 sin paginación ni límite del historial");
}
await Reject<OperationCanceledException>(() => IvaComprasCierreArchivos.EscribirZip(new MemoryStream(), "202510", _ => DatosFake.Masivo(), new CancellationToken(true)), "Cancelación respetada");
datos.Existe = false;
await Reject<IvaComprasCierreConflictoException>(() => svc.GenerarRg3685(pedido, User(), "P1", "C", new MemoryStream(), default), "Registro inventado no habilita descarga"); datos.Existe = true;

ApiController Api(DatosFake d) => new(Servicio(d), NullLogger<ApiController>.Instance) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = User() } } };
void Headers(ApiController api) { api.Request.Headers["X-Geco-Perfil"] = "P1"; api.Request.Headers["X-Geco-Menu"] = "C"; }
var apiOk = Api(new()); Headers(apiOk);
var descarga = await apiOk.Rg3685(pedido, default) as FileStreamResult;
Check(descarga != null && descarga.ContentType == "application/zip", "API entrega archivo sólo después de generación completa");
var tempPath = ((FileStream)descarga!.FileStream).Name; Check(File.Exists(tempPath), "Temporal existe durante entrega");
await descarga.FileStream.DisposeAsync(); Check(!File.Exists(tempPath), "Temporal eliminado al terminar entrega");
var apiFail = Api(new() { FallarD = true }); Headers(apiFail);
var antes = Directory.GetFiles(Path.GetTempPath(), "geco-iva-compras-*.zip").Order().ToArray();
var error = await apiFail.Rg3685(pedido, default) as ObjectResult;
Check(error?.StatusCode == 503 && !JsonConvert.SerializeObject(error.Value).Contains("secreto"), "Error en D no entrega ZIP ni filtra excepción SQL");
Check(antes.SequenceEqual(Directory.GetFiles(Path.GetTempPath(), "geco-iva-compras-*.zip").Order()), "Error de D limpia el temporal");
Check(typeof(ApiController).GetCustomAttributes().Any(a => a is Microsoft.AspNetCore.Authorization.AuthorizeAttribute), "API exige autenticación");
Check(typeof(gc.sitio.Areas.Libros.Controllers.IvaComprasCierreController).GetCustomAttributes().Any(a => a is AutoValidateAntiforgeryTokenAttribute), "Sitio exige antiforgery en POST");
Check(!typeof(ApiController).GetMethods().Any(m => m.Name.Contains("Duplicados")), "Sin endpoint Duplicados en esta etapa");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews().AddApplicationPart(typeof(gc.sitio.Areas.Libros.Controllers.IvaComprasCierreController).Assembly);
var app = builder.Build();
async Task<string> Historial(HttpContext http, IvaComprasCierreEstado modelo)
{
    var action = new ActionContext(http, new RouteData(), new ActionDescriptor());
    var engine = http.RequestServices.GetRequiredService<ICompositeViewEngine>();
    var view = engine.GetView(null, "/Areas/Libros/Views/IvaComprasCierre/_Historial.cshtml", false);
    if (!view.Success) throw new Exception("No se encontró la vista real.");
    using var sw = new StringWriter();
    var data = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = modelo };
    var temp = new TempDataDictionary(http, http.RequestServices.GetRequiredService<ITempDataProvider>());
    await view.View.RenderAsync(new ViewContext(action, view.View, data, temp, sw, new HtmlHelperOptions()));
    return sw.ToString();
}
using (var scope = app.Services.CreateScope())
{
    var html = System.Net.WebUtility.HtmlDecode(await Historial(new DefaultHttpContext { RequestServices = scope.ServiceProvider }, Fixture()));
    Check(html.Contains("999.999.999.999.999,99") && html.Contains("Otros no gravados") && html.Contains("Monotributistas"), "Razor real preserva importes máximos y conceptos propios");
    Check(html.Contains("data-clave=\"2:202510\"") && html.Contains("data-tipo=\"2\"") && html.Contains("Hasta 72 registros"), "Vista conserva clave compuesta y límite explícito");
    Check(html.Contains("—") && !html.Contains("Fecha de cierre"), "NULL visibles sin fecha inexistente");
    var html72 = await Historial(new DefaultHttpContext { RequestServices = scope.ServiceProvider }, completo72);
    Check(html72.Split("data-clave=").Length - 1 == 72, "Razor real conserva las 72 filas completas");
    var htmlVacio = await Historial(new DefaultHttpContext { RequestServices = scope.ServiceProvider }, new IvaComprasCierreEstado { ProximoPeriodo = "202511" });
    Check(htmlVacio.Contains("Todav") && !htmlVacio.Contains("data-clave="), "Estado vacío sin registros ficticios");
}
Console.WriteLine($"TOTAL: {checks} comprobaciones sintéticas correctas; cero conexiones SQL.");
if (args.Contains("--serve"))
{
    var raiz = Directory.GetCurrentDirectory();
    var index = await File.ReadAllTextAsync(Path.Combine(raiz, "gc.sitio/Areas/Libros/Views/IvaComprasCierre/Index.cshtml"));
    var seccion = index[index.IndexOf("<section", StringComparison.Ordinal)..index.LastIndexOf("</section>", StringComparison.Ordinal)] + "</section>";
    seccion = seccion.Replace("@Url.Action(\"Estado\")", "/estado").Replace("@Url.Action(\"Cerrar\")", "/cerrar").Replace("@Url.Action(\"Rg3685\")", "/rg3685")
        .Replace("@Url.Action(\"Index\", \"Home\", new { area = \"\" })", "/qa")
        .Replace("@(ViewBag.Error == null ? \"true\" : \"false\")", "true").Replace("@Html.AntiForgeryToken()", "<input type=\"hidden\" name=\"__RequestVerificationToken\" value=\"QA-SOLO-FIXTURE\">");
    seccion = System.Text.RegularExpressions.Regex.Replace(seccion, "    @if \\(ViewBag.Error != null\\)[^\\r\\n]*", "");
    var frame = "<!doctype html><html lang=\"es\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><link rel=\"stylesheet\" href=\"/bootstrap.css\"><link rel=\"stylesheet\" href=\"/golden.css\"><link rel=\"stylesheet\" href=\"/module.css\"></head><body style=\"margin:0 0 35px;font-family:Arial,sans-serif;background:#f7f6f1\"><header style=\"height:86px;background:#363128;color:#fff;padding:20px\">GECO · QA AISLADA · DATOS SINTÉTICOS · SIN SQL</header><h1 style=\"font-size:16px;margin:12px\">Libro IVA Compras - Cierres</h1><main>" + seccion + "</main><footer style=\"height:30px;font-size:12px\">Prueba sintética, no es el módulo autenticado ni un cierre fiscal</footer><script src=\"/bootstrap.js\"></script><script src=\"/module.js\"></script></body></html>";
    app.MapGet("/qa", () => Results.Content(frame, "text/html"));
    app.MapGet("/module.css", () => Results.File(Path.Combine(raiz, "gc.sitio/wwwroot/css/modules/iva-compras-cierre.css"), "text/css"));
    app.MapGet("/module.js", () => Results.File(Path.Combine(raiz, "gc.sitio/wwwroot/js/app/libros/iva-compras-cierre.js"), "text/javascript"));
    app.MapGet("/bootstrap.css", () => Results.File(Path.Combine(raiz, "gc.sitio/wwwroot/lib/bootstrap/css/bootstrap.css"), "text/css"));
    app.MapGet("/bootstrap.js", () => Results.File(Path.Combine(raiz, "gc.sitio/wwwroot/lib/bootstrap/js/bootstrap.bundle.min.js"), "text/javascript"));
    app.MapGet("/golden.css", () => Results.File(Path.Combine(raiz, "gc.sitio/wwwroot/css/golden-overlay.css"), "text/css"));
    app.MapPost("/estado", async (HttpContext http) => { await Task.Delay(750); var m = Fixture(); if (http.Request.Query["sucursal"] == "1") m.PuedeCerrar = false; return Results.Content(await Historial(http, m), "text/html"); });
    app.MapPost("/cerrar", () => Results.Json(new { resultado = 2, resultado_msj = "QA: cierre real deshabilitado en el host sintético." }));
    app.MapPost("/rg3685", async () => { using var ms = new MemoryStream(); await IvaComprasCierreArchivos.EscribirZip(ms, "202510", _ => DatosFake.Lineas(["QA SINTETICA"]), default); return Results.File(ms.ToArray(), "application/zip", "QA_SINTETICA.zip"); });
    await app.RunAsync("http://127.0.0.1:17866");
}

static IvaComprasCierreEstado Fixture() => new() { ProximoPeriodo = "202511", PuedeCerrar = true, Libros = [new() {
    tipo_registro = "2", periodo = "202510", importe = 999999999999999.99m, importe_iva = -30394.99m,
    importe_gravado = 12345678901234.56m, importe_no_gravado = 1679693504.31m, importe_exento = .01m,
    importe_otro_ng = 117606438.73m, importe_mt = 1000000.12m, percep_imp_nacionales = 1324567890.56m,
    percep_ib = 123456789.12m, percep_municipales = 56789.01m, importe_imp_internos = 6578912.34m }] };

public class UsuariosFake : DispatchProxy
{
    public int Calls; public string MenuItem = "";
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    { if (method?.Name != "ObtenerMenu") throw new NotSupportedException(); Calls++; return new List<MenuPpalDto> { new() { mnu_item_id = MenuItem } }; }
}
public class DatosFake : IIvaComprasCierreDatos
{
    public const string LineaC = "1000 CAFÉ ÑANDÚ     0000000123  ";
    public int CloseCalls, Writes; public string Actor = "", Adm = "", PeriodoExistencia = "", TipoExistencia = "", Next = "202510";
    public bool Rechazar, FallarCierre, FallarD, VacioD, Existe = true;
    public List<char> Ramas = []; private readonly SemaphoreSlim gate = new(1, 1);
    public Task<IvaComprasCierreEstado> Consultar(CancellationToken ct) => Task.FromResult(new IvaComprasCierreEstado { ProximoPeriodo = Next });
    public async Task<IvaComprasCierreRespuesta> Cerrar(string p, string u, string a, CancellationToken ct)
    {
        CloseCalls++; if (FallarCierre) throw new TimeoutException("secreto simulado");
        await gate.WaitAsync(ct);
        try { await Task.Delay(5, ct); if (Next != p) throw new IvaComprasCierreConflictoException("Período cambiado"); if (Rechazar) return new() { resultado = 2, resultado_msj = "Rechazado" }; Actor = u; Adm = a; Writes++; Next = "202511"; return new() { resultado = 0, resultado_msj = "OK" }; }
        finally { gate.Release(); }
    }
    public Task<bool> ExisteCierre(string p, string t, CancellationToken ct) { PeriodoExistencia = p; TipoExistencia = t; return Task.FromResult(Existe); }
    public IAsyncEnumerable<string?> LineasRg3685(string p, char tipo, CancellationToken ct)
    { Ramas.Add(tipo); return tipo == 'D' && FallarD ? Falla() : Lineas(tipo == 'D' && VacioD ? [] : [tipo == 'C' ? LineaC : "20000  0001"]); }
    public static async IAsyncEnumerable<string?> Lineas(string?[] lines) { await Task.CompletedTask; foreach (var l in lines) yield return l; }
    public static async IAsyncEnumerable<string?> Masivo() { await Task.CompletedTask; for (var i = 0; i < 10001; i++) yield return "10000"; }
    private static async IAsyncEnumerable<string?> Falla() { await Task.CompletedTask; yield return "0001"; throw new IOException("secreto simulado SQL"); }
}
