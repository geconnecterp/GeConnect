using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Servicios.Libros;
using gc.api.infra.Datos.Implementacion;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.Dtos.Users;
using gc.sitio.core.Servicios.Implementacion.Libros;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Reflection;
using System.Security.Claims;

// Sólo contratos y datos sintéticos. No abre SQL ni emite documentos.
var checks = 0;
void Check(bool valor, string texto) { if (!valor) throw new Exception("FAIL: " + texto); checks++; Console.WriteLine("OK: " + texto); }
async Task Reject<T>(Func<Task> action, string text) where T:Exception
{ try { await action(); } catch(T) { Check(true,text); return; } throw new Exception("FAIL: " + text); }
var hoy=IvaVentasFiltro.HoyArgentina();
CertificadosPercepcionFiltro Filtro(string imp="IB") => new() {Impuesto=imp,Periodo=hoy.Year,Mes=2};
foreach(var imp in new[]{"IB","IV"}) { Filtro(imp).Validar(); Check(true,"Filtro soporta " + imp); }
foreach(var imp in new[]{"GA","LH","","ib","' OR 1=1"}) await Reject<ArgumentException>(()=>{Filtro(imp).Validar();return Task.CompletedTask;},"Impuesto ajeno rechazado");
var inicial=CertificadosPercepcionFiltro.Inicial();
Check(inicial.Periodo==hoy.Year && inicial.Mes==hoy.Month && !inicial.Fechas && !inicial.PorCuit,"Mes/año Argentina y opcionales apagados");
var f=Filtro(); f.Periodo=hoy.Year-5; f.Validar(); Check(true,"Se admite el sexto año");
f.Periodo--; await Reject<ArgumentException>(()=>{f.Validar();return Task.CompletedTask;},"Séptimo año rechazado");
f=Filtro(); f.Periodo=hoy.Year+1; await Reject<ArgumentException>(()=>{f.Validar();return Task.CompletedTask;},"Año futuro rechazado");
foreach(var mes in new[]{0,13}) { f=Filtro(); f.Mes=mes; await Reject<ArgumentException>(()=>{f.Validar();return Task.CompletedTask;},"Mes inválido rechazado"); }
f=Filtro(); f.Fechas=true; f.Desde=new(hoy.Year,2,2); f.Hasta=new(hoy.Year,2,1);
await Reject<ArgumentException>(()=>{f.Validar();return Task.CompletedTask;},"Rango invertido rechazado");
f.Desde=new(hoy.Year,1,31); await Reject<ArgumentException>(()=>{f.Validar();return Task.CompletedTask;},"Rango fuera del mes rechazado");
f.Desde=null; await Reject<ArgumentException>(()=>{f.Validar();return Task.CompletedTask;},"Rango activo incompleto rechazado");
f.Desde=new(hoy.Year,2,1); f.Validar(); Check(true,"Rango dentro del mes válido");
f=Filtro(); f.PorCuit=true;
foreach(var cuit in new[]{""," ",new string('1',16)}) { f.Cuit=cuit; await Reject<ArgumentException>(()=>{f.Validar();return Task.CompletedTask;},"CUIT activo vacío/extenso rechazado"); }
f.Cuit="%ABC%"; f.Validar(); Check(true,"CUIT patrón sin validación fiscal");
f.PorCuit=false; f.Cuit="obsoleto"; f.Desde=DateTime.MinValue; f.Hasta=null;
var pars=CertificadosPercepcionDatos.Parametros(f);
Check(pars.Length==8 && pars.Select(p=>p.ParameterName).SequenceEqual(new[]{"@imp_id","@periodo","@mes","@f","@fecha_d","@fecha_h","@cuit","@cuit_like"}),"Ocho parámetros exactos, sin parámetros de paginación");
Check(pars[0].Size==2 && pars[1].Size==4 && pars[2].Size==2 && pars[7].Size==15 && pars[3].SqlDbType==SqlDbType.Bit && pars[4].SqlDbType==SqlDbType.DateTime,"Tipos y tamaños SQL");
Check((string)pars[2].Value=="02" && (string)pars[7].Value=="%" && (DateTime)pars[4].Value==new DateTime(hoy.Year,2,1),"Opcionales apagados ignoran datos obsoletos");
f.PorCuit=true; f.Cuit="%' OR 1=1 --"; Check((string)CertificadosPercepcionDatos.Parametros(f)[7].Value==f.Cuit,"CUIT se parametriza, no se concatena como SQL");
Check(CertificadosPercepcionDatos.SpConsulta=="dbo.SPGECO_Imp_Certificdos_AP","Nombre real Certificdos_AP sin corregir ortografía");

