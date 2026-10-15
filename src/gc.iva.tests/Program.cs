using System.Reflection;
using System.Security.Claims;
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

// Integración SQL explícita y de solo lectura; nunca se ejecuta en la batería sintética.
if (args.SequenceEqual(new[] { "--sql-iva-202511" }))
{
    await IvaVentasSqlCheck.Run();
    return;
}

var passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    Console.WriteLine("OK: " + name); passed++;
}
void Reject(Action action, string name)
{
    try { action(); } catch (ArgumentException) { Check(true, name); return; }
    catch (InvalidOperationException) { Check(true, name); return; }
    throw new Exception("FAIL: se aceptó " + name);
}
IvaVentasFiltro Filter() => IvaVentasFiltro.Inicial(100);
// Componente visual: límites, extremos y costo independiente del total de páginas.
foreach (var total in new[] { 1, 2, 5, 6, 7, 140, 1533, int.MaxValue })
{
    foreach (var pagina in new[] { 1, Math.Max(1, total / 2), total })
    {
        var numeros = new gc.sitio.Models.Components.PaginadorNumerado(pagina, total).Numeros();
        var valores = numeros.OfType<int>().ToArray();
        Check(valores.Length <= 7 && numeros.Count <= 9 && valores.Distinct().Count() == valores.Length &&
              valores.SequenceEqual(valores.OrderBy(n => n)) && valores.Contains(1) && valores.Contains(total) &&
              valores.Contains(pagina) && valores.All(n => n >= 1 && n <= total), $"Paginador acotado {pagina}/{total}");
    }
}
Check(new gc.sitio.Models.Components.PaginadorNumerado(0, 0).Pagina == 1 &&
      new gc.sitio.Models.Components.PaginadorNumerado(99, 5).Pagina == 5, "Paginador normaliza límites de presentación");
var hoy = IvaVentasFiltro.HoyArgentina();
var f = Filter();
// Regresión de producción: el SP entrega cm_cae_vto como CHAR(8) yyyyMMdd.
var culturaOriginal = System.Globalization.CultureInfo.CurrentCulture;
try
{
    foreach (var cultura in new[] { "es-AR", "en-US" })
    {
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(cultura);
        Check((DateTime)IvaVentasDatos.ConvertirValor("cm_cae_vto", "20251130", typeof(DateTime))! == new DateTime(2025, 11, 30), $"CAE yyyyMMdd independiente de cultura {cultura}");
        Check((DateTime)IvaVentasDatos.ConvertirValor("cm_cae_vto", "20251104", typeof(DateTime))! == new DateTime(2025, 11, 4), $"CAE no invierte mes/día en {cultura}");
    }
}
finally { System.Globalization.CultureInfo.CurrentCulture = culturaOriginal; }
Check(IvaVentasDatos.ConvertirValor("cm_cae_vto", DBNull.Value, typeof(DateTime)) == null &&
      IvaVentasDatos.ConvertirValor("cm_cae_vto", null, typeof(DateTime)) == null, "CAE nulo se conserva, sin fecha ficticia");
