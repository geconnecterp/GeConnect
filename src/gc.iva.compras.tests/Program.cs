using System.Reflection;
using System.Security.Claims;
using System.Globalization;
using ClosedXML.Excel;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Servicios.Libros;
using gc.api.core.Servicios.Reportes;
using gc.api.infra.Datos.Implementacion;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using gc.sitio.core.Servicios.Implementacion.Libros;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Pruebas sintéticas: no abren conexiones SQL ni alteran datos, menús o archivos desplegados.
if (args.SequenceEqual(new[] { "--sql-compras-202510" }))
{
    await IvaComprasSqlCheck.Run();
    return;
}
int passed = 0;
void Check(bool ok, string name) { if (!ok) throw new Exception("FAIL: " + name); passed++; Console.WriteLine("OK: " + name); }
void Reject(Action action, string name)
{
    try { action(); } catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or OperationCanceledException) { Check(true, name); return; }
    throw new Exception("FAIL: " + name);
}
IvaComprasFiltro Filter() => IvaComprasFiltro.Inicial();
var now = IvaComprasFiltro.HoyArgentina();
var f = Filter();
Check(f.Periodo == now.Year && f.Mes == now.Month && f.Registros == 100 && f.Pagina == 1 &&
    !f.Fechas && !f.PorCuit && !f.PorComprobante && !f.PorTipoProveedor, "Inicial: mes actual, 100 filas, opcionales apagados");
Check(IvaComprasFiltro.Inicial(5000).Registros == 500 && IvaComprasFiltro.Inicial(0).Registros == 100, "Límite y fallback");
f.Periodo = now.Year - 5; f.Validar(); Check(true, "Seis años incluidos");
f.Periodo--; Reject(() => f.Validar(), "Séptimo año rechazado");
f = Filter(); f.Mes = 13; Reject(() => f.Validar(), "Mes inválido");
f = Filter(); f.Fechas = true; Reject(() => f.Validar(), "Fechas activas incompletas");
f.Periodo = 2024; f.Mes = 2; f.Desde = new(2024, 2, 1); f.Hasta = new(2024, 2, 29);
f.Validar(hoy: new(2026, 10, 17)); Check(true, "Fechas históricas en mes seleccionado y bisiesto");
f.Hasta = new(2024, 3, 1); Reject(() => f.Validar(hoy: new(2026, 10, 17)), "Fuera del mes");
f.Hasta = new(2024, 2, 1); f.Desde = new(2024, 2, 2); Reject(() => f.Validar(hoy: new(2026, 10, 17)), "Fechas invertidas");
f = Filter(); f.PorCuit = true; f.Cuit = "sin-formato%"; f.Validar(); Check(true, "CUIT sin algoritmo ni formato");
f.Cuit = new string('1', 16); Reject(() => f.Validar(), "CUIT respeta varchar(15)");
f = Filter(); f.PorTipoProveedor = true; Reject(() => f.Validar(), "Tipo activo requiere selección");
foreach (var ids in new[] { new[] { "%" }, new[] { "1,2" }, new[] { "001" }, new[] { "" } })
{ f.TiposProveedor = ids; Reject(() => f.Validar(), "Lista de proveedores inválida"); }
f.TiposProveedor = ["01", "02"]; f.PorComprobante = true; f.Comprobantes = ["001", "003"]; f.Validar(); Check(true, "Listas múltiples válidas");
f = Filter(); f.Registros = 501; Reject(() => f.Validar(), "Más de 500 no permitido");
f.Registros = 500; f.Pagina = int.MaxValue; Reject(() => f.Validar(), "Overflow de offset rechazado");
f = Filter(); f.Pagina = 3; f.Registros = 250; f.PorCuit = true; f.Cuit = "%301%";
f.PorTipoProveedor = true; f.TiposProveedor = ["01", "02", "01"]; f.PorComprobante = true; f.Comprobantes = ["001", "003"];
var p = IvaComprasDatos.Parametros(f, false).ToDictionary(x => x.ParameterName, x => x.Value);
Check(p.Count == 14 && (int)p["@registros"] == 250 && (int)p["@pagina"] == 3, "Contrato SQL paginado de 14 parámetros");
Check((string)p["@periodo"] == now.Year.ToString() && (string)p["@mes"] == now.Month.ToString("00"), "Período SQL yyyy y MM");
Check((string)p["@ope_list"] == "01,02" && (string)p["@tco_list"] == "001,003" && (string)p["@cuit_like"] == "%301%", "Listas CSV deduplicadas y CUIT preservado");
Check(!p.ContainsKey("@adm") && !p.ContainsKey("@caja"), "No se envían filtros de Ventas ajenos a Compras");
f.Pagina = 0; f.Registros = 5000;
p = IvaComprasDatos.Parametros(f, true).ToDictionary(x => x.ParameterName, x => x.Value);
Check((int)p["@pagina"] == 1 && (int)p["@registros"] == int.MaxValue && (string)p["@ope_list"] == "01,02", "Excel ignora navegación pero conserva filtros");
f = Filter(); f.Cuit = "ignorado"; f.TiposProveedor = ["xx"];
p = IvaComprasDatos.Parametros(f, false).ToDictionary(x => x.ParameterName, x => x.Value);
Check((string)p["@cuit_like"] == "%" && (string)p["@ope_list"] == "" && (bool)p["@ope"] == false, "Opcionales desmarcados no restringen");
Check(p["@fecha_d"] is DateTime && p["@fecha_h"] is DateTime, "Fechas obligatorias SQL siempre enviadas");
foreach (var culture in new[] { "es-AR", "en-US" })
{
    var old = CultureInfo.CurrentCulture;
    try { CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        Check(Equals(IvaComprasDatos.ConvertirValor("cm_cae_vto", "20251104", typeof(DateTime)), new DateTime(2025, 11, 4)), "CAE yyyyMMdd sin invertir mes/día " + culture);
    } finally { CultureInfo.CurrentCulture = old; }
}
Check(IvaComprasDatos.ConvertirValor("recepcion", DBNull.Value, typeof(DateTime)) == null &&
    IvaComprasDatos.ConvertirValor("cm_cae_vto", "   ", typeof(DateTime)) == null, "Recepción y CAE ausentes sin fecha inventada");
