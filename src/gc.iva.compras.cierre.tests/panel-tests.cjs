// Red y DOM simulados. Nunca alcanza a la API ni ejecuta un cierre real.
const fs = require('node:fs'), vm = require('node:vm'), assert = require('node:assert/strict'), path = require('node:path');
class Element {
    constructor() { this.disabled=false; this.hidden=false; this.dataset={}; this.style={}; this.events={}; this.attrs={}; this.classList={add(){},toggle(){}}; }
    addEventListener(n,fn){this.events[n]=fn;}
    fire(n,arg={}){this.events[n]?.({preventDefault(){},...arg});}
    setAttribute(k,v){this.attrs[k]=v;}
    getBoundingClientRect(){return {top:180,height:24};}
    replaceChildren(){this.panel=null;this.list=[];}
    querySelector(s){return s==='.icc-panel'?this.panel:this.button;}
    querySelectorAll(){return this.list||[];}
    contains(){return true;}
    closest(s){if(s==='tr[data-clave]'||s==='tr')return this.row||this;if(s==='.icc-select')return this.row?this:null;}
    focus(){} appendChild(){} remove(){} click(){downloads++;names.push(this.download);}
    set innerHTML(value){this.markup=value;if(this!==els['icc-resultado'])return;const s=JSON.parse(value);this.panel={dataset:{proximo:s.next,puedeCerrar:String(s.allow)}};this.list=s.rows.map(item=>{const p=typeof item==='string'?item:item.period,t=typeof item==='string'?'2':item.type;const r=new Element();r.dataset={clave:t+':'+p,periodo:p,tipo:t};r.button=new Element();r.button.row=r;return r;});}
    get innerHTML(){return this.markup;}
}
class Data {constructor(){this.values=new Map([['__RequestVerificationToken','test']]);}append(k,v){this.values.set(k,v);}get(k){return this.values.get(k);}}
const ids=['iva-compras-cierre','icc-resultado','icc-contenido','icc-mensaje','icc-actualizar','icc-cerrar','icc-rg','icc-salir','icc-confirmacion','icc-confirmar','icc-cancelar','icc-esperando','icc-espera-texto','icc-token','icc-seleccion','icc-periodo-confirmar'];
const els=Object.fromEntries(ids.map(id=>[id,new Element()]));
els['iva-compras-cierre'].dataset={disponible:'true',estado:'/gcsitio/Libros/IvaComprasCierre/Estado',cerrar:'/gcsitio/Libros/IvaComprasCierre/Cerrar',descarga:'/gcsitio/Libros/IvaComprasCierre/Rg3685'};
const pending=[],calls=[],names=[];let downloads=0;
const modal={show(){els['icc-confirmacion'].fire('shown.bs.modal');},hide(){els['icc-confirmacion'].fire('hidden.bs.modal');}};
const ctx={document:{getElementById:id=>els[id],querySelector:()=>null,createElement:()=>new Element(),body:new Element()},window:{innerHeight:900,addEventListener(){}},FormData:Data,requestAnimationFrame:fn=>fn(),setTimeout:fn=>fn(),URL:{createObjectURL:()=> 'blob:test',revokeObjectURL(){}},bootstrap:{Modal:{getInstance:()=>modal,getOrCreateInstance:()=>modal}},fetch:(url,opt)=>{calls.push({url,opt});return new Promise((resolve,reject)=>pending.push({resolve,reject}));},console};
vm.runInNewContext(fs.readFileSync(path.join(__dirname,'../gc.sitio/wwwroot/js/app/libros/iva-compras-cierre.js'),'utf8'),ctx);
const tick=()=>new Promise(r=>setImmediate(r));
async function reply(body,type='text/html',status=200){const p=pending.shift();assert.ok(p);p.resolve({ok:status===200,status,redirected:false,headers:{get:()=>type},text:async()=>JSON.stringify(body),json:async()=>body,blob:async()=>({})});await tick();}
const state=(next='202511',allow=true,rows=['202510','202509'])=>({next,allow,rows});
const select=(i=0)=>els['icc-resultado'].fire('click',{target:els['icc-resultado'].list[i].button});
(async()=>{
    assert.equal(calls.length,1);assert.equal(els['icc-esperando'].hidden,false);assert.equal(els['icc-cerrar'].disabled,true);
    await reply(state());assert.equal(els['icc-cerrar'].disabled,false);assert.equal(els['icc-rg'].disabled,true);
    els['icc-rg'].fire('click');assert.equal(calls.length,1);
    select();assert.equal(els['icc-rg'].disabled,false);assert.equal(els['icc-resultado'].list[0].button.attrs['aria-pressed'],'true');
    els['icc-cerrar'].fire('click');assert.equal(els['icc-periodo-confirmar'].textContent,'2025-11');assert.equal(els['icc-rg'].disabled,true);assert.equal(els['icc-salir'].attrs['aria-disabled'],'true');
    modal.hide();assert.equal(calls.length,1);assert.equal(els['icc-rg'].disabled,false);
    els['icc-cerrar'].fire('click');els['icc-confirmar'].fire('click');els['icc-confirmar'].fire('click');
    assert.equal(calls.length,2);assert.equal(calls.at(-1).opt.body.get('Periodo'),'202511');
    await reply({resultado:0,resultado_msj:'OK'},'application/json');assert.equal(calls.at(-1).url.endsWith('/Estado'),true);
    await reply(state('202512',true,['202511','202510']));assert.equal(els['icc-cerrar'].innerHTML.includes('2025-12'),true);assert.equal(els['icc-rg'].disabled,false);assert.equal(els['icc-mensaje'].className,'alert alert-success');
    els['icc-rg'].fire('click');assert.equal(calls.at(-1).opt.body.get('Periodo'),'202511');assert.equal(calls.at(-1).opt.body.get('TipoRegistro'),'2');assert.equal(calls.at(-1).opt.body.get('__RequestVerificationToken'),'test');
    const n=calls.length;els['icc-rg'].fire('click');els['icc-cerrar'].fire('click');els['icc-actualizar'].fire('click');assert.equal(calls.length,n);assert.equal(els['icc-esperando'].hidden,false);
    await reply({},'application/zip');assert.equal(downloads,1);assert.equal(names[0],'IVA_Compras_3685_2025-11.zip');
    els['icc-rg'].fire('click');await reply({mensaje:'Registro nulo en D.'},'application/json',422);assert.equal(downloads,1);assert.equal(els['icc-mensaje'].textContent,'Registro nulo en D.');
    els['icc-rg'].fire('click');await reply({},'text/html');assert.equal(downloads,1);assert.equal(els['icc-mensaje'].className,'alert alert-warning');
    els['icc-cerrar'].fire('click');els['icc-confirmar'].fire('click');await reply({mensaje:'El período cambió.'},'application/json',409);assert.equal(els['icc-cerrar'].disabled,true);assert.equal(els['icc-rg'].disabled,true);assert.equal(pending.length,0);
    els['icc-actualizar'].fire('click');await reply(state('202601',false));assert.equal(els['icc-cerrar'].disabled,true);select();assert.equal(els['icc-rg'].disabled,false);
    els['icc-actualizar'].fire('click');await reply(state('202601',true,[]));assert.equal(els['icc-rg'].disabled,true);assert.equal(els['icc-cerrar'].disabled,false);
    els['icc-actualizar'].fire('click');await reply({mensaje:'No se pudo consultar.'},'application/json',503);assert.equal(els['icc-resultado'].panel,null);assert.equal(els['icc-cerrar'].disabled,true);
    els['icc-actualizar'].fire('click');await reply(state('202511',true,[{period:'202510',type:'1'},{period:'202510',type:'2'}]));select(0);els['icc-rg'].fire('click');assert.equal(calls.at(-1).opt.body.get('TipoRegistro'),'1');await reply({},'application/zip');
    select(1);assert.equal(els['icc-seleccion'].textContent.includes('tipo 2'),true);
    els['icc-cerrar'].fire('click');els['icc-confirmar'].fire('click');pending.shift().reject(new TypeError('network'));await tick();assert.equal(pending.length,0);assert.equal(els['icc-cerrar'].disabled,true);assert.equal(els['icc-mensaje'].textContent.includes('comprobar el estado'),true);
    els['icc-actualizar'].fire('click');await reply(state());els['icc-cerrar'].fire('click');els['icc-confirmar'].fire('click');await reply({},'application/json');assert.equal(els['icc-cerrar'].disabled,true);assert.equal(pending.length,0);
    console.log('PASS: selección compuesta, próximo período independiente, cancelar, doble confirmación, bloqueo simultáneo, ZIP, antiforgery, PathBase, MIME, errores, sucursal, vacío, desconexión y respuesta inválida sin reintentos.');
})().catch(e=>{console.error(e);process.exitCode=1;});
