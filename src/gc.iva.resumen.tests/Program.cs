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
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Implementacion.Libros;
using iTextSharp.text.pdf;
using Newtonsoft.Json;

// Sólo datos sintéticos: ninguna conexión SQL ni escritura de datos de negocio.
int passed = 0;
void Check(bool ok, string name) { if (!ok) throw new Exception(name); Console.WriteLine("OK: " + name); passed++; }
void Reject(Action act, string name) { try { act(); } catch (ArgumentException) { Check(true, name); return; } catch (InvalidOperationException) { Check(true, name); return; } throw new Exception(name); }
var filtro = IvaVentasResumenFiltro.Inicial();
Check(filtro.Periodo == IvaVentasFiltro.HoyArgentina().Year, "Período inicial actual");
Reject(() => new IvaVentasResumenFiltro { Periodo = filtro.Periodo - 6, Mes = 1 }.Validar(), "Rechazo fuera de seis años");
Reject(() => new IvaVentasResumenFiltro { Periodo = filtro.Periodo, Mes = 13 }.Validar(), "Rechazo mes inválido");
var parametros = IvaVentasResumenDatos.Parametros(new() { Periodo = filtro.Periodo, Mes = 2 });
Check(parametros.Length == 2 && (string)parametros[1].Value == "02" && parametros[0].Size == 4 && parametros[1].Size == 2, "Contrato SP: sólo año y mes varchar");
IvaVentasResumenFila Row(string id, string op, short orden, string descripcion, decimal valor) => new()
{ adm_id = id, adm_nombre = "Sucursal sintética", tipo_ope = op, orden = orden, tipo = "GR", tipo_desc = descripcion, ali = 21m, fa_cf_imp = valor };
var rows = new List<IvaVentasResumenFila> {
    Row("9999", "V", 2, "Gravado 21%", 10000000000.25m), Row("0001", "C", 2, "Gravado 21%", -0.05m),
    Row("0001", "V", 2, "Gravado 21%", 10000000000.25m), Row("9999", "C", 2, "Gravado 21%", -0.05m),
    Row("0001", "V", 11, "Percepción: texto del SP sin normalizar", 0m)
};
var libro = IvaVentasResumenLibro.Crear(filtro, rows);
Check(libro.Grupos[0].Id == "0001" && libro.Grupos[^1].EsTotal, "Sucursales antes del consolidado");
Check(libro.Grupos[0].Totales[5] == 10000000000.20m && libro.Grupos[^1].Totales[5] == 10000000000.20m, "Centavos exactos y consolidado no duplicado");
Check(libro.Grupos[0].Filas[^1].tipo_ope == "C" && libro.Cantidad == rows.Count, "Orden operaciones y preservación de filas cero");
Check(libro.Grupos[0].Filas[1].tipo_desc == rows[4].tipo_desc, "Descripción residual conservada");
Reject(() => IvaVentasResumenLibro.Crear(filtro, rows.Where(x => x.adm_id != "9999").ToList()), "Consolidado ausente rechazado");
Check(JsonConvert.DeserializeObject<IvaVentasResumenFila>(JsonConvert.SerializeObject(rows[0]))!.fa_cf_imp == 10000000000.25m, "Newtonsoft conserva decimal");
var xlsx = IvaVentasResumenReporte.Excel(libro);
using (var wb = new XLWorkbook(new MemoryStream(xlsx))) {
    Check(wb.Worksheets.Count == 2, "Excel contiene sucursal y Total Libro separados");
    var ws = wb.Worksheet("Sucursal 0001");
    Check(ws.Cell(7, 3).GetValue<decimal>() == 10000000000.25m && ws.Cell(10, 7).GetValue<decimal>() == 10000000000.20m, "Excel conserva importes y total");
    Check(ws.Cell(9, 3).GetValue<decimal>() == -0.05m, "Excel conserva signo NC");
}
var enormes = IvaVentasResumenLibro.Crear(filtro, [Row("9999", "V", 2, "Prueba precisión", 12345678901234.56m)]);
using (var wb = new XLWorkbook(new MemoryStream(IvaVentasResumenReporte.Excel(enormes))))
    Check(wb.Worksheet(1).Cell(7, 3).DataType == XLDataType.Text && wb.Worksheet(1).Cell(7, 3).GetString() == "12345678901234,56", "Excel extremo como texto exacto");
