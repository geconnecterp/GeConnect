// Prueba de la máquina de estados del módulo, sin navegador ni servidor.
const fs = require('node:fs'), vm = require('node:vm'), assert = require('node:assert/strict');
const path = require('node:path');
class Element {
    constructor() { this.disabled = false; this.hidden = false; this.dataset = {}; this.style = {}; this.events = {}; this.children = []; }
    addEventListener(name, cb) { this.events[name] = cb; }
    fire(name) { this.events[name]?.({ preventDefault() {} }); }
    setAttribute(k,v) { this[k] = v; }
    getBoundingClientRect() { return { top: 160, height: 24 }; }
    replaceChildren() { this.panel = null; }
    set innerHTML(value) { this.panel = value === 'invalid' ? null : { dataset: { count: value } }; }
    querySelector() { return this.panel; }
    focus() { this.focused = true; }
    reportValidity() { return true; }
    appendChild() {} remove() {} click() { downloaded++; }
}
class Data {
    constructor(form) { this.values = new Map(form?.values || []); }
    append(k,v) { this.values.set(k,v); }
    get(k) { return this.values.get(k); }
    forEach(fn) { this.values.forEach(fn); }
}
const ids = ['iva-resumen','ivr-filtros','ivr-resultado','divDetalle','btnFiltro','btnDetalle','ivr-imprimir','ivr-excel','ivr-mensaje','ivr-cargando','ivr-espera'];
const els = Object.fromEntries(ids.map(id => [id,new Element()]));
const fieldset = new Element(), form = els['ivr-filtros'];
form.querySelector = () => fieldset;
form.values = [['Periodo','2025'],['Mes','11'],['__RequestVerificationToken','test']];
els['iva-resumen'].dataset = { consulta:'/gcsitio/Libros/IvaVentasResumen/Consultar', excel:'/gcsitio/Libros/IvaVentasResumen/ExportarExcel', perfil:'P1', menu:'M' };
let resolveFetch, requests = [], downloaded = 0, print;
const ctx = {
    document: { getElementById:id=>els[id], querySelector:()=>null, createElement:()=>new Element(), body:new Element() },
    window: { innerHeight:900, addEventListener(){} }, FormData:Data,
    requestAnimationFrame:cb=>cb(), setTimeout:cb=>{cb();},
    URL:{createObjectURL:()=> 'blob:test',revokeObjectURL(){}},
    fetch:(url,opt)=>{requests.push({url,opt}); return new Promise(resolve=>{resolveFetch=resolve;});},
    ReporteResetCeldaEnArre:n=>{ctx.arrRepoParams[n-1]=undefined;},
    cargarReporteEnArre:(n,p)=>{ctx.arrRepoParams[n-1]={reporte:n,parametros:p};},
    arrRepoParams:new Array(300), invocacionGestorDoc:x=>{print=x;}, console
};
const planilla103 = { reporte:103, parametros:{ modulo:'PLANILLA_ELABORACION_Y_FRACCION' } };
ctx.arrRepoParams[102] = planilla103;
vm.runInNewContext(fs.readFileSync(path.join(__dirname,'../gc.sitio/wwwroot/js/app/libros/iva-ventas-resumen.js'),'utf8'),ctx);
const tick=()=>new Promise(resolve=>setImmediate(resolve));
async function response(count,status=200,excel=false) {
    resolveFetch({ok:status===200,status,redirected:false,headers:{get:()=>excel?'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet':'text/html'},text:async()=>String(count),json:async()=>({mensaje:'Error controlado'}),blob:async()=>({})});
    await tick();
}
(async()=>{
    assert.equal(form.hidden,false); assert.equal(els['btnDetalle'].disabled,true); assert.equal(els['ivr-excel'].disabled,true);
    form.fire('submit'); assert.equal(els['ivr-cargando'].hidden,false); assert.equal(fieldset.disabled,true); assert.equal(form.hidden,true);
    form.fire('submit'); assert.equal(requests.length,1); // sin duplicar consulta
    await response(8);
    assert.equal(els['ivr-cargando'].hidden,true); assert.equal(els['ivr-excel'].disabled,false); assert.equal(form.hidden,true);
    assert.equal(ctx.arrRepoParams[103].parametros.Mes,'11');
    assert.equal(ctx.arrRepoParams[103].reporte,104);
    assert.strictEqual(ctx.arrRepoParams[102],planilla103);
    els['btnFiltro'].fire('click'); assert.equal(form.hidden,false); assert.equal(els['divDetalle'].hidden,true);
    els['btnFiltro'].fire('click'); assert.equal(form.hidden,true); assert.equal(els['ivr-excel'].disabled,false);
    els['ivr-imprimir'].fire('click'); assert.equal(print.moduloGestor,'IVA_VENTAS_RESUMEN'); assert.equal(print.reporte,104);
    assert.deepEqual(Array.from(print.reportesPreseleccionados),[104]);
    els['ivr-excel'].fire('click'); assert.equal(requests.at(-1).opt.body.get('Periodo'),'2025'); assert.equal(els['ivr-cargando'].hidden,false);
    await response(8,200,true); assert.equal(downloaded,1);
    els['btnDetalle'].fire('click'); assert.equal(form.hidden,false); assert.equal(els['divDetalle'].hidden,true); assert.equal(form.values[0][1],'2025'); assert.equal(els['ivr-excel'].disabled,true);
    assert.equal(ctx.arrRepoParams[103],undefined); assert.strictEqual(ctx.arrRepoParams[102],planilla103);
    form.fire('submit'); await response(0); assert.equal(els['ivr-imprimir'].disabled,true); assert.equal(els['btnDetalle'].disabled,false);
    els['btnFiltro'].fire('click'); form.fire('change'); assert.equal(ctx.arrRepoParams[103],undefined); assert.equal(els['btnDetalle'].disabled,true);
    form.fire('submit'); await response(8); els['btnFiltro'].fire('click'); form.fire('submit'); await response(0,503);
    assert.equal(form.hidden,false); assert.equal(els['ivr-resultado'].panel,null); assert.equal(els['ivr-excel'].disabled,true); assert.equal(els['ivr-mensaje'].textContent,'Error controlado');
    form.fire('reset'); assert.equal(els['btnDetalle'].disabled,true); assert.equal(fieldset.disabled,false);
    assert.equal(ctx.arrRepoParams[103],undefined); assert.strictEqual(ctx.arrRepoParams[102],planilla103);
    console.log('PASS: paneles, spinner, doble envío, filtros conservados, impresión, Excel, vacío, error y reset. Prueba sintética; no sustituye navegador.');
})().catch(e=>{console.error(e);process.exitCode=1;});
