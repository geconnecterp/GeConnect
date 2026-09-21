const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),assert=require('node:assert/strict');
const source=fs.readFileSync(path.join(__dirname,'../wwwroot/js/app/factListaPrecioAutorizado.js'),'utf8');
function extract(name){const start=source.search(new RegExp('^    (?:async )?function '+name+'\\(','m'));assert(start>=0,name);const end=/^    }\r?$/m.exec(source.slice(start));return source.slice(start,start+end.index+5);}
let count=0;
function setup(){let callback,unblock=0,errors=[],sent=[];const ctx={S:{modal:'#lp'},catalogo:[{lp_id:'02'},{lp_id:'03'}],seleccion:{id:'02',descripcion:'Lista 2'},solicitudEnCurso:false,$:()=>({modal(){}}),listaActivaId:()=> '01',escaparHtml:String,mostrarError:x=>errors.push(x),mostrarExito(){},bloquearInterfaz(){},restaurarBotonesModal:()=>unblock++,urlGlobal:(n,f)=>f,AbrirMensaje:(t,m,cb)=>callback=cb,window:{},ajaxJson(url,data){return new Promise((resolve,reject)=>sent.push({url,data,resolve,reject}));}};vm.createContext(ctx);vm.runInContext(['confirmarCambio','solicitarAutorizacion','seleccionar'].map(extract).join('\n'),ctx);return {ctx,reply:r=>callback(r),sent,errors,get unblock(){return unblock;}};}
async function test(name,fn){await fn();count++;console.log('OK '+name);}
const tick=()=>new Promise(setImmediate);
(async()=>{
 for(const answer of ['NO',undefined])await test('Cancelar/X o respuesta indefinida no solicitan autorización: '+answer,()=>{const t=setup();t.ctx.confirmarCambio();t.reply(answer);assert.equal(t.sent.length,0);assert.equal(t.ctx.seleccion.id,'02');});
 await test('Aceptar y doble clic generan una sola solicitud',async()=>{const t=setup();t.ctx.confirmarCambio();t.reply('SI');t.reply('SI');assert.equal(t.sent.length,1);assert.equal(t.sent[0].data.lpId,'02');t.sent[0].reject(new Error('Sin conexión'));await tick();assert.equal(t.unblock,1);assert(!t.ctx.solicitudEnCurso);});
 await test('No cambia la lista seleccionada durante la solicitud',async()=>{const t=setup();const p=t.ctx.solicitarAutorizacion();t.ctx.seleccionar('03');assert.equal(t.ctx.seleccion.id,'02');t.sent[0].reject(new Error('Sin conexión'));await p;});
 await test('Misma lista y selección vacía no generan solicitudes',()=>{const t=setup();t.ctx.seleccion.id='01';t.ctx.confirmarCambio();t.ctx.seleccion=null;t.ctx.confirmarCambio();assert.equal(t.sent.length,0);});
 await test('Error 409 conserva el mensaje del servidor y libera bloqueo',async()=>{const t=setup();const p=t.ctx.solicitarAutorizacion();t.sent[0].reject(new Error('La lista de precios sólo puede cambiarse antes de cargar productos.'));await p;assert(t.errors[0].includes('antes de cargar productos'));assert(!t.ctx.solicitudEnCurso);assert.equal(t.unblock,1);});
 await test('Aprobación aplica sólo la lista devuelta por el servidor',async()=>{const t=setup();let applied;
 t.ctx.aplicarEnPantalla=lista=>applied=lista;t.ctx.finalizarNoAprobada=()=>{};
 t.ctx.window.AutorizacionRemota={estados:{APROBADA:'APROBADA'},esperar:async options=>{await options.onAprobada({idSolicitud:'ID-1'});return {tipo:'APROBADA'};}};
 const p=t.ctx.solicitarAutorizacion();t.sent[0].resolve({ok:true,idSolicitud:'ID-1'});await tick();assert.equal(t.sent.length,2);assert.equal(t.sent[1].data.idSolicitud,'ID-1');assert.equal(applied,undefined);
 t.sent[1].resolve({ok:true,lista:{id:'02',descripcion:'Autorizada'}});await p;assert.equal(applied.id,'02');assert(!t.ctx.solicitudEnCurso);
 });
 for(const tipo of ['RECHAZADA','EXPIRADA'])await test(tipo+' no aplica ninguna lista',async()=>{const t=setup();t.ctx.finalizarNoAprobada=()=>{};let applied=0;t.ctx.aplicarEnPantalla=()=>applied++;
 t.ctx.window.AutorizacionRemota={estados:{APROBADA:'APROBADA',RECHAZADA:'RECHAZADA',EXPIRADA:'EXPIRADA'},esperar:async()=>({tipo})};
 const p=t.ctx.solicitarAutorizacion();t.sent[0].resolve({ok:true,idSolicitud:'ID-1'});await p;assert.equal(t.sent.length,1);assert.equal(applied,0);assert(!t.ctx.solicitudEnCurso);
 });
 console.log(count+' escenarios LP aprobados, sin autorizaciones reales.');
})().catch(e=>{console.error(e);process.exitCode=1;});