var fechaSql = new DateTime(2025, 11, 30, 12, 34, 56);
Check(Equals(IvaVentasDatos.ConvertirValor("fecha", fechaSql, typeof(DateTime)), fechaSql), "Fechas SQL tipadas no cambian");
Check((DateTime)IvaVentasDatos.ConvertirValor("cm_cae_vto", " 20240229 ", typeof(DateTime))! == new DateTime(2024, 2, 29), "Fecha compacta bisiesta y espacios CHAR");
foreach (var invalida in new[] { "20250229", "20251301", "00000000", "", "        ", "04/11/2025", "dato-privado" })
{
    try { IvaVentasDatos.ConvertirValor("cm_cae_vto", invalida, typeof(DateTime)); throw new Exception("Se aceptó una fecha inválida"); }
    catch (IvaVentasDatoInvalidoException ex)
    {
        Check(ex.Campo == "cm_cae_vto" && ex.InnerException == null && !ex.Message.Contains("dato-privado"), "Fecha inválida rechazada con diagnóstico de campo, sin valores fiscales");
    }
}
Check(Equals(IvaVentasDatos.ConvertirValor("total", 123456789.12m, typeof(decimal)), 123456789.12m), "Conversión de importes sin cambios");
Check(Equals(IvaVentasDatos.ConvertirValor("cae", "00123456789012", typeof(string)), "00123456789012"), "Identificadores conservan ceros");
var errorDatos = IvaVentasCliente.LeerErrorDatos("{\"codigo\":\"IVA_DATO_INVALIDO\",\"campo\":\"cm_cae_vto\",\"mensaje\":\"no propagar este texto externo\"}");
Check(errorDatos?.Campo == "cm_cae_vto" && !errorDatos.Message.Contains("texto externo"), "Sitio reconstruye mensaje controlado de datos inválidos");
Check(IvaVentasCliente.LeerErrorDatos("<html>error</html>") == null &&
      IvaVentasCliente.LeerErrorDatos("{\"codigo\":\"OTRO\",\"campo\":\"cm_cae_vto\"}") == null &&
      IvaVentasCliente.LeerErrorDatos("{\"codigo\":\"IVA_DATO_INVALIDO\",\"campo\":\"dato-no-autorizado\"}") == null, "No propagar HTML, códigos ni campos ajenos al contrato");
Check(f.Periodo == hoy.Year && f.Mes == hoy.Month && !f.Fechas && !f.PorCuit && !f.PorSucursal && !f.PorComprobante && !f.PorCaja, "Inicialización: período actual, opcionales apagados");
Check(IvaVentasFiltro.Inicial(5000).Registros == 500 && IvaVentasFiltro.Inicial(0).Registros == 100, "Límite 500 y fallback 100");
f.Periodo = hoy.Year - 5; f.Validar(); Check(true, "Sexto año incluido");
f.Periodo--; Reject(() => f.Validar(), "Séptimo año rechazado");
f = Filter(); f.Mes = 13; Reject(() => f.Validar(), "Mes inválido rechazado");
f = Filter(); f.Periodo = 2024; f.Mes = 2; f.Fechas = true; f.Desde = new DateTime(2024, 2, 1); f.Hasta = new DateTime(2024, 2, 29);
f.Validar(hoy: new DateTime(2026, 10, 14)); Check(true, "Rango histórico y año bisiesto");
f.Hasta = new DateTime(2024, 3, 1); Reject(() => f.Validar(hoy: new DateTime(2026, 10, 14)), "Fecha fuera del período");
f.Hasta = new DateTime(2024, 2, 1); f.Desde = new DateTime(2024, 2, 2); Reject(() => f.Validar(hoy: new DateTime(2026, 10, 14)), "Fechas invertidas");
f = Filter(); f.PorCuit = true; f.Cuit = "texto%"; f.Validar(); Check(true, "CUIT sin formato ni algoritmo");
f.Cuit = new string('1', 16); Reject(() => f.Validar(), "CUIT respeta varchar(15)");
f = Filter(); f.PorCaja = true; Reject(() => f.Validar(), "Filtro activo exige selección");
f.Cajas = ["0001", "0002"]; f.Validar(); Check(true, "Selección múltiple válida");
f.Cajas = ["%"]; Reject(() => f.Validar(), "No permitir comodín en lista de IDs");
f.Cajas = ["1,2"]; Reject(() => f.Validar(), "No permitir separadores en ID");
f = Filter(); f.Registros = 501; Reject(() => f.Validar(), "Más de 500 por página rechazado");
f.Registros = 500; f.Pagina = int.MaxValue; Reject(() => f.Validar(), "Overflow OFFSET rechazado");
f = Filter(); f.Pagina = 3; f.Registros = 500; f.PorCuit = true; f.Cuit = "%301%";
f.PorSucursal = true; f.Sucursales = ["0001", "0002", "0001"];
f.PorComprobante = true; f.Comprobantes = ["001", "002"]; f.PorCaja = true; f.Cajas = ["0003"];
var page = IvaVentasDatos.Parametros(f, false).ToDictionary(p => p.ParameterName, p => p.Value);
var all = IvaVentasDatos.Parametros(f, true).ToDictionary(p => p.ParameterName, p => p.Value);
Check((int)page["@registros"] == 500 && (int)page["@pagina"] == 3, "Parámetros de página");
Check((int)all["@registros"] == int.MaxValue && (int)all["@pagina"] == 1, "Exportar pide int.MaxValue, página 1");
Check(page.Where(p => p.Key is not "@pagina" and not "@registros").All(p => Equals(p.Value, all[p.Key])), "Mismos filtros en consulta y exportación");
Check((string)all["@adm_list"] == "0001,0002" && (string)all["@cuit_like"] == "%301%", "IDs sin duplicar y patrón CUIT intacto");
f = Filter(); f.Cuit = "ignorar"; f.Cajas = ["1234"]; all = IvaVentasDatos.Parametros(f, true).ToDictionary(p => p.ParameterName, p => p.Value);
Check((string)all["@cuit_like"] == "%" && (string)all["@caja_list"] == "", "Filtros deshabilitados no se envían como activos");
f.Pagina = -7; f.Registros = 5000; all = IvaVentasDatos.Parametros(f, true).ToDictionary(p => p.ParameterName, p => p.Value);
Check((int)all["@pagina"] == 1 && (int)all["@registros"] == int.MaxValue, "Exportación ignora exclusivamente navegación");

