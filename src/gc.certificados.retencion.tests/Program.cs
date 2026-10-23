using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Servicios.Libros;
using gc.api.infra.Datos.Implementacion;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using gc.sitio.core.Servicios.Implementacion.Libros;
using gc.sitio.Models.Components;
using Newtonsoft.Json;
using System.Data;
using System.Reflection;
using System.Security.Claims;

// Sólo datos sintéticos; no se abre SQL ni se emiten, anulan o cierran documentos.
var passed = 0;
void Check(bool value, string name) { if (!value) throw new Exception("FAIL: " + name); passed++; Console.WriteLine("OK: " + name); }
async Task Reject<T>(Func<Task> action, string name) where T : Exception
{ try { await action(); } catch (T) { Check(true, name); return; } throw new Exception("FAIL: " + name); }
var hoy = IvaVentasFiltro.HoyArgentina();
CertificadosRetencionFiltro Filtro(string imp = "IB") => new() { Impuesto = imp, Periodo = hoy.Year, Mes = 2 };
foreach (var imp in new[] { "IB", "GA", "IV" }) { Filtro(imp).Validar(); Check(true, "Impuesto soportado " + imp); }
foreach (var imp in new[] { "", "LH", "ib", "' OR 1=1" }) await Reject<ArgumentException>(() => { Filtro(imp).Validar(); return Task.CompletedTask; }, "Impuesto no soportado rechazado");
var inicial = CertificadosRetencionFiltro.Inicial();
Check(inicial.Periodo == hoy.Year && inicial.Mes == hoy.Month && !inicial.Fechas && !inicial.PorCuit, "Inicial actual Argentina sin filtros opcionales");
var f = Filtro(); f.Periodo = hoy.Year - 5; f.Validar(); Check(true, "Sexto año incluido");
f.Periodo--; await Reject<ArgumentException>(() => { f.Validar(); return Task.CompletedTask; }, "Séptimo año rechazado");
f = Filtro(); f.Mes = 13; await Reject<ArgumentException>(() => { f.Validar(); return Task.CompletedTask; }, "Mes inválido rechazado");
f = Filtro(); f.Fechas = true; f.Desde = new(hoy.Year, 2, 2); f.Hasta = new(hoy.Year, 2, 1);
await Reject<ArgumentException>(() => { f.Validar(); return Task.CompletedTask; }, "Fechas invertidas rechazadas");
f.Desde = new(hoy.Year, 1, 31); await Reject<ArgumentException>(() => { f.Validar(); return Task.CompletedTask; }, "Fecha fuera del período rechazada");
f.Desde = new(hoy.Year, 2, 1); f.Validar(); Check(true, "Rango dentro del período válido");
f = Filtro(); f.PorCuit = true; f.Cuit = "%ABC%"; f.Validar(); Check(true, "CUIT patrón admitido sin validación fiscal");
f.Cuit = new string('1', 16); await Reject<ArgumentException>(() => { f.Validar(); return Task.CompletedTask; }, "CUIT mayor a varchar15 rechazado");
f.PorCuit = false; f.Fechas = false; f.Desde = DateTime.MinValue; f.Hasta = null;
var parametros = CertificadosRetencionDatos.Parametros(f);
Check(parametros.Length == 8 && parametros.Select(p => p.ParameterName).SequenceEqual(new[] { "@imp_id", "@periodo", "@mes", "@f", "@fecha_d", "@fecha_h", "@cuit", "@cuit_like" }), "Ocho parámetros SQL exactos");
Check(parametros[0].Size == 2 && parametros[1].Size == 4 && parametros[2].Size == 2 && parametros[7].Size == 15, "Tamaños varchar del SP");
Check(parametros[3].SqlDbType == SqlDbType.Bit && parametros[4].SqlDbType == SqlDbType.DateTime && (string)parametros[2].Value == "02", "Tipos de fecha/bit y mes con cero");
Check((string)parametros[7].Value == "%" && (DateTime)parametros[4].Value == new DateTime(hoy.Year,2,1), "Opcionales desactivados ignoran valores obsoletos");
Check(CertificadosRetencionDatos.SpConsulta == "dbo.SPGECO_Imp_Certificdos_AR", "Se conserva el nombre real Certificdos");
f = Filtro(); f.PorCuit = true; f.Cuit = "%' OR 1=1 --";
Check((string)CertificadosRetencionDatos.Parametros(f)[7].Value == f.Cuit, "Patrón transmitido como parámetro, no SQL concatenado");

