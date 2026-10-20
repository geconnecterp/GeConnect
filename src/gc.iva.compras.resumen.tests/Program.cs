using ClosedXML.Excel;
using System.Reflection;
using System.Security.Claims;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Servicios.Libros;
using gc.infraestructura.Dtos.Users;
using gc.api.core.Servicios.Reportes;
using gc.api.infra.Datos.Implementacion;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.core.Servicios.Implementacion.Libros;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

// Fixtures sintéticos: jamás conecta con SQL ni ejecuta un cierre o escritura de negocio.
int checks = 0;
void Check(bool ok, string label) { if (!ok) throw new Exception(label); checks++; Console.WriteLine("OK " + label); }
void Reject(Action action, string label) { try { action(); } catch (ArgumentException) { Check(true, label); return; } throw new Exception(label); }
var filtro = new IvaComprasResumenFiltro { Periodo = 2025, Mes = 10 };
var libro = Fixture(filtro);
libro.Validar();
var usuarios = DispatchProxy.Create<IApiUsuarioServicio, UsuariosFake>();
var permisos = (UsuariosFake)(object)usuarios;
var servicio = new IvaComprasResumenServicio(new DatosFake(libro), usuarios);
var claims = new[] { new Claim("user", "QA"), new Claim("AdmId", "0001#QA"), new Claim("perfiles", "[{\"perfil_id\":\"P1\"}]") };
var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
Check(!servicio.TieneAcceso(new ClaimsPrincipal(), "P1", "C"), "Anónimo denegado");
Check(!servicio.TieneAcceso(user, "P2", "C") && permisos.Llamadas == 0, "Perfil ajeno denegado antes de consultar menú");
Check(!servicio.TieneAcceso(user, "P1", "C"), "Sin imp_lc_res no se permite consultar");
permisos.MenuItem = "imp_lv_res";
Check(!servicio.TieneAcceso(user, "P1", "C"), "Permiso de Ventas no habilita Compras");
permisos.MenuItem = "imp_lc_res";
Check(servicio.TieneAcceso(user, "P1", "C"), "Permiso propio imp_lc_res aceptado");
var forzado = new ClaimsPrincipal(new ClaimsIdentity(claims.Append(new Claim("cambio_clave_obligatorio", "true")), "test"));
Check(!servicio.TieneAcceso(forzado, "P1", "C"), "Cambio de clave obligatorio bloquea módulo");
Check((await servicio.Consultar(filtro, default)).Cantidad == libro.Cantidad, "Servicio devuelve completos los cuatro conjuntos");
Check(libro.Bienes.Diferencia == 0, "Dif. cero exacta en BI");
Check(libro.Otros.Diferencia == -30394.99m, "Dif. = Contabilidad - Libro IVA, conserva signo");
Check(libro.Bienes.TotalResumen == libro.Bienes.Resumen.Sum(r => r.importe), "Total resumen suma todos los conceptos sin reinterpretarlos");
Check(libro.Bienes.Resumen.Count(r => r.tipo == "IV") == 2, "No se agrupan IV ni se pierden NC");
Check(libro.Bienes.Control[2].ccb.StartsWith("00"), "Cuenta conserva ceros iniciales");
Check((int)InfoReporte.R103_Planilla_Elaboracion_Y_Fraccion == 103 && (int)InfoReporte.R104_LibroIvaVentasResumen == 104, "Reportes anteriores sin cambios");
Check(IvaComprasResumenDocumentos.Reporte == 105 && IvaComprasResumenReporte.Id == 105, "Gestor y PDF usan 105");
Check(Enum.GetValues<InfoReporte>().Select(v => (int)v).Distinct().Count() == Enum.GetValues<InfoReporte>().Length, "Sin IDs de reporte duplicados");
var p = IvaComprasResumenDatos.Parametros(filtro);
Check(p.Length == 2 && p[0].Size == 4 && p[1].Size == 2 && (string)p[1].Value == "10", "SP recibe sólo período y mes varchar");
Reject(() => new IvaComprasResumenFiltro { Periodo = 2020, Mes = 1 }.Validar(new DateTime(2026, 10, 19)), "Año fuera de rango rechazado");
Reject(() => new IvaComprasResumenFiltro { Periodo = 2025, Mes = 13 }.Validar(new DateTime(2026, 10, 19)), "Mes inválido rechazado");
var roundtrip = JsonConvert.DeserializeObject<IvaComprasResumenLibro>(JsonConvert.SerializeObject(libro), new JsonSerializerSettings { FloatParseHandling = FloatParseHandling.Decimal })!;
roundtrip.Validar();
Check(roundtrip.Bienes.TotalLibro == libro.Bienes.TotalLibro && roundtrip.Cantidad == libro.Cantidad, "Newtonsoft preserva importes y filas");
try { JsonConvert.DeserializeObject<IvaComprasResumenLibro>("{}"); throw new Exception("Contrato incompleto aceptado"); }
catch (JsonSerializationException) { Check(true, "Contrato parcial rechazado, no se fabrica libro en cero"); }
var docs = new DocsManager(); IvaComprasResumenDocumentos.Configurar(docs); IvaComprasResumenDocumentos.Configurar(docs);
Check(docs.Modulos.Count(m => m.Id == IvaComprasResumenDocumentos.Modulo) == 1, "Registro documental idempotente");
Check(!docs.Modulos.Single().Reportes.Single().PermiteEnlacePublico, "Sin exposición anónima del libro");
var xlsx = IvaComprasResumenReporte.Excel(libro);
using (var wb = new XLWorkbook(new MemoryStream(xlsx)))
{
    Check(wb.Worksheets.Select(w => w.Name).SequenceEqual(new[] { "Control BI", "Resumen BI", "Control Otros", "Resumen Otros" }), "Excel con cuatro hojas completas");
    Check(wb.Worksheet("Control BI").Cell(7, 1).GetString() == libro.Bienes.Control[2].ccb, "Cuenta textual exacta en Excel");
    Check(wb.Worksheet("Control BI").Cell(5, 4).GetValue<decimal>() == libro.Bienes.Control[0].importe_iva, "Importe Excel exacto");
    Check(!wb.Worksheet("Control BI").CellsUsed().Any(c => c.GetString().StartsWith("Dif.")), "Excel omite Dif. cero");
    Check(wb.Worksheet("Control Otros").CellsUsed().Count(c => c.GetString().StartsWith("Dif.")) == 1, "Excel muestra Dif. distinta de cero");
    Check(wb.Worksheet("Resumen BI").LastRowUsed()!.RowNumber() == libro.Bienes.Resumen.Count + 5, "Resumen Excel sin límite de filas");
}
var empresa = new EmpresaGeco { Nombre = "Café América · PRUEBA SINTÉTICA", CUIT = "DATOS NO FISCALES", Direccion = "Verificación de diseño" };
var pdf = IvaComprasResumenReporte.Pdf(libro, empresa);
using (var reader = new PdfReader(pdf))
{
    Check(reader.NumberOfPages == 1, "PDF normal con cuatro cuadros en una página");
    var size = reader.GetPageSizeWithRotation(1);
    Check(Math.Abs(size.Width - 841.89) < 1 && Math.Abs(size.Height - 595.28) < 1, "PDF A4 horizontal");
    var texto = System.Text.Encoding.Latin1.GetString(reader.GetPageContent(1));
    Check(texto.Split("Dif.:").Length - 1 == 1, "PDF sólo muestra diferencia no cero");
    Check(texto.Contains("-30.394,99") && texto.Contains("1.679.693.504,31"), "PDF conserva números grandes y diferencia");
}
var grande = Fixture(filtro);
grande.Bienes.Control[2].importe = 12345678901234.56m;
using (var wb = new XLWorkbook(new MemoryStream(IvaComprasResumenReporte.Excel(grande))))
    Check(wb.Worksheet("Control BI").Cell(7, 3).DataType == XLDataType.Text && wb.Worksheet("Control BI").Cell(7, 3).GetString() == "12345678901234,56", "Excel extremo no pierde centavos");