var fake = new FakeDatos();
var service = new IvaVentasServicio(fake, null!);
fake.Rows = Enumerable.Range(1, 100).Select(i => new IvaVentasDetalleDto { compte = i.ToString(), total_registros = 501 }).ToList();
var consulta = await service.Consultar(Filter(), default);
Check(consulta.Total == 501 && consulta.Paginas == 6 && consulta.Filas.Count == 100, "Metadatos de paginación: 501 / 100 = 6");
fake.EmptyLaterPages = true; f = Filter(); f.Pagina = 99;
consulta = await service.Consultar(f, default);
Check(consulta.Pagina == 1 && consulta.Total == 501 && fake.Calls == 3, "Página desaparecida vuelve a primera sin mostrar datos viejos");
fake.Rows = []; consulta = await service.Consultar(Filter(), default);
Check(consulta.Total == 0 && consulta.Filas.Count == 0, "Resultado vacío limpia grilla");
Reject(() => IvaVentasServicio.ValidarExportacionCompleta([]), "Sin registros no genera archivo vacío");
Reject(() => IvaVentasServicio.ValidarExportacionCompleta([new() { total_registros = 2, compte = "1" }]), "Exportación incompleta rechazada");
Reject(() => IvaVentasServicio.ValidarExportacionCompleta([new() { total_registros = 2 }, new() { total_registros = 2 }]), "Duplicados rechazados");

