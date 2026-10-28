using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Servicios.Libros;
using gc.api.core.Servicios.Reportes;
using gc.api.infra.Datos.Implementacion;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Implementacion.Libros;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Reflection;
using System.Security.Claims;

// Datos sintéticos y dobles de prueba: no abre SQL ni altera comprobantes.
var checks = 0;
void Check(bool valor, string texto) { if (!valor) throw new Exception("FAIL: " + texto); checks++; Console.WriteLine("OK: " + texto); }
async Task Reject<T>(Func<Task> action, string texto) where T : Exception
{ try { await action(); } catch (T) { Check(true, texto); return; } throw new Exception("FAIL: " + texto); }
var hoy = IvaVentasFiltro.HoyArgentina();
CertificadosSufridosFiltro Filtro(string imp = "IB") => new() { Impuesto = imp, Periodo = hoy.Year, Mes = 2 };
var f = Filtro(); f.Validar();
var inicial = CertificadosSufridosFiltro.Inicial();
Check(inicial.Periodo == hoy.Year && inicial.Mes == hoy.Month && !inicial.Fechas && !inicial.PorCuit && !inicial.SepararCierre && !inicial.AgrupadoPorCuit, "Inicial Argentina y opcionales apagados");
foreach (var imp in new[] { "IB", "IV", "GA", "LH", "X" }) { Filtro(imp).Validar(); Check(true, "Filtro admite códigos de catálogo de hasta dos caracteres: " + imp); }
foreach (var imp in new[] { "", "XXX", "'XX", "  " }) await Reject<ArgumentException>(() => { Filtro(imp).Validar(); return Task.CompletedTask; }, "Identificador SQL inválido rechazado");
f = Filtro(); f.Periodo = hoy.Year - 5; f.Validar(); f.Periodo--;
await Reject<ArgumentException>(() => { f.Validar(); return Task.CompletedTask; }, "Sigue ventana de seis años de Libro IVA");
f = Filtro(); f.Fechas = true; f.Desde = new(hoy.Year, 2, 2); f.Hasta = new(hoy.Year, 2, 1);
await Reject<ArgumentException>(() => { f.Validar(); return Task.CompletedTask; }, "Rango invertido rechazado");
f.Desde = new(hoy.Year, 1, 31); await Reject<ArgumentException>(() => { f.Validar(); return Task.CompletedTask; }, "Fecha activa fuera del período rechazada");
f = Filtro(); f.PorCuit = true;
foreach (var cuit in new[] { "", " ", new string('1', 16), "%305%", "30_12" }) { f.Cuit = cuit; await Reject<ArgumentException>(() => { f.Validar(); return Task.CompletedTask; }, "CUIT vacío/extenso/patrón rechazado por contrato final exacto"); }
f.Cuit = "30591773468"; f.Validar();
f.AgrupadoPorCuit = true; f.SepararCierre = true; f.Validar(); Check(!f.SepararCierre, "Agrupado elimina separación de corte no devuelta por el SP");
f = Filtro("IV"); f.SepararCierre = true; f.Validar(); Check(!f.SepararCierre, "Separación desactivada en servidor fuera de IB");
f = Filtro(); f.Desde = DateTime.MinValue; f.Hasta = null; f.Cuit = "obsoleto";
var pars = CertificadosSufridosDatos.Parametros(f);
Check(pars.Select(p => p.ParameterName).SequenceEqual(new[] { "@imp_id", "@periodo", "@mes", "@f", "@fecha_d", "@fecha_h", "@cuit", "@cuit_like" }), "Ocho parámetros exactos, sin corte, agrupación ni paginación SQL");
Check(pars[0].SqlDbType == SqlDbType.Char && pars[0].Size == 2 && pars[1].Size == 4 && pars[2].Size == 2 && pars[7].Size == 15, "Tipos/tamaños del SP entregado");
Check((string)pars[2].Value == "02" && (string)pars[7].Value == "%" && (DateTime)pars[4].Value == new DateTime(hoy.Year, 2, 1), "Fechas obligatorias y valores apagados ignorados");

