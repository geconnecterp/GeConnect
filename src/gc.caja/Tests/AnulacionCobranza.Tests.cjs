const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),assert=require('node:assert/strict');
const source=fs.readFileSync(path.join(__dirname,'../wwwroot/js/app/anulacionCobranza.js'),'utf8');
function extract(s,name){const start=s.search(new RegExp('^function '+name+'\\(','m'));assert(start>=0,name);const end=/^}\r?$/m.exec(s.slice(start));return s.slice(start,start+end.index+1);}
let requests,dialogs,routes,elements,events;
function $(id){if(!elements.has(id))elements.set(id,{props:{},classes:new Set()});const el=elements.get(id);return {
 prop(k,v){if(v===undefined)return el.props[k];el.props[k]=v;return this;},attr(k,v){el.props[k]=v;return this;},
 toggleClass(k,on){if(on)el.classes.add(k);else el.classes.delete(k);return this;},trigger(){return this;},
 find(s){return $(id+' '+s);},html(v){el.html=v;return this;},text(v){el.text=v;return this;},modal(){return this;},
 one(k,fn){events.set(k,fn);return this;}
};}
$.ajax=o=>requests.push(o);
const ctx={$ ,console:{log(){},warn(){},error(){}},window:{location:{replace:u=>routes.push(u)}},
 anulacionCobranzaAnularUrl:'/Anular',anulacionCobranzaMenuUrl:'/',
 AbrirMensaje:(...args)=>dialogs.push(args),setTimeout:fn=>fn()};
vm.createContext(ctx);
const names=['confirmarAnulacionCobranza','ejecutarAnulacionCobranza','bloquearVistaAnulaCob','mostrarProgresoAnulaCob','volverAlMenuAnulaCob','informarResultadoInciertoAnulaCob','actualizarSeleccionAnulaCob','construirMensajeAnulacionExitosa','mostrarMensajeAnulaCob','normalizarMontoAnulaCob','formatearMontoAnulaCob','escaparHtmlAnulaCob','obtenerPrimerValorAnulaCob','buscarCobranzasAnulaCob','seleccionarCobranzaAnulaCob'];
vm.runInContext(names.map(n=>extract(source,n)).join('\n'),ctx);
function setup(){requests=[];dialogs=[];routes=[];elements=new Map();events=new Map();ctx.anulandoAnulaCob=false;ctx.saliendoAnulaCob=false;ctx.cobranzaSeleccionadaAnulaCob={rb_compte:'RC<123>',co_cobranza:100000,caja_nro_operacion:8,cta_id:'TEST',caja_nro_proceso:'P',caja_nro_cierre:1};}
let count=0;function test(name,fn){setup();fn();count++;console.log('OK '+name);}
test('Confirmación identifica recibo e importe, escapa HTML y usa interrogación correcta',()=>{
 ctx.confirmarAnulacionCobranza();const d=dialogs[0];assert(d[0].includes('anulación'));assert(d[1].includes('¿Confirmás'));assert(d[1].includes('RC&lt;123&gt;'));assert(d[1].includes('card-golden'));assert(d[1].includes('100,000.00'));assert.equal(requests.length,0);
 d[2]('NO');assert.equal(requests.length,0);
});
test('Doble confirmación y llamada directa producen un solo envío',()=>{
 ctx.confirmarAnulacionCobranza();dialogs[0][2]('SI');dialogs[0][2]('SI');ctx.ejecutarAnulacionCobranza();
 assert.equal(requests.length,1);assert(ctx.anulandoAnulaCob);assert($('#modalAnulacionCobranza, #modalIdentificarCliente').prop('inert'));assert($('#modalAnulacionCobranza button, input').prop('disabled'));
 assert(elements.get('#bloqueoAnulaCob').classes.has('d-flex'));
});
test('No se puede buscar ni cambiar selección durante el envío',()=>{
 ctx.ejecutarAnulacionCobranza();const selected=ctx.cobranzaSeleccionadaAnulaCob;ctx.buscarCobranzasAnulaCob();ctx.seleccionarCobranzaAnulaCob(null);ctx.actualizarSeleccionAnulaCob();assert.equal(requests.length,1);assert.equal(ctx.cobranzaSeleccionadaAnulaCob,selected);assert($('#btnAnularCobranza').prop('disabled'));
});
test('Éxito mantiene bloqueo después de complete y vuelve al menú una sola vez',()=>{
 ctx.ejecutarAnulacionCobranza();requests[0].success({ok:true,recibo:'RC1',importe:100000});requests[0].complete();
 assert(ctx.anulandoAnulaCob);assert($('#btnAnularCobranza').prop('disabled'));assert.equal(dialogs[0][5],'succ!');assert(!elements.get('#bloqueoAnulaCob').classes.has('d-flex'));
 ctx.ejecutarAnulacionCobranza();assert.equal(requests.length,1);dialogs[0][2]();events.get('hidden.bs.modal.salidaAnulaCob')();assert.deepEqual(routes,['/']);
});
test('Cerrar el aviso exitoso con X también vuelve al menú',()=>{
 ctx.ejecutarAnulacionCobranza();requests[0].success({ok:true});events.get('hidden.bs.modal.salidaAnulaCob')();assert.deepEqual(routes,['/']);
});
test('Rechazo explícito desbloquea sin navegar',()=>{
 ctx.ejecutarAnulacionCobranza();requests[0].success({ok:false,mensaje:'Rechazado'});requests[0].complete();assert(!ctx.anulandoAnulaCob);assert.equal($('#modalAnulacionCobranza, #modalIdentificarCliente').prop('inert'),false);assert.equal($('#btnAnularCobranza').prop('disabled'),false);assert.equal(routes.length,0);
});
test('Timeout conserva bloqueo y pide verificar antes de repetir',()=>{
 ctx.ejecutarAnulacionCobranza();requests[0].error({},'timeout','timeout');requests[0].complete();ctx.ejecutarAnulacionCobranza();assert.equal(requests.length,1);assert(ctx.anulandoAnulaCob);assert(dialogs[0][1].includes('consultá las cobranzas'));dialogs[0][2]();assert.deepEqual(routes,['/']);
});
test('Respuesta vacía tampoco habilita reenvío',()=>{
 ctx.ejecutarAnulacionCobranza();requests[0].success(null);requests[0].complete();assert(ctx.anulandoAnulaCob);ctx.ejecutarAnulacionCobranza();assert.equal(requests.length,1);
});
console.log(`${count} escenarios de anulación aprobados. Sin anular cobranzas reales.`);