var muchos = Fixture(filtro);
for (var i = 0; i < 100; i++) muchos.Bienes.Resumen.Add(new() { tipo = "GR", tipo_desc = "EXTENSO-" + i.ToString("000"), importe = i });
using (var reader = new PdfReader(IvaComprasResumenReporte.Pdf(muchos, empresa)))
{
    var texto = string.Join("\n", Enumerable.Range(1, reader.NumberOfPages).Select(i => System.Text.Encoding.Latin1.GetString(reader.GetPageContent(i))));
    Check(reader.NumberOfPages > 1 && texto.Contains("EXTENSO-099"), "PDF extenso conserva última fila sin truncar");
}
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews().AddApplicationPart(typeof(gc.sitio.Areas.Libros.Controllers.IvaComprasResumenController).Assembly);
var app = builder.Build();
async Task<string> Grilla(HttpContext http, IvaComprasResumenLibro modelo)
{
    var action = new ActionContext(http, new RouteData(), new ActionDescriptor());
    var engine = http.RequestServices.GetRequiredService<ICompositeViewEngine>();
    var view = engine.GetView(null, "/Areas/Libros/Views/IvaComprasResumen/_Grilla.cshtml", false);
    if (!view.Success) throw new Exception("No se encontró la vista real: " + string.Join(",", view.SearchedLocations));
    using var sw = new StringWriter();
    var data = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = modelo };
    var temp = new TempDataDictionary(http, http.RequestServices.GetRequiredService<ITempDataProvider>());
    await view.View.RenderAsync(new ViewContext(action, view.View, data, temp, sw, new HtmlHelperOptions()));
    return sw.ToString();
}
using (var scope = app.Services.CreateScope())
{
    var html = await Grilla(new DefaultHttpContext { RequestServices = scope.ServiceProvider }, libro);
    Check(html.Split("class=\"icr-cuadro ").Length - 1 == 4, "Vista real contiene cuatro grids");
    Check(html.Split("class=\"icr-diferencia\"").Length - 1 == 1, "Vista real omite Dif. cero");
    Check(!html.Contains("overflow:auto"), "Vista sin contenedores de scroll");
    var destino = System.IO.Path.GetFullPath(".artifacts/iva-compras-resumen-20261019/qa");
    Directory.CreateDirectory(destino);
    await File.WriteAllBytesAsync(System.IO.Path.Combine(destino, "resumen-sintetico.xlsx"), xlsx);
    await File.WriteAllBytesAsync(System.IO.Path.Combine(destino, "resumen-sintetico.pdf"), pdf);
}
Console.WriteLine($"TOTAL: {checks} comprobaciones correctas");
if (args.Contains("--serve"))
{
    // Host QA local aislado: datos sintéticos; no ejecuta Program ni autenticación de gc.sitio.
    var raiz = Directory.GetCurrentDirectory();
    app.MapGet("/qa", () => Results.Content("""
    <!doctype html><html lang="es"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
    <link rel="stylesheet" href="/bootstrap.css"><link rel="stylesheet" href="/core.css"><link rel="stylesheet" href="/golden.css"><link rel="stylesheet" href="/site.css"><link rel="stylesheet" href="/module.css?v=2"></head>
    <body style="margin:0;background:#f7f6f1;color:#363128;font-family:Arial,sans-serif"><header style="height:86px;padding:15px 24px;background:#363128;color:white">GECO · QA SINTÉTICA · NO ES UN LIBRO FISCAL</header><h1 style="font-size:16px;height:28px;margin:0;padding-left:24px">Libro IVA Compras - Consulta Resumen</h1>
    <main style="padding:0 24px"><section id="iva-compras-resumen" data-reporte="105" data-consulta="/consultar" data-excel="/excel" data-perfil="QA" data-menu="QA">
    <nav class="ivr-navigation"><button id="btnFiltro" class="btn btn-primary btn-sm">Filtro</button><button id="btnDetalle" class="btn btn-primary btn-sm">Detalle</button><button id="ivr-imprimir" class="btn btn-primary btn-sm">Imprimir</button><button id="ivr-excel" class="btn btn-primary btn-sm">Exportar Excel completo</button></nav>
    <form id="ivr-filtros"><fieldset class="filter-golden"><div id="divFiltro"><label>Año <select name="Periodo"><option selected>2026</option><option>2025</option></select></label> <label>Mes <select name="Mes"><option value="10" selected>Octubre</option><option value="11">Noviembre</option></select></label></div><div class="ivr-actions"><button type="reset" id="btnCancel" class="btn btn-danger">Inicializar</button><button id="btnBuscar" type="submit" class="btn btn-success">Ver</button></div></fieldset></form>
    <div id="ivr-mensaje" role="status"></div><div id="divDetalle" hidden><div id="ivr-resultado"></div><div id="ivr-cargando" hidden><span class="spinner-border"></span><span id="ivr-espera"></span></div></div></section></main>
    <footer style="height:30px;padding-left:24px;font-size:12px">Prueba visual aislada · datos sintéticos</footer>
    <script>var arrRepoParams=[];function ReporteResetCeldaEnArre(id){arrRepoParams[id-1]=null;}function cargarReporteEnArre(id,p){arrRepoParams[id-1]=p;}function invocacionGestorDoc(p){document.getElementById('ivr-mensaje').textContent='QA: gestor invocado con reporte '+p.reportesPreseleccionados;}</script><script src="/module.js"></script></body></html>
    """, "text/html"));
    app.MapGet("/module.css", () => Results.File(System.IO.Path.Combine(raiz, "gc.sitio/wwwroot/css/modules/iva-compras-resumen.css"), "text/css"));
    app.MapGet("/module.js", () => Results.File(System.IO.Path.Combine(raiz, "gc.sitio/wwwroot/js/app/libros/iva-compras-resumen.js"), "text/javascript"));
    app.MapGet("/bootstrap.css", () => Results.File(System.IO.Path.Combine(raiz, "gc.sitio/wwwroot/lib/bootstrap/css/bootstrap.css"), "text/css"));
    app.MapGet("/core.css", () => Results.File(System.IO.Path.Combine(raiz, "gc.sitio/wwwroot/css/core.css"), "text/css"));
    app.MapGet("/golden.css", () => Results.File(System.IO.Path.Combine(raiz, "gc.sitio/wwwroot/css/golden-overlay.css"), "text/css"));
    app.MapGet("/site.css", () => Results.File(System.IO.Path.Combine(raiz, "gc.sitio/wwwroot/css/site.css"), "text/css"));
    app.MapPost("/consultar", async (HttpContext http) => { await Task.Delay(1400); var form = await http.Request.ReadFormAsync(); var f = new IvaComprasResumenFiltro { Periodo = int.Parse(form["Periodo"]!), Mes = int.Parse(form["Mes"]!) }; await Results.Content(await Grilla(http, Fixture(f)), "text/html").ExecuteAsync(http); });
    app.MapPost("/excel", () => Results.File(xlsx, IvaComprasResumenReporte.MimeExcel, "resumen-sintetico.xlsx"));
    await app.RunAsync("http://127.0.0.1:17865");
}