DataTable Tabla()
{
    var t = new DataTable(); foreach (var p in typeof(CertificadoSufridoFila).GetProperties()) t.Columns.Add(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType);
    var row = t.NewRow(); row["origen"] = "C"; row["base"] = 9999999999999.99m; row["percepcion"] = -0.01m;
    row["cm_compte"] = "0001-00000001"; row["cm_fecha"] = new DateTime(2022, 10, 1); row["corte_ib_vtas"] = "S"; t.Rows.Add(row); return t;
}
using (var t = Tabla()) using (var r = t.CreateDataReader())
{
    Check(t.Columns.Count == 27, "Contrato detallado de 27 alias"); var map = CertificadosSufridosDatos.CrearMapeador(r); r.Read(); var fila = map(r);
    Check(fila.@base == 9999999999999.99m && fila.percepcion == -0.01m && fila.percepcion_lh == null, "Preserva precisión, negativos y NULL");
    Check(fila.cm_compte == "0001-00000001" && fila.cm_fecha == new DateTime(2022, 10, 1), "No reconstruye números ni elimina compras con fecha fuera del período libro");
}
using (var t = Tabla()) { t.Columns["carga"]!.ColumnName = "otro"; using var r = t.CreateDataReader(); await Reject<ArgumentException>(() => { CertificadosSufridosDatos.CrearMapeador(r); return Task.CompletedTask; }, "Alias cambiado se rechaza"); }
using (var t = Tabla()) { t.Rows.Clear(); t.Columns.Remove("base"); t.Columns.Add("base", typeof(double)); using var r = t.CreateDataReader(); await Reject<InvalidOperationException>(() => { CertificadosSufridosDatos.CrearMapeador(r); return Task.CompletedTask; }, "Tipo decimal cambiado se rechaza"); }
var consulta = new CertificadosSufridosConsulta { ImpuestoTexto = "Ingresos Brutos", Filas = [
    new() { tipo = "P", tipo_desc = "Percepción", corte_ib_vtas = "N", @base = 100.01m, percepcion = 3.01m, percepcion_lh = 0.20m, cm_nombre = "<b>literal</b>", cm_compte = "0001-00000001" },
    new() { tipo = "P", tipo_desc = "Percepción", corte_ib_vtas = "S", @base = -0.01m, percepcion = -0.01m, percepcion_lh = 0.10m, cm_compte = "0001-00000001" },
    new() { tipo = "B", tipo_desc = "SIRCREB", corte_ib_vtas = "N", @base = 0, percepcion = 1, percepcion_lh = 0 } ] };