DataTable Tabla()
{
    var t=new DataTable();
    foreach(var p in typeof(CertificadoPercepcionFila).GetProperties()) t.Columns.Add(p.Name,Nullable.GetUnderlyingType(p.PropertyType)??p.PropertyType);
    var row=t.NewRow(); row["tco"]="001"; row["compte"]="0001-00000001"; row["base"]=9999999999999.99m; row["percepcion"]=-2.01m;
    row["fecha"]=new DateTime(2022,10,1); row["razon_social"]="<b>literal</b>"; t.Rows.Add(row); return t;
}
using(var t=Tabla()) using(var r=t.CreateDataReader())
{
    var mapear=CertificadosPercepcionDatos.CrearMapeador(r); r.Read(); var row=mapear(r);
    Check(row.@base==9999999999999.99m && row.percepcion==-2.01m && row.percepcion_lh==null,"Mapeo preserva decimal y LH nulo");
    Check(row.tco=="001" && row.compte=="0001-00000001" && row.tco_id_ori==null,"Ceros y referencia original nula intactos");
    Check(row.fecha==new DateTime(2022,10,1),"No se filtra por fecha al materializar el período libro");
}
using(var t=Tabla()) { t.Columns["domicilio"]!.ColumnName="campo_cambiado"; using var r=t.CreateDataReader(); await Reject<ArgumentException>(()=>{CertificadosPercepcionDatos.CrearMapeador(r);return Task.CompletedTask;},"Cambio de alias rechazado"); }
using(var t=Tabla()) { t.Columns.Add("campo_extra",typeof(string)); using var r=t.CreateDataReader(); await Reject<InvalidOperationException>(()=>{CertificadosPercepcionDatos.CrearMapeador(r);return Task.CompletedTask;},"Columna extra rechazada"); }
using(var t=Tabla()) { t.Rows.Clear(); t.Columns.Remove("base"); t.Columns.Add("base",typeof(double)); using var r=t.CreateDataReader(); await Reject<InvalidOperationException>(()=>{CertificadosPercepcionDatos.CrearMapeador(r);return Task.CompletedTask;},"No convierte double a decimal silenciosamente"); }
var consulta=new CertificadosPercepcionConsulta {Filas=[new(){@base=9999999999999.99m,percepcion=10.01m,percepcion_lh=3},new(){@base=-0.01m,percepcion=-2.01m,percepcion_lh=2}]};
Check(consulta.TotalBase==9999999999999.98m && consulta.TotalPercepcion==8m && consulta.TotalLH==5m,"Totalización exacta con negativos y LH separado");
consulta.Filas.Add(new(){@base=0,percepcion=0,percepcion_lh=null});
Check(consulta.TotalLH==null && consulta.TotalPercepcion==8m && !consulta.ImportesIncompletos,"LH nulo sólo invalida su propio total");
consulta.Filas.Add(new()); Check(consulta.TotalBase==null && consulta.TotalPercepcion==null && consulta.ImportesIncompletos,"NULL no se totaliza como cero");
Check(new CertificadosPercepcionConsulta().TotalBase==0m,"Consulta vacía total cero");
var serialized=JsonConvert.SerializeObject(consulta,new JsonSerializerSettings{NullValueHandling=NullValueHandling.Ignore});
var restored=JsonConvert.DeserializeObject<CertificadosPercepcionConsulta>(serialized)!;
Check(restored.Filas[0].@base==9999999999999.99m && restored.Filas[^1].percepcion==null,"Newtonsoft conserva precisión y NULL");
await Reject<JsonSerializationException>(()=>{JsonConvert.DeserializeObject<CertificadosPercepcionConsulta>("{\"Filas\":[{\"percepcion\":1}]}");return Task.CompletedTask;},"Contrato JSON incompleto rechazado");
var datos=new PercepcionDatosFake(); var usuarios=DispatchProxy.Create<IApiUsuarioServicio,PercepcionUsuarioFake>(); var proxy=(PercepcionUsuarioFake)(object)usuarios;
var servicio=new CertificadosPercepcionServicio(datos,usuarios);
ClaimsPrincipal User(string perfiles="[{\"perfil_id\":\"P1\"}]",bool forced=false)=>new(new ClaimsIdentity([new Claim("user","prueba"),new Claim("AdmId","0001#Sucursal"),new Claim("perfiles",perfiles),new Claim("cambio_clave_obligatorio",forced?"true":"false")],"test"));
Check(!servicio.TieneAcceso(new ClaimsPrincipal(),"P1","C"),"Anónimo denegado");
Check(!servicio.TieneAcceso(User(),"P2","C") && proxy.Calls==0,"Perfil ajeno al token denegado antes del menú");
Check(!servicio.TieneAcceso(User("incorrecto"),"P1","C"),"Perfiles malformados denegados");
Check(!servicio.TieneAcceso(User(forced:true),"P1","C"),"Cambio obligatorio de clave denegado");
proxy.Item="imp_ar"; await Reject<UnauthorizedAccessException>(()=>servicio.Consultar(Filtro(),User(),"P1","C",default),"imp_ar no concede imp_ap");
Check(datos.QueryCalls==0 && datos.OptionsCalls==0,"Sin permiso no accede a SQL");
proxy.Item="imp_ap"; var opciones=await servicio.Opciones(User(),"P1","C",default);
Check(opciones.Select(i=>i.Id).SequenceEqual(new[]{"IB","IV"}),"Catálogo sólo IB/IV con agente_per S, no agente_ret");
foreach(var imp in new[]{"IB","IV"})
{
    var result=await servicio.Consultar(Filtro(imp),User(),"P1","C",default);
    Check(result.Filas.Count==2 && result.Filas[0].compte==result.Filas[1].compte,"No deduplica comprobantes repetidos " + imp);
    Check(result.Filas[0].fecha==new DateTime(2022,10,1),"No excluye fecha fuera del mes libro " + imp);
    var json=JObject.FromObject(CertificadosPercepcionPresentacion.Crear(result,Filtro(imp)));
    Check(json["columnas"]!.Count()==(imp=="IB"?16:14) && json["filas"]![0]!.Count()==(imp=="IB"?16:14),"Columnas específicas AP " + imp);
    Check(!json["columnas"]!.Any(c=>c["titulo"]!.ToString().Contains("Certificado")||c["titulo"]!.ToString().Contains("OP")),"Sin columnas heredadas Certificado/OP " + imp);
    Check(json["filas"]![0]![3]!.ToString()=="001" && json["filas"]![0]![7]!.ToString()=="0001-00000001","Tipo/número sin reconstruir " + imp);
    Check(json["totalLH"]!.Type==(imp=="IB"?JTokenType.String:JTokenType.Null),"LH sólo visible en IB " + imp);
    Check(json["contexto"]!.ToString().Contains("Libro IVA") && json["filas"]![0]![2]!.ToString()=="<b>literal</b>","Contexto libro y texto literal " + imp);
}
datos.Activo=false; await Reject<ArgumentException>(()=>servicio.Consultar(Filtro(),User(),"P1","C",default),"Impuesto deshabilitado no consulta"); Check(datos.QueryCalls==2,"Sin agente_per no invoca SP AP");
datos.Activo=true; datos.Falla=true; await Reject<IOException>(()=>servicio.Consultar(Filtro(),User(),"P1","C",default),"Error de lectura se propaga sin resultado parcial");
using(var cancel=new CancellationTokenSource()) { cancel.Cancel(); await Reject<OperationCanceledException>(()=>servicio.Consultar(Filtro(),User(),"P1","C",cancel.Token),"Cancelación propagada"); }
Check(IvaVentasCliente.Url("https://api.example/gcservicios","api/certificados-percepcion-emitidos/consultar").AbsoluteUri=="https://api.example/gcservicios/api/certificados-percepcion-emitidos/consultar","Ruta conserva subaplicación IIS");
checks += await PercepcionDdjjChecks.Run(args);
datos.Falla=false;
var pedidoDdjj=new CertificadosPercepcionDdjjPedido{Impuesto="IB",Periodo=hoy.Year,Mes=10};
using(var salida=new MemoryStream())
{
    proxy.Item="imp_ar";
    await Reject<UnauthorizedAccessException>(()=>servicio.GenerarDdjj(pedidoDdjj,User(),"P1","C",salida,default),"DDJJ exige imp_ap; imp_ar no concede descarga");
    Check(datos.DdjjCalls==0 && salida.Length==0,"DDJJ sin permiso no invoca SP ni genera archivo");
    proxy.Item="imp_ap"; datos.Activo=false;
    await Reject<ArgumentException>(()=>servicio.GenerarDdjj(pedidoDdjj,User(),"P1","C",salida,default),"DDJJ revalida agente_per");
    Check(datos.DdjjCalls==0,"Sin agente_per no ejecuta SP DDJJ");
    datos.Activo=true; await servicio.GenerarDdjj(pedidoDdjj,User(),"P1","C",salida,default);
    Check(datos.DdjjCalls==1 && salida.Length>0,"DDJJ autorizada genera Excel mensual");
}
Console.WriteLine($"PASS: {checks} verificaciones de Percepciones; sólo datos sintéticos, sin SQL ni escrituras de negocio.");