// Más de 5000, no solamente de 500: prueba real del contenido XLSX generado y reabierto.
var rows = Enumerable.Range(1, 5001).Select(i => new IvaVentasDetalleDto {
    total_registros = 5001, suc = "0001", tipo = "001", pv = "0002", compte = i.ToString("00000000"),
    repetido = "0", nombre = i == 1 ? "=HYPERLINK(\"https://example.invalid\",\"texto\")" : "Cliente de prueba",
    cuit = "00123456789", cae = "00123456789012", fecha = new DateTime(2026, 10, 1),
    cm_cae_vto = i == 1 ? (DateTime?)IvaVentasDatos.ConvertirValor("cm_cae_vto", "20251130", typeof(DateTime)) : null,
    no_gravado = 10.01m, exento = 20.02m, total = 123456789.12m, iva_2100 = -123.45m
}).ToList();
fake.Rows = rows; var todos = await service.Todos(Filter(), default);
Check(fake.Complete && todos.Count == 5001, "Servicio pide dataset completo");
var watch = System.Diagnostics.Stopwatch.StartNew();
var bytes = IvaVentasExcel.Generar(rows, Filter());
using (var ms = new MemoryStream(bytes))
using (var wb = new XLWorkbook(ms))
{
    var ws = wb.Worksheet(1);
    Check(ws.LastRowUsed()!.RowNumber() == 5006, "Excel incluye las 5001 filas");
    Check(ws.LastColumnUsed()!.ColumnNumber() == 30, "30 columnas sin metadatos de paginación");
    Check(ws.Cell(5, 12).GetString() == "No gravado" && ws.Cell(5, 13).GetString() == "Exento", "Conceptos separados y orden estable");
    Check(ws.Cell(6, 12).GetValue<decimal>() == 10.01m && ws.Cell(6, 13).GetValue<decimal>() == 20.02m, "Exento y no gravado no se suman");
    Check(ws.Cell(6, 24).GetValue<decimal>() == 123456789.12m && ws.Cell(6, 17).GetValue<decimal>() == -123.45m, "Importes grandes y negativos numéricos");
    Check(ws.Cell(6, 6).GetString() == "00000001" && ws.Cell(6, 10).GetString() == "00123456789" && ws.Cell(6, 27).GetString() == "00123456789012", "Número, CUIT y CAE preservan ceros");
    Check(ws.Cell(6, 2).DataType == XLDataType.DateTime, "Fechas reales de Excel");
    Check(ws.Cell(6, 28).DataType == XLDataType.DateTime && ws.Cell(6, 28).GetDateTime() == new DateTime(2025, 11, 30) && ws.Cell(7, 28).IsEmpty(), "Excel conserva vencimiento CAE convertido y nulos");
    Check(ws.Cell(6, 9).DataType == XLDataType.Text && !ws.Cell(6, 9).HasFormula && ws.Cell(6, 9).GetString().StartsWith("=HYPERLINK"), "Texto no se ejecuta como fórmula");
    Check(ws.Cell(6, 14).IsEmpty(), "NULL no se convierte silenciosamente en cero");
}
Console.WriteLine($"XLSX 5001 x 30: {bytes.Length:N0} bytes; generación + reapertura {watch.ElapsedMilliseconds:N0} ms (datos sintéticos, no SQL).");
Check(IvaVentasCliente.Url("https://api.example/apireport", "api/iva-ventas/excel").AbsoluteUri == "https://api.example/apireport/api/iva-ventas/excel", "URL conserva subaplicación IIS");
Check(IvaVentasCliente.Url("https://localhost:5001/", "api/iva-ventas/opciones").AbsoluteUri == "https://localhost:5001/api/iva-ventas/opciones", "URL desarrollo en raíz");
Reject(() => IvaVentasCliente.Url("", "api/iva-ventas/excel"), "Configuración de URL ausente rechazada");