foreach (var value in new[] { "04/11/2025", "00000000", "20250229", "secreto" })
{
    try { IvaComprasDatos.ConvertirValor("cm_cae_vto", value, typeof(DateTime)); throw new Exception("Se aceptó fecha inválida"); }
    catch (IvaComprasDatoInvalidoException ex) { Check(!ex.Message.Contains(value) && ex.Campo == "cm_cae_vto", "Error de formato sin datos fiscales"); }
}
Check(Equals(IvaComprasDatos.ConvertirValor("ccb_21", "0000012345", typeof(string)), "0000012345"), "Cuentas contables conservan ceros");
var invalid = IvaComprasCliente.LeerErrorDatos("{\"codigo\":\"IVA_COMPRAS_DATO_INVALIDO\",\"campo\":\"cm_cae_vto\",\"mensaje\":\"texto externo\"}");
Check(invalid != null && !invalid.Message.Contains("texto externo"), "Sitio reconstruye error de contrato seguro");
Check(IvaComprasCliente.LeerErrorDatos("<html/>") == null && IvaComprasCliente.LeerErrorDatos("{\"codigo\":\"IVA_COMPRAS_DATO_INVALIDO\",\"campo\":\"extra\"}") == null, "No se propaga HTML ni campos ajenos");

var fake = new FakeDatos(); var service = new IvaComprasServicio(fake, null!);
var empty = await service.Consultar(Filter(), default); Check(empty.Total == 0 && empty.Paginas == 0, "Consulta vacía sin metadatos inventados");
fake.Rows = [new() { total_registros = 1, total_paginas = 1, tipo = "001", compte = "0001", movi = "M1", cta_id = "C1" }];
fake.EmptyLaterPages = true; f = Filter(); f.Pagina = 3;
var first = await service.Consultar(f, default); Check(first.Pagina == 1 && first.Total == 1 && fake.Calls == 3, "Página vacía por cambios vuelve a primera");
fake.EmptyLaterPages = false;
fake.Rows[0].total_paginas = 99; Reject(() => service.Consultar(Filter(), default).GetAwaiter().GetResult(), "Metadatos de páginas inconsistentes");
fake.Rows[0].total_paginas = 1; fake.Rows[0].total_registros = 101;
Reject(() => service.Consultar(Filter(), default).GetAwaiter().GetResult(), "Página incompleta se rechaza");
fake.Rows[0].total_registros = 1;
var all = await service.Todos(Filter(), default); Check(fake.Complete && all.Count == 1, "Exportación solicita conjunto completo");
Reject(() => IvaComprasServicio.ValidarExportacionCompleta([]), "No exportar conjunto vacío");
Reject(() => IvaComprasServicio.ValidarExportacionCompleta([new() { total_registros = 2, total_paginas = 1 }]), "No exportar parcial");
Reject(() => IvaComprasServicio.ValidarExportacionCompleta([new() { total_registros = 2, total_paginas = 1 }, new() { total_registros = 2, total_paginas = 1 }]), "No exportar clave duplicada");
IvaComprasServicio.ValidarExportacionCompleta([new() { total_registros = 2, total_paginas = 1, movi = "A" }, new() { total_registros = 2, total_paginas = 1, movi = "B" }]);
Check(true, "Movimiento distingue comprobantes del mismo número");