public class PercepcionDatosFake:ICertificadosPercepcionDatos
{
    public bool Activo=true,Falla; public int QueryCalls,OptionsCalls,DdjjCalls;
    public async IAsyncEnumerable<object?[]> Ddjj(CertificadosPercepcionDdjjPedido pedido,[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested(); DdjjCalls++; await Task.Yield();
        yield return PercepcionDdjjChecks.Fila(pedido.Impuesto);
    }
    public Task<List<PercepcionImpuesto>> Impuestos(CancellationToken ct)
    { ct.ThrowIfCancellationRequested(); OptionsCalls++; return Task.FromResult(new List<PercepcionImpuesto>{new(){Id="IB",AgentePer=Activo?"S":"N"},new(){Id="IV",AgentePer="S"},new(){Id="GA",AgentePer="S"},new(){Id="LH",AgentePer="S"},new(){Id="OT",AgentePer="N"}}); }
    public Task<List<CertificadoPercepcionFila>> Consultar(CertificadosPercepcionFiltro filtro,CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested(); QueryCalls++; if(Falla)throw new IOException("Fallo sintético");
        var row=new CertificadoPercepcionFila{tco="001",compte="0001-00000001",@base=1,percepcion=-0.01m,percepcion_lh=null,fecha=new DateTime(2022,10,1),razon_social="<b>literal</b>"};
        return Task.FromResult(new List<CertificadoPercepcionFila>{row,row});
    }
}
public class PercepcionUsuarioFake:DispatchProxy
{
    public string Item=""; public int Calls;
    protected override object? Invoke(MethodInfo? method,object?[]? args)
    { if(method?.Name!="ObtenerMenu")throw new NotSupportedException(); Calls++; return new List<MenuPpalDto>{new(){mnu_item_id=Item}}; }
}