var usuarios = DispatchProxy.Create<IApiUsuarioServicio, UsuarioProxy>();
var proxy = (UsuarioProxy)(object)usuarios;
service = new IvaVentasServicio(fake, usuarios);
var claims = new[] { new Claim("user", "prueba"), new Claim("AdmId", "0001#Sucursal"), new Claim("perfiles", "[{\"perfil_id\":\"P1\"}]") };
var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
Check(!service.TieneAcceso(new ClaimsPrincipal(), "P1", "M"), "Rechaza anónimo");
Check(!service.TieneAcceso(user, "P2", "M") && proxy.Calls == 0, "Perfil no incluido en token rechazado antes de SQL");
Check(!service.TieneAcceso(user, "P1", "M"), "Sin permiso imp_lv_det denegado");
proxy.Allow = true; Check(service.TieneAcceso(user, "P1", "M"), "Menú autorizado habilita consulta");
var forced = new ClaimsPrincipal(new ClaimsIdentity(claims.Append(new Claim("cambio_clave_obligatorio", "true")), "test"));
Check(!service.TieneAcceso(forced, "P1", "M"), "Cambio obligatorio bloquea reportes");
// Regresión del arranque: el cliente no debe exponer campos System.Text.Json 9
// al descubrimiento MVC de una aplicación net8 con otro grafo de dependencias.
var jsonField = typeof(IvaVentasCliente).GetField("Json", BindingFlags.Static | BindingFlags.NonPublic)!;
Check(jsonField.FieldType == typeof(JsonSerializerSettings), "Cliente IVA usa exclusivamente configuración Newtonsoft");
var jsonSettings = (JsonSerializerSettings)jsonField.GetValue(null)!;
Check(jsonSettings.TypeNameHandling == TypeNameHandling.None, "JSON no permite activación de tipos arbitrarios");
f = Filter(); f.PorCaja = true; f.Cajas = ["0001", "0002"]; f.PorCuit = true; f.Cuit = "%301%";
var jsonFiltro = JObject.Parse(JsonConvert.SerializeObject(f, jsonSettings));
Check(jsonFiltro["porCaja"]!.Value<bool>() && jsonFiltro["cajas"]!.Count() == 2 && jsonFiltro["cuit"]!.Value<string>() == "%301%", "JsonConvert conserva contrato camelCase y filtros");
var jsonPagina = "{\"filas\":[{\"total_registros\":1,\"compte\":\"00000001\",\"total\":123456789.12,\"fecha\":\"2026-10-14T00:00:00\"}],\"total\":1,\"pagina\":1,\"registros\":100}";
var desPagina = JsonConvert.DeserializeObject<IvaVentasPagina>(jsonPagina, jsonSettings)!;
Check(desPagina.Filas[0].total == 123456789.12m && desPagina.Filas[0].compte == "00000001" && desPagina.Filas[0].fecha == new DateTime(2026, 10, 14), "JsonConvert conserva importes, fechas e identificadores");
var desOpciones = JsonConvert.DeserializeObject<IvaVentasOpciones>("{\"cajas\":[{\"id\":\"0001\",\"texto\":\"Caja 1\"}]}", jsonSettings)!;
Check(desOpciones.Cajas.Single() == new IvaOpcion("0001", "Caja 1"), "JsonConvert deserializa las opciones de las listas");
var malformed = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("user", "prueba"), new Claim("AdmId", "0001#Sucursal"), new Claim("perfiles", "no es json") }, "test"));
Check(!service.TieneAcceso(malformed, "P1", "M"), "Claim de perfiles inválido denegado con Newtonsoft");
Console.WriteLine($"PASS: {passed} verificaciones.");

public class FakeDatos : IIvaVentasDatos
{
    public List<IvaVentasDetalleDto> Rows = [];
    public bool EmptyLaterPages, Complete;
    public int Calls;
    public Task<List<IvaVentasDetalleDto>> Consultar(IvaVentasFiltro filtro, bool completo, CancellationToken ct)
    { Calls++; Complete = completo; return Task.FromResult(EmptyLaterPages && !completo && filtro.Pagina > 1 ? new List<IvaVentasDetalleDto>() : Rows); }
    public Task<IvaVentasOpciones> Opciones(CancellationToken ct) => Task.FromResult(new IvaVentasOpciones());
}
public class UsuarioProxy : DispatchProxy
{
    public bool Allow;
    public int Calls;
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        if (method?.Name != "ObtenerMenu") throw new NotSupportedException();
        Calls++;
        return Allow ? new List<MenuPpalDto> { new() { mnu_item_id = "imp_lv_det" } } : new List<MenuPpalDto>();
    }
}
