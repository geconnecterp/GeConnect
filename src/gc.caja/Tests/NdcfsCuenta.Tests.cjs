const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),assert=require('node:assert/strict');
const source=fs.readFileSync(path.join(__dirname,'../wwwroot/js/app/ndNcFs.js'),'utf8');
const extract=n=>{const m=source.match(new RegExp(`^    function ${n}[(][^]*?^    }`,'m'));assert(m,n);return m[0];};
function setup(){
 const requests=[],messages=[],props=new Map();let removed=0,hydrated=null;
 const $=selector=>({length:1,data:key=>selector[key],prop(k,v){props.set(selector+'.'+k,v);return this;},
 val(){return this;},hide(){return this;},show(){return this;},toggle(){return this;},addClass(){return this;},empty(){return this;},remove(){removed++;return this;}});
 $.ajax=options=>{const callbacks={};requests.push({options,callbacks});const req={done(fn){callbacks.done=fn;return req;},fail(fn){callbacks.fail=fn;return req;},always(fn){callbacks.always=fn;return req;}};return req;};
 const ctx=vm.createContext({$,window:{ndcfsBuscarCuentaPorIdUrl:'/gc/Facturacion/Cliente/BuscarClientePorId'},busquedaEnCurso:false,cuentaSeleccionada:null,
 SELECTORES:{btnSeguir:'seguir',btnBuscar:'buscar',btnCancelar:'cancelar',inputBusqueda:'criterio'},
 mostrarMensaje(...a){messages.push(a);},mostrarLoaderNdcfs(){},ocultarLoaderNdcfs(){},logError(){},
 hidratarDatosCuenta(c){hydrated=c;},buscarCuenta(){assert.fail('No debe repetir búsqueda general');}});
 vm.runInContext(['obtenerRechazoCuenta','seleccionarCuentaDesdeFila','procesarCuentaSeleccionada','limpiarSeleccionVisual','bloquearBusqueda'].map(extract).join('\n'),ctx);
 return {ctx,requests,messages,props,get removed(){return removed;},get hydrated(){return hydrated;},
 select(id,origen){ctx.seleccionarCuentaDesdeFila($({'cta-id':id,'cta-origen':origen,'cta-documento':'20300111222'}));},
 respond(cliente,ok=true){const c=requests.at(-1).callbacks;c.done({ok,cliente,mensaje:'Rechazo SP'});c.always();}};
}
let count=0;function test(n,fn){fn();count++;console.log('OK '+n);}
test('Consumidor final y cuentas sin ID no redibujan ni consultan',()=>{
 for(const [id,o] of [['','F'],['C1','F'],['','C'],['   ','P'],['C1','N'],['P1','Q'],['C1','X']]){const t=setup();t.select(id,o);assert.equal(t.requests.length,0);assert.equal(t.removed,0);assert.equal(t.messages.length,1);assert.equal(t.ctx.cuentaSeleccionada,null);}
});
test('Cliente/proveedor seleccionan la fila exacta con su origen y un solo envío',()=>{
 for(const o of ['C','P']){const t=setup();t.select('ID01',o);t.select('ID02',o);assert.equal(t.requests.length,1);const req=t.requests[0].options;assert.equal(req.data.clienteId,'ID01');assert.equal(req.data.origen,o);assert.equal(req.data.documento,'20300111222');assert.match(req.url,/BuscarClientePorId$/);assert.equal(t.removed,0);t.respond({id:'ID01',origen:o});assert.equal(t.removed,1);assert.equal(t.hydrated.id,'ID01');assert.equal(t.props.get('seguir.disabled'),false);assert.equal(t.ctx.busquedaEnCurso,false);}
});
test('Respuesta equivocada o rechazada conserva resultados y no habilita seguir',()=>{
 for(const cliente of [{id:'OTRA',origen:'C'},{id:'ID01',origen:'P'},{id:'',origen:'C'},null]){const t=setup();t.select('ID01','C');t.respond(cliente);assert.equal(t.removed,0);assert.equal(t.ctx.cuentaSeleccionada,null);assert.equal(t.props.get('seguir.disabled'),true);assert.equal(t.ctx.busquedaEnCurso,false);assert.equal(t.messages.length,1);}
 const t=setup();t.select('ID01','C');t.respond({id:'ID01',origen:'C'},false);assert.equal(t.removed,0);assert.equal(t.ctx.cuentaSeleccionada,null);
});
test('Falla de red conserva grilla y permite reintentar',()=>{const t=setup();t.select('ID01','C');const c=t.requests[0].callbacks;c.fail({status:500});c.always();assert.equal(t.removed,0);assert.equal(t.props.get('cancelar.disabled'),false);t.select('ID01','C');assert.equal(t.requests.length,2);});
test('Resultado único inválido limpia una selección anterior',()=>{for(const cuenta of [{id:'','origen':'C'},{id:'C1',origen:'F'},{id:'P1',origen:'Q'}]){const t=setup();t.ctx.cuentaSeleccionada={id:'PREVIA',origen:'C'};t.ctx.procesarCuentaSeleccionada(cuenta);assert.equal(t.ctx.cuentaSeleccionada,null);assert.equal(t.props.get('seguir.disabled'),true);assert.equal(t.removed,0);}});
test('Resultado único registrado habilita seguir',()=>{const t=setup();t.ctx.procesarCuentaSeleccionada({id:'C001',origen:'C'});assert.equal(t.ctx.cuentaSeleccionada.id,'C001');assert.equal(t.props.get('seguir.disabled'),false);});
console.log(`PASS: ${count} grupos de pruebas de selección ND/NC/FS.`);
