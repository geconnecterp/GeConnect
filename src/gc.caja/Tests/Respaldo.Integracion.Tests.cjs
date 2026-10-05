const assert=require('node:assert/strict'),vm=require('node:vm'),fs=require('node:fs'),path=require('node:path');
const source=fs.readFileSync(path.join(__dirname,'../wwwroot/js/app/prodfact.js'),'utf8');
function extraer(nombre){const m=source.match(new RegExp(`^(?:async )?function ${nombre}\\([^]*?^}`, 'm'));assert.ok(m,nombre);return m[0];}
let cantidad=0;
function nuevo(){
 const datos={guardado:[{codigo:'ANTERIOR',cantidad:3}],escrituras:[],peticiones:[],errores:[],mensajes:[],logs:[],response:{ok:true,producto:[{p_id:'P1',cantidad_tot:12,respuesta:0}],omitidos:0}};
 const noop=()=>{};const jq=new Proxy({}, {get:(_,p)=>p==='hasClass'?()=>false:p==='val'?(...args)=>args.length?jq:'antiforgery-prueba':()=>jq});
 const $=()=>jq;$.ajax=async o=>{datos.peticiones.push(o);if(datos.errorHttp)throw datos.errorHttp;if(datos.fallo)throw new Error('sin conexión');return datos.response;};
 const c=vm.createContext({console:{log:noop,info:(...a)=>datos.logs.push(a),warn:noop,error:(...a)=>datos.logs.push(a)},$,queueMicrotask,setTimeout:noop,clearTimeout:noop,document:{getElementById:()=>null},window:{crypto:{randomUUID:()=> '113030a4-6ae4-4f6c-a4f0-d68a778f6140'}},
  RecuperarBackupUrl:'/gccaja/Facturacion/ProductoFact/RecuperarBackup',productosFactura:[],totalFactura:0,clienteActualFactura:null,modoBloqueoGrilla:null,origenCargaActual:'directo',ultimoCambioProducto:null,
  respaldoSuspendido:false,respaldoProgramado:false,recuperandoDetalle:false,tieneBackupPendiente:false,
  respaldoLocal:{guardar:async p=>{datos.guardado=p.map(x=>({codigo:x.codigo??x.p_id,cantidad:x.cantidad??x.cantidad_tot}));datos.escrituras.push(datos.guardado);},leer:async()=>({productos:datos.guardado})},
  estadoRespaldo:e=>{if(e)datos.errores.push(e);},actualizarEstadoBotonUltimoDetalle:noop,
  mostrarLoaderCalculando:noop,ocultarLoaderCalculando:noop,mostrarMensajeEstado:m=>datos.mensajes.push(m),
  agregarProductoAGrilla:p=>{c.productosFactura.push(p);vm.runInContext('programarRespaldoLocal()',c);}
 });
 vm.runInContext(['guardarDetalleLocal','programarRespaldoLocal','verificarBackupPendiente','limpiarVentaCompleta','ejecutarRecuperacionBackup'].map(extraer).join('\n'),c);
 return {c,datos,run:s=>vm.runInContext(s,c)};
}
async function prueba(nombre,f){await f();cantidad++;console.log('OK '+nombre);}
(async()=>{
 await prueba('74 filas de una carga masiva producen un solo respaldo completo',async()=>{const t=nuevo();for(let i=0;i<74;i++){t.c.productosFactura.push({p_id:'P'+i,cantidad_tot:i+1});t.run('programarRespaldoLocal()');}await new Promise(r=>setImmediate(r));assert.equal(t.datos.escrituras.length,1);assert.equal(t.datos.guardado.length,74);});
 await prueba('Eliminar el último producto vacía el respaldo',async()=>{const t=nuevo();await t.run('guardarDetalleLocal()');assert.equal(t.datos.guardado.length,0);});
 await prueba('Pago y nueva identificación conservan el último detalle',async()=>{const t=nuevo();t.run('limpiarVentaCompleta(false)');await new Promise(r=>setImmediate(r));assert.equal(t.datos.guardado[0].codigo,'ANTERIOR');assert.equal(t.datos.escrituras.length,0);});
 await prueba('Diferimiento limpia el respaldo cuando corresponde',async()=>{const t=nuevo();t.run('limpiarVentaCompleta(true)');await new Promise(r=>setImmediate(r));assert.equal(t.datos.guardado.length,0);});
 await prueba('Recuperar envía sólo códigos y cantidades, con antiforgery',async()=>{const t=nuevo();await t.run('ejecutarRecuperacionBackup()');assert.deepEqual(JSON.parse(t.datos.peticiones[0].data),{productos:[{codigo:'ANTERIOR',cantidad:3}]});assert.equal(t.datos.peticiones[0].headers.RequestVerificationToken,'antiforgery-prueba');assert.equal(t.datos.guardado[0].codigo,'P1');assert.equal(t.datos.guardado[0].cantidad,12);assert.equal(t.c.recuperandoDetalle,false);});
 await prueba('La vista genera la URL de recuperación con Url.Action',async()=>{const vista=fs.readFileSync(path.join(__dirname,'../Areas/Facturacion/Views/Inicio/Index.cshtml'),'utf8');assert.match(vista,/var RecuperarBackupUrl = .*Url.Action\("RecuperarBackup", "ProductoFact", new \{ area = "Facturacion" \}\)/);});
 await prueba('Recuperación respeta raíz y subaplicación de IIS',async()=>{for(const prefijo of ['', '/gccaja']){const t=nuevo();t.c.RecuperarBackupUrl=prefijo+'/Facturacion/ProductoFact/RecuperarBackup';await t.run('ejecutarRecuperacionBackup()');assert.equal(t.datos.peticiones[0].url,t.c.RecuperarBackupUrl);assert.equal(t.datos.peticiones[0].dataType,'json');}});
 await prueba('Sin URL configurada conserva respaldo y no consulta otra aplicación',async()=>{const t=nuevo();delete t.c.RecuperarBackupUrl;await t.run('ejecutarRecuperacionBackup()');assert.equal(t.datos.peticiones.length,0);assert.equal(t.datos.guardado[0].codigo,'ANTERIOR');assert.match(t.datos.mensajes[0],/dirección.*recuperar/);assert.equal(t.c.recuperandoDetalle,false);});
 await prueba('Error HTTP informa estado y conserva respaldo',async()=>{const t=nuevo();t.datos.errorHttp={status:404};await t.run('ejecutarRecuperacionBackup()');assert.match(t.datos.mensajes[0],/HTTP 404/);assert.equal(t.datos.guardado[0].codigo,'ANTERIOR');assert.equal(t.datos.escrituras.length,0);});
 await prueba('Logs identifican etapa y HTTP sin registrar respaldo ni antiforgery',async()=>{const t=nuevo();t.datos.errorHttp={status:400,getResponseHeader:n=>n==='Content-Type'?'text/html':'113030a4-6ae4-4f6c-a4f0-d68a778f6140'};await t.run('ejecutarRecuperacionBackup()');const registro=t.datos.logs.find(x=>x[0]==='[UltimoDetalle] Error')[1];assert.equal(registro.etapa,'consultar-servidor');assert.equal(registro.http,400);assert.equal(registro.id,t.datos.peticiones[0].headers['X-Respaldo-Id']);assert.ok(!JSON.stringify(t.datos.logs).includes('ANTERIOR'));assert.ok(!JSON.stringify(t.datos.logs).includes('antiforgery-prueba'));});
 await prueba('Falla al revalidar conserva respaldo y grilla vacía',async()=>{const t=nuevo();t.datos.fallo=true;await t.run('ejecutarRecuperacionBackup()');assert.equal(t.datos.guardado[0].codigo,'ANTERIOR');assert.equal(t.c.productosFactura.length,0);assert.equal(t.datos.escrituras.length,0);});
 await prueba('Grilla ocupada y doble clic no inician otra recuperación',async()=>{const t=nuevo();t.c.productosFactura.push({p_id:'EXISTE',cantidad_tot:1});await t.run('ejecutarRecuperacionBackup()');t.c.productosFactura=[];t.c.recuperandoDetalle=true;await t.run('ejecutarRecuperacionBackup()');assert.equal(t.datos.peticiones.length,0);});
 await prueba('Rechazo de todos los productos conserva el original',async()=>{const t=nuevo();t.datos.response={ok:true,producto:[],omitidos:1};await t.run('ejecutarRecuperacionBackup()');assert.equal(t.datos.guardado[0].codigo,'ANTERIOR');});
 assert.match(source,/function actualizarGrillaProductos\(\) \{\s+programarRespaldoLocal\(\)/);
 console.log(cantidad+' escenarios de integración correctos.');
})().catch(e=>{console.error(e);process.exitCode=1;});