Reject(() => IvaVentasResumenReporte.Excel(IvaVentasResumenLibro.Crear(filtro, [])), "Sin filas no hay archivo vacío");
var many = Enumerable.Range(0, 120).Select(i => Row("9999", "V", 2, $"CONCEPTO-{i:000} Gravado 21%", 14278505813.70m)).ToList();
var multipagina = IvaVentasResumenLibro.Crear(filtro, many);
var empresa = new EmpresaGeco { Nombre = "GECO - PRUEBA SINTÉTICA", RazonSocial = "Café América - datos de prueba", CUIT = "PRUEBA", Direccion = "No corresponde a un libro fiscal real" };
var pdf = IvaVentasResumenReporte.Pdf(multipagina, empresa);
using (var reader = new PdfReader(pdf)) {
    Check(reader.NumberOfPages > 1, "PDF multipágina");
    Check(Enumerable.Range(1, reader.NumberOfPages).All(i => reader.GetPageSizeWithRotation(i).Width > reader.GetPageSizeWithRotation(i).Height), "Todas las páginas A4 horizontal");
}
var usuarios = DispatchProxy.Create<IApiUsuarioServicio, UsuarioProxy>();
var proxy = (UsuarioProxy)(object)usuarios;
var servicio = new IvaVentasResumenServicio(new DatosFake(rows), usuarios);
var claims = new[] { new Claim("user", "prueba"), new Claim("AdmId", "0001#Sucursal"), new Claim("perfiles", "[{\"perfil_id\":\"P1\"}]") };
var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
Check(!servicio.TieneAcceso(new ClaimsPrincipal(), "P1", "M"), "Anónimo denegado");
Check(!servicio.TieneAcceso(user, "P2", "M") && proxy.Calls == 0, "Perfil ajeno rechazado antes de consultar menú");
Check(!servicio.TieneAcceso(user, "P1", "M"), "Sin imp_lv_res denegado");
proxy.Allow = true; Check(servicio.TieneAcceso(user, "P1", "M"), "Permiso imp_lv_res aceptado");
var forced = new ClaimsPrincipal(new ClaimsIdentity(claims.Append(new Claim("cambio_clave_obligatorio", "true")), "test"));
Check(!servicio.TieneAcceso(forced, "P1", "M"), "Cambio de clave obligatorio bloquea consulta");
Check((await servicio.Consultar(filtro, default)).Count == rows.Count, "Servicio devuelve todas las filas");
var docs = new DocsManager();
IvaVentasResumenDocumentos.Configurar(docs); IvaVentasResumenDocumentos.Configurar(docs);
var modulo = docs.Modulos.Single(m => m.Id == IvaVentasResumenDocumentos.Modulo);
Check(modulo.Print && !modulo.Export && !modulo.Email && !modulo.Whatsapp && !modulo.Reportes.Single().PermiteEnlacePublico, "Gestor aditivo idempotente, sin enlace público");
if (args.Length == 1) {
    Directory.CreateDirectory(args[0]);
    File.WriteAllBytes(System.IO.Path.Combine(args[0], "resumen-sintetico.pdf"), pdf);
    File.WriteAllBytes(System.IO.Path.Combine(args[0], "resumen-sintetico.xlsx"), xlsx);
}
Console.WriteLine($"PASS: {passed} verificaciones sintéticas. Sin SQL.");
public class DatosFake(List<IvaVentasResumenFila> rows) : IIvaVentasResumenDatos {
    public Task<List<IvaVentasResumenFila>> Consultar(IvaVentasResumenFiltro f, CancellationToken ct) => Task.FromResult(rows);
}
public class UsuarioProxy : DispatchProxy {
    public bool Allow; public int Calls;
    protected override object? Invoke(MethodInfo? method, object?[]? args) {
        if (method?.Name != "ObtenerMenu") throw new NotSupportedException();
        Calls++; return Allow ? new List<MenuPpalDto> { new() { mnu_item_id = "imp_lv_res" } } : new List<MenuPpalDto>();
    }
}
