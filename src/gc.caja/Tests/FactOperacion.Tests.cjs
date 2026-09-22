const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),assert=require('node:assert/strict');
const factSource=fs.readFileSync(path.join(__dirname,'../wwwroot/js/app/fact.js'),'utf8');
const source=fs.readFileSync(path.join(__dirname,'../wwwroot/js/app/factOperacion.js'),'utf8');
function setup(){
 let prefilter,cleaned=0,drafts=0,overlays=0;const requests=[],modal={inert:false};
 function $(selector){return {each(fn){if(selector==='.modal')fn.call(modal);return this;},append(){overlays++;return this;},trigger(){return this;},remove(){overlays--;return this;}};}
 $.ajaxPrefilter=fn=>prefilter=fn;
 function xhr(){const always=[],done=[],fail=[];return {status:200,always(fn){always.push(fn);return this;},done(fn){done.push(fn);return this;},fail(fn){fail.push(fn);return this;},abort(){this.finish(0);},finish(status=200,value={ok:true}){this.status=status;this.responseJSON=value;always.forEach(fn=>fn());(status>=200&&status<300?done:fail).forEach(fn=>fn(status>=200&&status<300?value:this));}};}
 $.ajax=options=>{const x=xhr();requests.push({options,x});return x;};
 const window={jQuery:$,location:{href:'https://localhost:7257/Facturacion/Inicio',origin:'https://localhost:7257'},NuevaOperacionFacturaUrl:'/Facturacion/Inicio/NuevaOperacion',limpiarVentaCompleta:del=>{assert.equal(del,false);cleaned++;},descartarBorradorPago:()=>drafts++};
 vm.runInNewContext(source,{window,document:{},URL,setTimeout,clearTimeout,console});
 return {window,requests,modal,start:()=>window.prepararNuevaOperacionFactura(),track(url='/Facturacion/ProductoFact/CalcularFilas'){const x=xhr();prefilter({url},{},x);return x;},get cleaned(){return cleaned;},get drafts(){return drafts;},get overlays(){return overlays;}};
}
const tick=()=>new Promise(resolve=>setTimeout(resolve,5));
let count=0;async function test(name,fn){await fn();count++;console.log('OK '+name);}
(async()=>{
 await test('Inicio espera respuesta del servidor y conserva backup al limpiar',async()=>{const t=setup();assert(!t.window.operacionFacturaPreparada());const p=t.start();await tick();assert.equal(t.cleaned,0);assert(t.modal.inert);t.requests[0].x.finish();await p;assert.equal(t.cleaned,1);assert.equal(t.drafts,1);assert(!t.modal.inert);assert(t.window.operacionFacturaPreparada());assert.equal(t.overlays,0);});
 await test('Dos inicios simultáneos comparten una sola petición',async()=>{const t=setup();const p=t.start(),q=t.start();assert.equal(p,q);await tick();assert.equal(t.requests.length,1);t.requests[0].x.finish();await p;assert.equal(t.cleaned,1);});
 await test('Cálculo demorado termina antes del reinicio',async()=>{const t=setup();const old=t.track();const p=t.start();await tick();assert.equal(t.requests.length,0);old.finish();await tick();assert.equal(t.requests.length,1);t.requests[0].x.finish();await p;});
 await test('Callback que encadena otra petición también termina antes del reinicio',async()=>{const t=setup();let next;const old=t.track();old.done(()=>next=t.track('/Facturacion/Cliente/BuscarCliente'));const p=t.start();old.finish();await tick();assert.equal(t.requests.length,0);next.finish();await tick();t.requests[0].x.finish();await p;});
 await test('Autorización pendiente impide reiniciar y conserva venta',async()=>{const t=setup();t.window.cambioListaPrecioEnCurso=()=>true;await assert.rejects(t.start(),/autorización/);assert.equal(t.requests.length,0);assert.equal(t.cleaned,0);});
 await test('Rechazo del reinicio no limpia pantalla y permite reintentar',async()=>{const t=setup();const p=t.start();await tick();t.requests[0].x.finish(409,{mensaje:'Caja no disponible'});await assert.rejects(p,/Caja no disponible/);assert.equal(t.cleaned,0);assert(!t.modal.inert);const q=t.start();await tick();t.requests[1].x.finish();await q;assert.equal(t.cleaned,1);});
 await test('Respuesta perdida de cálculo previo no permite borrar su estado',async()=>{const t=setup();t.track().finish(0);await assert.rejects(t.start(),/respuesta completa/);assert.equal(t.requests.length,0);assert.equal(t.cleaned,0);});
 await test('Nueva petición de productos durante el reinicio no se envía',async()=>{const t=setup();const p=t.start();await tick();const late=t.track();assert.equal(late.status,0);t.requests[0].x.finish();await p;});
 for(const [name,nueva,preparada,expected] of [['Reabrir dentro de la venta conserva estado',false,true,0],['Inicio explícito pide reiniciar',true,true,1],['Primer acceso requiere reinicio',false,false,1]]) {
 await test(name,async()=>{let resets=0,clears=0;const start=factSource.indexOf('async function abrirModalIdentificarCliente(');const end=/^}\r?$/m.exec(factSource.slice(start));
 const ctx={window:{operacionFacturaPreparada:()=>preparada,prepararNuevaOperacionFactura:async()=>resets++},limpiarModalCliente:()=>clears++,$:()=>({modal(){},trigger(){}}),setTimeout:fn=>fn()};vm.createContext(ctx);vm.runInContext(factSource.slice(start,start+end.index+1),ctx);await ctx.abrirModalIdentificarCliente(nueva);assert.equal(resets,expected);assert.equal(clears,1);});
 }
 await test('Búsqueda de cliente cancelada no impide iniciar otra venta',async()=>{const t=setup();t.track('/Facturacion/Cliente/BuscarCliente').finish(0);const p=t.start();await tick();t.requests[0].x.finish();await p;assert.equal(t.cleaned,1);});
 console.log(count+' escenarios de reinicio aprobados, sin servidor.');
})().catch(e=>{console.error(e);process.exitCode=1;});