static IvaComprasResumenLibro Fixture(IvaComprasResumenFiltro filtro)
{
    var libro = new IvaComprasResumenLibro { Filtro = filtro };
    foreach (var b in libro.Bloques)
    {
        var bienes = b.Codigo == "BI";
        var totalLibro = bienes ? 1679693504.31m : 117606438.73m;
        var totalContabilidad = bienes ? totalLibro : 117576043.74m;
        b.Control.Add(new() { tipo = "AA", ccb_desc = "IVA Libro", importe_iva = totalLibro + 100m });
        b.Control.Add(new() { tipo = "BB", ccb_desc = "IVA Libro NC", importe_iva = -100m });
        for (var i = 0; i < 9; i++) b.Control.Add(new() { tipo = bienes ? "BI" : "PS", ccb = "001201" + i, ccb_desc = "IVA Crédito Fiscal · cuenta de ejemplo", importe = i == 0 ? totalContabilidad : 0m });
        decimal[] alicuotas = [2.5m, 5m, 10.5m, 21m, 27m];
        foreach (var a in alicuotas) b.Resumen.Add(new() { tipo = "GR", tipo_desc = $"Gravado al {a:0.00}", importe = 1000000000.25m });
        b.Resumen.Add(new() { tipo = "IV", tipo_desc = "IVA Crédito Fiscal", importe = totalLibro + 100m });
        b.Resumen.Add(new() { tipo = "IV", tipo_desc = "IVA NC", importe = -100m });
        foreach (var (tipo, texto) in new[] { ("NG", "Exento"), ("MT", "Monotributistas"), ("II", "Impuestos Internos"), ("ON", "Otros No Grav."), ("PE", "Percepciones") }) b.Resumen.Add(new() { tipo = tipo, tipo_desc = texto, importe = 123456789.01m });
    }
    return libro;
}

public class DatosFake(IvaComprasResumenLibro libro) : IIvaComprasResumenDatos
{
    public Task<IvaComprasResumenLibro> Consultar(IvaComprasResumenFiltro f, CancellationToken ct) => Task.FromResult(libro);
}
public class UsuariosFake : DispatchProxy
{
    public int Llamadas;
    public string MenuItem = "";
    protected override object? Invoke(MethodInfo? metodo, object?[]? args)
    {
        if (metodo?.Name != "ObtenerMenu") throw new NotSupportedException();
        Llamadas++;
        return new List<MenuPpalDto> { new() { mnu_item_id = MenuItem } };
    }
}
