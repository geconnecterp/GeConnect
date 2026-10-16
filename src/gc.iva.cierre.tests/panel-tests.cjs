// Estados operativos con red simulada: evita ejecutar cierres reales en estas pruebas.
const fs = require('node:fs'), vm = require('node:vm'), assert = require('node:assert/strict'), path = require('node:path');
class Element {
    constructor() { this.disabled = false; this.hidden = false; this.dataset = {}; this.style = {}; this.events = {}; this.attrs = {}; this.classList = { add:()=>{}, toggle:()=>{} }; }
    addEventListener(n,fn) { this.events[n]=fn; }
    fire(n,arg={}) { this.events[n]?.({preventDefault(){},...arg}); }
    setAttribute(k,v) { this.attrs[k]=v; }
    getBoundingClientRect() { return {top:180,height:24}; }
    replaceChildren() { this.panel=null; this.list=[]; }
    querySelector(selector) { return selector==='.ivc-panel'?this.panel:this.button; }
    querySelectorAll() { return this.list||[]; }
    contains() { return true; }
    closest(selector) { if(selector==='tr[data-clave]' || selector==='tr') return this.row||this; if(selector==='.ivc-select') return this.row?this:null; }
    focus() {} appendChild() {} remove() {} click() { downloads++; }
    set innerHTML(value) {
        this.markup=value;
        if(this!==els['ivc-resultado']) return;
        const state=JSON.parse(value);
        this.panel={dataset:{proximo:state.next,puedeCerrar:String(state.allow)}};
        this.list=state.rows.map(p=>{ const row=new Element(); row.dataset={clave:'2:'+p,periodo:p}; row.button=new Element(); row.button.row=row; return row; });
    }
    get innerHTML() { return this.markup; }
}
class Data { constructor(){this.values=new Map([['__RequestVerificationToken','test']]);} append(k,v){this.values.set(k,v);} get(k){return this.values.get(k);} }
const ids=['iva-cierre','ivc-resultado','ivc-contenido','ivc-mensaje','ivc-actualizar','ivc-cerrar','ivc-rg','ivc-confirmacion','ivc-confirmar','ivc-cancelar','ivc-esperando','ivc-espera-texto','ivc-token','ivc-seleccion','ivc-periodo-confirmar'];
const els=Object.fromEntries(ids.map(id=>[id,new Element()]));
els['iva-cierre'].dataset={disponible:'true',estado:'/gcsitio/Libros/IvaVentasCierre/Estado',cerrar:'/gcsitio/Libros/IvaVentasCierre/Cerrar',descarga:'/gcsitio/Libros/IvaVentasCierre/Rg3685'};
const pending=[],calls=[]; let downloads=0;
const modal={show(){els['ivc-confirmacion'].fire('shown.bs.modal');},hide(){els['ivc-confirmacion'].fire('hidden.bs.modal');}};
const ctx={document:{getElementById:id=>els[id],querySelector:()=>null,createElement:()=>new Element(),body:new Element()},window:{innerHeight:900,addEventListener(){}},
    FormData:Data,requestAnimationFrame:fn=>fn(),setTimeout:fn=>fn(),URL:{createObjectURL:()=> 'blob:test',revokeObjectURL(){}},
    bootstrap:{Modal:{getInstance:()=>modal,getOrCreateInstance:()=>modal}},
    fetch:(url,opt)=>{calls.push({url,opt});return new Promise((resolve,reject)=>pending.push({resolve,reject}));},console};
vm.runInNewContext(fs.readFileSync(path.join(__dirname,'../gc.sitio/wwwroot/js/app/libros/iva-ventas-cierre.js'),'utf8'),ctx);
const tick=()=>new Promise(r=>setImmediate(r));
async function reply(body,type='text/html',status=200) { const p=pending.shift(); assert.ok(p,'Hay una sola solicitud pendiente esperada'); p.resolve({ok:status===200,status,redirected:false,headers:{get:()=>type},text:async()=>JSON.stringify(body),json:async()=>body,blob:async()=>({})}); await tick(); }
const state=(next='202511',allow=true,rows=['202510','202509'])=>({next,allow,rows});
const selectFirst=()=>els['ivc-resultado'].fire('click',{target:els['ivc-resultado'].list[0].button});
(async()=>{
    assert.equal(calls.length,1); assert.equal(els['ivc-esperando'].hidden,false); assert.equal(els['ivc-cerrar'].disabled,true);
    await reply(state()); assert.equal(els['ivc-cerrar'].disabled,false); assert.equal(els['ivc-rg'].disabled,true);
    selectFirst(); assert.equal(els['ivc-rg'].disabled,false); assert.equal(els['ivc-resultado'].list[0].button.attrs['aria-pressed'],'true');
    els['ivc-cerrar'].fire('click'); assert.equal(els['ivc-periodo-confirmar'].textContent,'2025-11'); assert.equal(els['ivc-rg'].disabled,true);
    modal.hide(); assert.equal(calls.length,1); assert.equal(els['ivc-rg'].disabled,false); // cancelar no escribe
    els['ivc-cerrar'].fire('click'); els['ivc-confirmar'].fire('click'); els['ivc-confirmar'].fire('click');
    assert.equal(calls.length,2); assert.equal(calls.at(-1).opt.body.get('Periodo'),'202511'); // no cierra el seleccionado 202510
    await reply({resultado:0,resultado_msj:'OK'},'application/json');
    assert.equal(calls.at(-1).url.endsWith('/Estado'),true);
    await reply(state('202512',true,['202511','202510']));
    assert.equal(els['ivc-cerrar'].innerHTML.includes('2025-12'),true); assert.equal(els['ivc-rg'].disabled,false);
    assert.equal(els['ivc-mensaje'].className,'alert alert-success');
    els['ivc-rg'].fire('click'); assert.equal(calls.at(-1).opt.body.get('Periodo'),'202511'); await reply({},'application/zip'); assert.equal(downloads,1);
    els['ivc-cerrar'].fire('click'); els['ivc-confirmar'].fire('click');
    await reply({mensaje:'El período cambió.'},'application/json',409);
    assert.equal(els['ivc-cerrar'].disabled,true); assert.equal(els['ivc-rg'].disabled,true); assert.equal(pending.length,0); // sin reintento
    els['ivc-actualizar'].fire('click'); await reply(state('202601',false));
    assert.equal(els['ivc-cerrar'].disabled,true); selectFirst(); assert.equal(els['ivc-rg'].disabled,false); // sucursal consulta/exporta
    els['ivc-actualizar'].fire('click'); await reply(state('202601',true,[])); assert.equal(els['ivc-rg'].disabled,true); assert.equal(els['ivc-cerrar'].disabled,false);
    els['ivc-actualizar'].fire('click'); await reply({mensaje:'No se pudo consultar.'},'application/json',503);
    assert.equal(els['ivc-resultado'].panel,null); assert.equal(els['ivc-cerrar'].disabled,true);
    els['ivc-actualizar'].fire('click'); await reply(state()); els['ivc-cerrar'].fire('click'); els['ivc-confirmar'].fire('click');
    pending.shift().reject(new TypeError('network')); await tick();
    assert.equal(pending.length,0); assert.equal(els['ivc-cerrar'].disabled,true); assert.equal(els['ivc-mensaje'].textContent.includes('comprobar el estado'),true);
    console.log('PASS: selección, próximo período independiente, cancelar, doble confirmación, refresco tras cierre, ZIP, conflicto, sucursal, vacío, error y desconexión sin reintentos.');
})().catch(e=>{console.error(e);process.exitCode=1;});