var resultado = new CertificadosRetencionConsulta { Filas = [new() { @base = 9999999999999.99m, reten = 10.01m, cib_reten_lh = 3, estado = "A" }, new() { @base = -0.01m, reten = -2.01m, cib_reten_lh = 2, estado = "?" }] };
Check(resultado.TotalBase == 9999999999999.98m && resultado.TotalRetencion == 8m, "Totales decimal con signos y sin excluir estados");
Check(resultado.TotalLH == 5m && resultado.TotalRetencion != resultado.TotalLH + 8m, "LH independiente, no sumado a reten");
Check(CertificadosRetencionConsulta.Importe(resultado.TotalBase) == "9.999.999.999.999,98", "Importes grandes completos N2 es-AR");
resultado.Filas.Add(new()); Check(resultado.TotalBase == null && resultado.TotalRetencion == null && resultado.ImportesIncompletos, "NULL invalida el total afectado, no se suma como cero");
Check(new CertificadosRetencionConsulta().TotalBase == 0m, "Consulta vacía total cero sin inventar registros");
var texto = JsonConvert.SerializeObject(resultado, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
var recovered = JsonConvert.DeserializeObject<CertificadosRetencionConsulta>(texto)!;
Check(recovered.Filas[0].@base == 9999999999999.99m && recovered.Filas[2].reten == null, "Newtonsoft conserva precisión y NULL incluidos explícitamente");
await Reject<JsonSerializationException>(() => { JsonConvert.DeserializeObject<CertificadosRetencionConsulta>("{\"Filas\":[{\"reten\":1}]}"); return Task.CompletedTask; }, "Contrato incompleto no materializa ceros por defecto");
var datos = new DatosFake(); var usuarios = DispatchProxy.Create<IApiUsuarioServicio, UsuarioFake>(); var proxy = (UsuarioFake)(object)usuarios;
var servicio = new CertificadosRetencionServicio(datos, usuarios);
ClaimsPrincipal User(string perfiles = "[{\"perfil_id\":\"P1\"}]", bool forced = false) => new(new ClaimsIdentity(new[] { new Claim("user","operador"), new Claim("AdmId","0001#Sucursal"), new Claim("perfiles",perfiles), new Claim("cambio_clave_obligatorio",forced ? "true" : "false") }, "test"));
Check(!servicio.TieneAcceso(new ClaimsPrincipal(), "P1", "M"), "Anónimo denegado");
Check(!servicio.TieneAcceso(User(), "P2", "M") && proxy.Calls == 0, "Perfil ajeno al token denegado antes de menú");
Check(!servicio.TieneAcceso(User("invalido"), "P1", "M"), "JSON de perfiles malformado denegado");
Check(!servicio.TieneAcceso(User(forced:true), "P1", "M"), "Cambio de clave obligatorio denegado");
await Reject<UnauthorizedAccessException>(() => servicio.Consultar(Filtro(), User(), "P1", "M", default), "Sin imp_ar no consulta certificados");
Check(datos.QueryCalls == 0 && datos.OptionsCalls == 0, "Denegación no llega a SQL");
proxy.Allow = true;
var opciones = await servicio.Opciones(User(), "P1", "M", default);
Check(opciones.Count == 3 && opciones.All(i=>i.AgenteRet == "S"), "Lista filtra agente_ret S");
foreach (var imp in new[] { "IB", "GA", "IV" }) { var r = await servicio.Consultar(Filtro(imp), User(), "P1", "M", default); Check(r.Filas.Count == 2 && datos.Last!.Impuesto == imp, "Consulta completa por impuesto " + imp); }
datos.ImpuestosActivos = false;
await Reject<ArgumentException>(() => servicio.Consultar(Filtro(), User(), "P1", "M", default), "Impuesto no agente rechazado aun manipulando la petición");
Check(datos.QueryCalls == 3, "Sin agente no ejecuta el SP principal");
datos.ImpuestosActivos = true; datos.Falla = true;
await Reject<IOException>(() => servicio.Consultar(Filtro(), User(), "P1", "M", default), "Fallo tardío se propaga sin entregar resultados parciales");
Check(IvaVentasCliente.Url("https://api.example/gcservicios", "api/certificados-retencion-emitidos/consultar").AbsoluteUri == "https://api.example/gcservicios/api/certificados-retencion-emitidos/consultar", "Preserva PathBase IIS");
foreach (var paginas in new[] { 1,2,5,7,8,307,int.MaxValue }) foreach (var pagina in new[] { 1, Math.Max(1,paginas/2), paginas })
{
    var numeros = new PaginadorNumerado(pagina,paginas).Numeros().OfType<int>().ToArray();
    Check(numeros.Length <= 7 && numeros.First() == 1 && numeros.Last() == paginas && numeros.Contains(pagina), "Ventana acotada del paginador");
}
await DdjjTests.Run(Check, Reject<CertificadosRetencionDdjjException>);
Console.WriteLine($"PASS: {passed} verificaciones sintéticas; sin conexiones SQL ni escrituras en la base.");

public class DatosFake : ICertificadosRetencionDatos
{
    public int OptionsCalls, QueryCalls; public bool ImpuestosActivos = true, Falla; public CertificadosRetencionFiltro? Last;
    public Task<List<RetencionImpuesto>> Impuestos(CancellationToken ct) { OptionsCalls++; return Task.FromResult(new List<RetencionImpuesto> { new() { Id="IB",AgenteRet=ImpuestosActivos?"S":"N" }, new() { Id="GA",AgenteRet="S" }, new() { Id="IV",AgenteRet="S" }, new() { Id="OT",AgenteRet="N" } }); }
    public Task<List<CertificadoRetencionFila>> Consultar(CertificadosRetencionFiltro filtro, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested(); QueryCalls++; Last=filtro;
        if (Falla) throw new IOException("Error sintético.");
        return Task.FromResult(new List<CertificadoRetencionFila> { new() { certificado="0002",@base=100,reten=2,cib_reten_lh=1,estado="A" }, new() { certificado="0001",@base=-50,reten=-1,cib_reten_lh=0,estado="S" } });
    }
    public async IAsyncEnumerable<object?[]> Ddjj(CertificadosRetencionDdjjPedido pedido, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested(); QueryCalls++; await Task.CompletedTask;
        if (Falla) throw new IOException("Error sintético DDJJ.");
        yield return pedido.Impuesto == "GA" ? ["06 00001  "] : pedido.Impuesto == "IB"
            ? ["Prueba", "00123", "Domicilio", "00999", "00001", new DateTime(pedido.Periodo, pedido.Mes, 1), 9999999999999.99m, 3.5m, -2.01m]
            : ["00001", "00123", "Prueba", "Domicilio", new DateTime(pedido.Periodo, pedido.Mes, 1), 9999999999999.99m, -2.01m, "A", "0001"];
    }
}
public class UsuarioFake : DispatchProxy
{
    public bool Allow; public int Calls;
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    { if(method?.Name!="ObtenerMenu")throw new NotSupportedException(); Calls++; return Allow?new List<MenuPpalDto>{new(){mnu_item_id="imp_ar"}}:new List<MenuPpalDto>(); }
}