f = Filtro(); Check(consulta.Grupos(f).Count == 2, "Agrupación por tipo/tipo_desc sin corte");
f.SepararCierre = true; var grupos = consulta.Grupos(f);
Check(grupos.Count == 3 && grupos[1].Titulo.Contains("posterior al cierre") && !grupos[1].Titulo.Contains("Antes"), "Conserva significado real de corte S, sin invertirlo");
Check(CertificadosSufridosConsulta.Sumar(consulta.Filas, r => r.@base) == 100m && CertificadosSufridosConsulta.Sumar(consulta.Filas, r => CertificadosSufridosConsulta.TotalFila(r, true)) == 4.30m, "Totales exactos y total IB incluye LH una vez");
consulta.Filas.Add(new() { tipo = "R", tipo_desc = "Retención", @base = null, percepcion = 1, percepcion_lh = null });
Check(CertificadosSufridosConsulta.Sumar(consulta.Filas, r => r.@base) == null && CertificadosSufridosConsulta.Sumar(consulta.Filas, r => r.percepcion) == 5m, "NULL invalida sólo el concepto afectado");
var json = JObject.FromObject(CertificadosSufridosPresentacion.Crear(consulta, f));
Check(json["columnas"]!.Count() == 15 && json["filas"]!.Count() == 4 && json["grupos"]!.Count() == 4, "Presentación IB completa por grupos");
Check(json["filas"]![0]!["celdas"]![5]!.ToString() == "<b>literal</b>" && json["filas"]![1]!["celdas"]![3]!.ToString() == "0001-00000001", "Texto literal y duplicados preservados");
Check((bool)json["incompletos"]! && json["totalBase"]!.ToString() == "No disponible", "Nulos informados sin total parcial");
json = JObject.FromObject(CertificadosSufridosPresentacion.Crear(consulta, Filtro("IV")));
Check(json["columnas"]!.Count() == 12 && json["totalLH"]!.Type == JTokenType.Null, "Otros impuestos ocultan LH");
var serialized = JsonConvert.SerializeObject(consulta, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
Check(JsonConvert.DeserializeObject<CertificadosSufridosConsulta>(serialized)!.Filas.Last().@base == null, "JSON preserva nulos requeridos");
await Reject<JsonSerializationException>(() => { JsonConvert.DeserializeObject<CertificadosSufridosConsulta>("{\"Filas\":[{}],\"ImpuestoTexto\":\"IB\"}"); return Task.CompletedTask; }, "Respuesta JSON incompleta rechazada");
var datos = new DatosFake(); var usuarios = DispatchProxy.Create<IApiUsuarioServicio, UsuarioFake>(); var proxy = (UsuarioFake)(object)usuarios;
var servicio = new CertificadosSufridosServicio(datos, usuarios);
ClaimsPrincipal User(string perfiles = "[{\"perfil_id\":\"P1\"}]", bool forced = false) => new(new ClaimsIdentity([
    new Claim("user", "prueba"), new Claim("AdmId", "0001#Sucursal"), new Claim("perfiles", perfiles), new Claim("cambio_clave_obligatorio", forced ? "true" : "false")], "test"));
Check(!servicio.TieneAcceso(new ClaimsPrincipal(), "P1", "C") && !servicio.TieneAcceso(User(), "P2", "C") && proxy.Calls == 0, "Anónimo/perfil ajeno rechazados antes de menú y SQL");
Check(!servicio.TieneAcceso(User("incorrecto"), "P1", "C") && !servicio.TieneAcceso(User(forced: true), "P1", "C"), "Token malformado o cambio de clave denegados");
proxy.Item = "imp_ap"; await Reject<UnauthorizedAccessException>(() => servicio.Consultar(Filtro(), User(), "P1", "C", default), "Permiso de emitidos no concede certificados sufridos");
Check(datos.Calls == 0, "Sin acceso no consulta SQL"); proxy.Item = "imp_ret_per_sufridas";
var opciones = await servicio.Opciones(User(), "P1", "C", default);
Check(opciones.Select(i => i.Id).SequenceEqual(new[] { "IB", "GA" }), "Catálogo usa cont S y admite GA, no agente_ret/per");
await servicio.Consultar(Filtro("GA"), User(), "P1", "C", default); Check(datos.Calls == 1, "Consulta contribuyente distinto de IB/IV");
await Reject<ArgumentException>(() => servicio.Consultar(Filtro("IV"), User(), "P1", "C", default), "Impuesto no contribuyente rechazado");
f = Filtro(); f.AgrupadoPorCuit = true; var agrupada = await servicio.Consultar(f, User(), "P1", "C", default);
Check(agrupada.AgrupadoPorCuit && datos.AgrupadasCalls == 1 && datos.Calls == 1, "Modo agrupado usa exclusivamente su contrato y datos");
Check(datos.Calls == 1, "Impuesto inactivo y modo agrupado no acceden al SP de detalle");
using (var cancel = new CancellationTokenSource()) { cancel.Cancel(); await Reject<OperationCanceledException>(() => servicio.Consultar(Filtro(), User(), "P1", "C", cancel.Token), "Cancelación propagada"); }

var salida = args.FirstOrDefault() ?? Path.Combine(Path.GetTempPath(), "geco-sufridos-qa"); Directory.CreateDirectory(salida);
var pdfDatos = new CertificadosSufridosConsulta { ImpuestoTexto = "Ingresos Brutos" };
for (var i = 0; i < 180; i++) pdfDatos.Filas.Add(new() { origen = "C", imp_id = "IB", tipo = "P", tipo_desc = "Percepción", corte_ib_vtas = i < 100 ? "N" : "S",
    cm_fecha = new(hoy.Year, 2, 1), cm_compte = $"0001-{i:00000000}", tco_desc = "Factura A", cm_nombre = "AGENTE SINTÉTICO DE PRUEBA CON NOMBRE EXTENSO", cm_cuit = "00000000000", @base = 1000.01m,
    ali = 0.03m, percepcion = i == 0 ? -0.01m : 30.01m, ali_lh = 20m, percepcion_lh = 0.20m });
var empresa = new EmpresaGeco { RazonSocial = "EMPRESA SINTÉTICA DE PRUEBA", CUIT = "00000000000", IngresosBrutos = "000-000000-0", Direccion = "DOMICILIO DE PRUEBA", Localidad = "SAN JUAN", Responsabilidad = "Responsable Inscripto", InicioActividades = new(2000, 1, 1) };
f = Filtro(); f.SepararCierre = true;
File.WriteAllBytes(Path.Combine(salida, "sufridos-IB-sintetico.pdf"), CertificadosSufridosReporte.Pdf(pdfDatos, f, empresa));
File.WriteAllBytes(Path.Combine(salida, "sufridos-GA-sintetico.pdf"), CertificadosSufridosReporte.Pdf(new() { ImpuestoTexto = "Ganancias", Filas = pdfDatos.Filas }, Filtro("GA"), empresa));
Check(File.ReadAllBytes(Path.Combine(salida, "sufridos-IB-sintetico.pdf")).Take(5).SequenceEqual("%PDF-"u8.ToArray()), "PDF multipágina sintético generado");
await Reject<ArgumentException>(() => { CertificadosSufridosReporte.Pdf(new(), Filtro(), empresa); return Task.CompletedTask; }, "Sin registros no genera un PDF vacío");
File.WriteAllText(Path.Combine(salida, "consulta-sintetica.json"), JsonConvert.SerializeObject(CertificadosSufridosPresentacion.Crear(pdfDatos, f)));
checks += await SufridosPorCuitChecks.Run(salida, empresa);
Console.WriteLine($"PASS: {checks} comprobaciones; salidas sintéticas en {salida}");

public class DatosFake : ICertificadosSufridosDatos
{
    public int Calls; public int AgrupadasCalls;
    public List<CertificadoSufridoPorCuitFila> Agrupadas { get; set; } = [];
    public bool FallaAgrupada { get; set; }
    public Task<List<SufridoImpuesto>> Impuestos(CancellationToken ct)
    { ct.ThrowIfCancellationRequested(); return Task.FromResult(new List<SufridoImpuesto> { new() { Id = "IB", Contribuyente = "S", Texto = "IB" }, new() { Id = "GA", Contribuyente = "S", Texto = "GA" }, new() { Id = "IV", Contribuyente = "N", Texto = "IV" } }); }
    public Task<List<CertificadoSufridoFila>> Consultar(CertificadosSufridosFiltro filtro, CancellationToken ct)
    { ct.ThrowIfCancellationRequested(); Calls++; return Task.FromResult(new List<CertificadoSufridoFila>()); }
    public Task<List<CertificadoSufridoPorCuitFila>> ConsultarPorCuit(CertificadosSufridosFiltro filtro, CancellationToken ct)
    { ct.ThrowIfCancellationRequested(); AgrupadasCalls++; if (FallaAgrupada) throw new IOException("Falla sintética de lectura agrupada"); return Task.FromResult(Agrupadas); }
}
public class UsuarioFake : DispatchProxy
{
    public string Item = ""; public int Calls;
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod?.Name != "ObtenerMenu") throw new NotSupportedException(); Calls++;
        if ((string)args![0]! != "P1" || (string)args[1]! != "prueba" || (string)args[2]! != "C" || (string)args[3]! != "0001") throw new Exception("Contexto incorrecto");
        return new List<MenuPpalDto> { new() { mnu_item_id = Item } };
    }
}