const int count = 5001;
var rows = Enumerable.Range(1, count).Select(i => new IvaComprasDetalleDto { total_registros = count, total_paginas = 1,
    tipo = "001", compte = i.ToString("00000000"), movi = "000000001", cta_id = "C0001", cuit = "00123456789", cm_cae = "00123456789012",
    nombre = "=HYPERLINK(\"https://invalid.test\")", fecha = new DateTime(2025, 11, 4), ccb_21 = "0000012345",
    gra_2100 = -123.45m, iva_2100 = 25.92m, exento = 20.02m, no_gravado = 10.01m, total = 123456789012.12m }).ToList();
rows[1].total = 123456789012345.67m;
var watch = System.Diagnostics.Stopwatch.StartNew();
var bytes = IvaComprasExcel.Generar(rows, Filter());
using (var book = new XLWorkbook(new MemoryStream(bytes)))
{
    var ws = book.Worksheet(1);
    int Col(string campo) => Array.FindIndex(IvaComprasColumnas.Todas, x => x.Campo == campo) + 1;
    Check(ws.LastRowUsed()!.RowNumber() == count + 5 && ws.LastColumnUsed()!.ColumnNumber() == 39, "Excel completo 5001 x 39 sin metadatos");
    Check(ws.Cell(6, Col("no_gravado")).GetValue<decimal>() == 10.01m && ws.Cell(6, Col("exento")).GetValue<decimal>() == 20.02m, "Conceptos separados");
    Check(ws.Cell(6, Col("total")).GetValue<decimal>() == 123456789012.12m && ws.Cell(6, Col("gra_2100")).GetValue<decimal>() == -123.45m, "Importes grandes y negativos numéricos");
    Check(ws.Cell(7, Col("total")).DataType == XLDataType.Text && ws.Cell(7, Col("total")).GetString() == "123456789012345.67", "Mayor precisión que Excel se preserva exacta como texto");
    foreach (var field in new[] { "compte", "cuit", "cm_cae", "ccb_21", "movi" }) Check(ws.Cell(6, Col(field)).GetString().StartsWith('0'), "Ceros de " + field);
    Check(ws.Cell(6, Col("fecha")).DataType == XLDataType.DateTime && ws.Cell(6, Col("recepcion")).IsEmpty(), "Fechas tipadas y nulos");
    Check(!ws.Cell(6, Col("nombre")).HasFormula && ws.Cell(6, Col("nombre")).DataType == XLDataType.Text, "No inyección de fórmulas");
    Check(ws.SheetView.SplitRow == 5 && ws.AutoFilter.IsEnabled, "Encabezados congelados y autofiltro");
}
Console.WriteLine($"XLSX sintético: {bytes.Length:N0} bytes, {watch.ElapsedMilliseconds:N0} ms. No es prueba de SQL ni volumen productivo.");
Reject(() => IvaComprasExcel.Generar(rows, Filter(), new CancellationToken(true)), "Cancelación no entrega archivo");
Check(IvaComprasCliente.Url("https://api.test/apireport", "api/iva-compras/excel").AbsoluteUri == "https://api.test/apireport/api/iva-compras/excel", "Subaplicación IIS preservada");
Reject(() => IvaComprasCliente.Url("", "api/iva-compras/excel"), "URL no configurada rechazada");
var jsonField = typeof(IvaComprasCliente).GetField("Json", BindingFlags.NonPublic | BindingFlags.Static)!;
var json = (JsonSerializerSettings)jsonField.GetValue(null)!;
Check(json.TypeNameHandling == TypeNameHandling.None, "Newtonsoft sin activación arbitraria de tipos");
f = Filter(); f.PorTipoProveedor = true; f.TiposProveedor = ["01", "02"];
var serialized = JObject.Parse(JsonConvert.SerializeObject(f, json));
Check(serialized["porTipoProveedor"]!.Value<bool>() && serialized["tiposProveedor"]!.Count() == 2, "Contrato camelCase conserva listas");
var users = DispatchProxy.Create<IApiUsuarioServicio, UsuarioProxy>(); var proxy = (UsuarioProxy)(object)users;
service = new IvaComprasServicio(fake, users);
var claims = new[] { new Claim("user", "prueba"), new Claim("AdmId", "0001#Sucursal"), new Claim("perfiles", "[{\"perfil_id\":\"P1\"}]") };
var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
Check(!service.TieneAcceso(new ClaimsPrincipal(), "P1", "C"), "Anónimo denegado");
Check(!service.TieneAcceso(user, "P2", "C") && proxy.Calls == 0, "Perfil ajeno al token denegado");
Check(!service.TieneAcceso(user, "P1", "C"), "Permiso de Ventas no habilita Compras");
proxy.Menu = "imp_lc_det"; Check(service.TieneAcceso(user, "P1", "C"), "Permiso propio imp_lc_det habilita");
var forced = new ClaimsPrincipal(new ClaimsIdentity(claims.Append(new Claim("cambio_clave_obligatorio", "true")), "test"));
Check(!service.TieneAcceso(forced, "P1", "C"), "Cambio de clave obligatorio bloquea exportación");
Console.WriteLine($"PASS: {passed} verificaciones.");

public class FakeDatos : IIvaComprasDatos
{
    public List<IvaComprasDetalleDto> Rows = [];
    public bool EmptyLaterPages, Complete;
    public int Calls;
    public Task<List<IvaComprasDetalleDto>> Consultar(IvaComprasFiltro f, bool completo, CancellationToken ct)
    { Calls++; Complete = completo; return Task.FromResult(EmptyLaterPages && f.Pagina > 1 ? new List<IvaComprasDetalleDto>() : Rows); }
    public Task<IvaComprasOpciones> Opciones(CancellationToken ct) => Task.FromResult(new IvaComprasOpciones());
}
public class UsuarioProxy : DispatchProxy
{
    public string Menu = "imp_lv_det"; public int Calls;
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    { if (method?.Name != "ObtenerMenu") throw new NotSupportedException(); Calls++; return new List<MenuPpalDto> { new() { mnu_item_id = Menu } }; }
}
