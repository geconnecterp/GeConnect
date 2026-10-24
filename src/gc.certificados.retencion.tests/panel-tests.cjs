// DOM y red simulados: ejecuta el JS real del módulo; no abre SQL ni el navegador.
const fs=require('node:fs'),vm=require('node:vm'),assert=require('node:assert/strict'),path=require('node:path');
class Element {
    constructor(tag='DIV'){this.tagName=tag.toUpperCase();this.children=[];this.events={};this.attrs={};this.dataset={};this.style={};this.value='';this.defaultValue='';this.hidden=false;this.disabled=false;this.checked=false;this.defaultChecked=false;this.type='';this.textContent='';this.options=[];}
    addEventListener(n,fn){this.events[n]=fn;}
    fire(n,arg={}){return this.events[n]?.({preventDefault(){},...arg});}
    setAttribute(n,v){this.attrs[n]=String(v);}
    appendChild(e){this.children.push(e);return e;}
    replaceChildren(...nodes){this.children=[];for(const n of nodes)this.children.push(...(n.tagName==='FRAGMENT'?n.children:[n]));}
    getBoundingClientRect(){return {top:200,height:30};}
    setCustomValidity(v){this.validity=v;}
    reportValidity(){return !this.validity;}
    querySelectorAll(){return controls;}
    querySelector(){return this.children[0]||null;}
    closest(){return scroll;}
    focus(){this.focused=true;}
    showModal(){this.open=true;}
    close(){this.open=false;this.fire('close');}
    click(){downloads.push(this.download);}
    remove(){}
}
const names=['retenciones-emitidas','ret-filtros','divFiltro','divDetalle','ret-panel','ret-campos','btnFiltro','btnDetalle','ret-mensaje','ret-cargando','ret-fechas','ret-por-fecha','ret-cuit-campos','ret-por-cuit','ret-periodo','ret-mes','ret-desde','ret-hasta','ret-tamanio','ret-filas','ret-columnas','ret-paginador','ret-cantidad','ret-vacio','ret-contexto','ret-base','ret-total','ret-lh-bloque','ret-lh','ret-impuesto','ret-cuit','ret-ddjj','ret-ddjj-dialogo','ret-ddjj-periodo','ret-ddjj-cancelar','ret-ddjj-confirmar','ret-espera-texto'];
const els=Object.fromEntries(names.map(n=>[n,new Element()]));for(const n of names)els[n].id=n;
const controls=[];
function select(id,values,current){const e=els[id];e.tagName='SELECT';e.value=current;e.options=values.map(value=>({value,defaultSelected:value===current}));controls.push(e);}
select('ret-periodo',['2026','2025'],'2026');select('ret-mes',Array.from({length:12},(_,i)=>String(i+1)),'10');select('ret-tamanio',['50','100','250','500'],'100');select('ret-impuesto',['IB','GA','IV'],'IB');
for(const n of ['ret-por-fecha','ret-por-cuit']){els[n].type='checkbox';controls.push(els[n]);}
for(const n of ['ret-desde','ret-hasta','ret-cuit'])controls.push(els[n]);
els['ret-filtros'].reportValidity=()=>true;
els['ret-filtros'].querySelector=()=>({value:'token-sintetico'});
els['retenciones-emitidas'].dataset={disponible:'true',consulta:'/gcsitio/Libros/CertificadosRetencionEmitidos/Consultar',ddjj:'/gcsitio/Libros/CertificadosRetencionEmitidos/Ddjj'};
const calls=[],pending=[],downloads=[],scroll={scrollTop:0};
class FormDataFake{
    constructor(form){this.values=new Map();if(form){assert.equal(els['ret-campos'].disabled,false,'captura filtros antes de bloquear');for(const [name,id] of [['Impuesto','ret-impuesto'],['Periodo','ret-periodo'],['Mes','ret-mes']])this.values.set(name,els[id].value);}}
    get(name){return this.values.get(name);}
    append(name,value){this.values.set(name,value);}
}
const ctx={document:{body:new Element('body'),getElementById:n=>els[n],querySelector:()=>null,createElement:tag=>new Element(tag),createDocumentFragment:()=>new Element('FRAGMENT')},window:{innerHeight:768,addEventListener(){}},requestAnimationFrame:fn=>fn(),FormData:FormDataFake,URL:{createObjectURL:()=> 'blob:sintetico',revokeObjectURL(){}},setTimeout:fn=>fn(),
    fetch:(url,opt)=>{calls.push({url,opt});return new Promise((resolve,reject)=>pending.push({resolve,reject}));},console};
vm.runInNewContext(fs.readFileSync(path.join(__dirname,'../gc.sitio/wwwroot/js/app/libros/certificados-retencion-emitidos.js'),'utf8'),ctx);
const tick=()=>new Promise(r=>setImmediate(r));
const data=(count=251)=>({columnas:Array.from({length:10},(_,i)=>({titulo:'Col '+i,numero:i>=6&&i<=8})),filas:Array.from({length:count},(_,i)=>Array.from({length:10},(_,c)=>c===3?'<script>NO EJECUTAR</script>':`${i}:${c}`)),total:count,contexto:'IB 2026-10',totalBase:'9.999.999.999.999,98',totalRetencion:'8,00',totalLH:'5,00',incompletos:false});
async function reply(body,status=200,mime='application/json'){pending.shift().resolve({ok:status===200,redirected:false,headers:{get:()=>mime},json:async()=>body});await tick();}
async function fileReply(status=200,mime='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',size=50){pending.shift().resolve({ok:status===200,status,redirected:false,headers:{get:()=>mime},json:async()=>({mensaje:'No hay registros mensuales'}),blob:async()=>({size})});await tick();}
(async()=>{
    assert.equal(calls.length,0,'no consulta automática sin Ver');assert.equal(els['btnDetalle'].disabled,true);assert.equal(els['ret-fechas'].disabled,true);assert.equal(els['ret-cuit-campos'].disabled,true);
    assert.equal(els['ret-desde'].value,'2026-10-01');assert.equal(els['ret-hasta'].value,'2026-10-31');
    els['ret-filtros'].fire('submit');els['ret-filtros'].fire('submit');assert.equal(calls.length,1);assert.equal(els['ret-cargando'].hidden,false);assert.equal(els['ret-campos'].disabled,true);assert.equal(els['btnFiltro'].disabled,true);
    await reply(data());assert.equal(els['ret-filas'].children.length,100);assert.equal(els['divFiltro'].hidden,true);assert.equal(els['btnDetalle'].disabled,false);
    assert.equal(els['ret-base'].textContent,'$ 9.999.999.999.999,98','importe grande no convertido a Number');assert.equal(els['ret-filas'].children[0].children[3].textContent,'<script>NO EJECUTAR</script>','datos escritos como texto');
    els['retenciones-emitidas'].fire('gc:page',{detail:{page:3}});assert.equal(els['ret-filas'].children.length,51);assert.equal(calls.length,1,'paginar no reconsulta SP');assert.equal(els['ret-total'].textContent,'$ 8,00');
    els['retenciones-emitidas'].fire('gc:page',{detail:{page:0}});assert.equal(els['ret-filas'].children.length,51);
    els['retenciones-emitidas'].fire('gc:page',{detail:{page:4}});assert.equal(els['ret-filas'].children.length,51);
    els['retenciones-emitidas'].fire('gc:page',{detail:{page:1}});assert.equal(els['ret-filas'].children.length,100);
    els['btnFiltro'].fire('click');assert.equal(els['divFiltro'].hidden,false);els['btnFiltro'].fire('click');assert.equal(els['divFiltro'].hidden,true);assert.equal(els['ret-filas'].children.length,100);
    els['btnDetalle'].fire('click');assert.equal(els['divFiltro'].hidden,false);assert.equal(els['divDetalle'].hidden,true);assert.equal(els['ret-filas'].children.length,0);assert.equal(els['ret-impuesto'].value,'IB');assert.equal(els['btnDetalle'].disabled,true);
    els['ret-filtros'].fire('submit');await reply(data(0));assert.equal(els['ret-filas'].children.length,0);assert.equal(els['ret-vacio'].hidden,false);assert.equal(els['ret-paginador'].children.length,0);
    els['btnFiltro'].fire('click');els['ret-periodo'].value='2025';els['ret-filtros'].fire('change',{target:els['ret-periodo']});assert.equal(els['divDetalle'].hidden,true);assert.equal(els['ret-hasta'].value,'2025-10-31');
    els['ret-por-cuit'].checked=true;els['ret-cuit'].value='%123%';els['ret-filtros'].fire('change',{target:els['ret-por-cuit']});assert.equal(els['ret-cuit-campos'].disabled,false);
    els['ret-por-fecha'].checked=true;els['ret-filtros'].fire('change',{target:els['ret-por-fecha']});assert.equal(els['ret-fechas'].disabled,false);
    els['ret-filtros'].fire('reset');assert.equal(els['ret-periodo'].value,'2026');assert.equal(els['ret-mes'].value,'10');assert.equal(els['ret-por-cuit'].checked,false);assert.equal(els['ret-cuit'].value,'');assert.equal(els['ret-desde'].value,'2026-10-01');assert.equal(els['ret-fechas'].disabled,true);
    els['ret-filtros'].fire('submit');await reply({mensaje:'Sin permiso'},403);assert.equal(els['ret-mensaje'].textContent,'Sin permiso');assert.equal(els['divFiltro'].hidden,false);assert.equal(els['divDetalle'].hidden,true);assert.equal(els['ret-campos'].disabled,false);
    els['ret-filtros'].fire('submit');pending.shift().reject(new TypeError('network'));await tick();assert.equal(els['divDetalle'].hidden,true);assert.equal(els['ret-cargando'].hidden,true);
    els['ret-filtros'].fire('submit');await reply({...data(1),total:2});assert.equal(els['ret-filas'].children.length,0);assert.equal(els['ret-mensaje'].textContent.includes('No se muestran datos parciales'),true);
    els['ret-filtros'].fire('submit');await reply({...data(1),incompletos:true,totalBase:'No disponible'});assert.equal(els['ret-mensaje'].textContent.includes('nulos'),true);assert.equal(els['ret-base'].textContent,'$ No disponible');
    els['btnFiltro'].fire('click');els['ret-tamanio'].value='500';els['ret-filtros'].fire('change',{target:els['ret-tamanio']});assert.equal(els['ret-filas'].children.length,1);assert.equal(els['btnDetalle'].disabled,false);
    els['ret-filtros'].fire('submit');await reply(data(5001));assert.equal(els['ret-filas'].children.length,500);
    const consultasAntes=calls.length, claves=[];
    for(let page=1;page<=11;page++){
        els['retenciones-emitidas'].fire('gc:page',{detail:{page}});
        assert.ok(els['ret-filas'].children.length<=500);
        claves.push(...els['ret-filas'].children.map(r=>r.children[0].textContent));
        assert.equal(els['ret-base'].textContent,'$ 9.999.999.999.999,98');
    }
    assert.equal(claves.length,5001);assert.equal(new Set(claves).size,5001);assert.equal(claves[0],'0:0');assert.equal(claves.at(-1),'5000:0');assert.equal(calls.length,consultasAntes);
    els['retenciones-emitidas'].fire('gc:page',{detail:{page:1.5}});assert.equal(els['ret-filas'].children.length,1);
    els['retenciones-emitidas'].fire('gc:page',{detail:{page:1}});assert.equal(els['ret-filas'].children.length,500);
    assert.equal(els['ret-ddjj'].disabled,false,'DDJJ accesible con filtro cerrado');
    els['ret-ddjj'].fire('click');assert.equal(els['ret-ddjj-dialogo'].open,true);assert.equal(els['ret-campos'].disabled,true);assert.ok(els['ret-ddjj-periodo'].textContent.includes('IB'));
    els['ret-ddjj-cancelar'].fire('click');assert.equal(els['ret-ddjj-dialogo'].open,false);assert.equal(els['ret-filas'].children.length,500);
    els['ret-periodo'].value='2025'; // Sin change: la descarga debe usar la consulta confirmada, no este DOM alterado.
    els['ret-ddjj'].fire('click');els['ret-ddjj-confirmar'].fire('click');els['ret-ddjj-confirmar'].fire('click');
    assert.equal(els['ret-cargando'].hidden,false);assert.equal(els['ret-ddjj'].disabled,true);assert.ok(els['ret-espera-texto'].textContent.includes('DDJJ mensual completa'));
    const request=calls.at(-1);assert.equal(request.url,els['retenciones-emitidas'].dataset.ddjj);assert.deepEqual([...request.opt.body.values.keys()],['__RequestVerificationToken','Impuesto','Periodo','Mes'],'DDJJ no envía CUIT ni fechas ni página');
    assert.equal(request.opt.body.get('Impuesto'),'IB');assert.equal(request.opt.body.get('Periodo'),'2026','usa instantánea de la consulta');els['ret-periodo'].value='2026';await fileReply();assert.equal(downloads.at(-1),'DDJJ_Retenciones_IB_2026-10.xlsx');assert.equal(els['ret-filas'].children.length,500);assert.equal(els['ret-cargando'].hidden,true);assert.equal(els['ret-ddjj'].disabled,false);
    els['ret-ddjj'].fire('click');els['ret-ddjj-confirmar'].fire('click');await fileReply(422,'application/json');assert.equal(els['ret-mensaje'].textContent,'No hay registros mensuales');assert.equal(downloads.length,1);assert.equal(els['ret-filas'].children.length,500);
    els['ret-ddjj'].fire('click');els['ret-ddjj-confirmar'].fire('click');await fileReply(200,'text/html');assert.equal(downloads.length,1);assert.ok(els['ret-mensaje'].textContent.includes('formato'));
    els['ret-ddjj'].fire('click');els['ret-ddjj-confirmar'].fire('click');pending.shift().reject(new TypeError('network'));await tick();assert.equal(els['ret-ddjj'].disabled,false);assert.equal(els['ret-cargando'].hidden,true);
    els['btnFiltro'].fire('click');els['ret-impuesto'].value='GA';els['ret-filtros'].fire('change',{target:els['ret-impuesto']});assert.equal(els['ret-ddjj'].disabled,true);
    els['ret-filtros'].fire('submit');await reply(data(0));assert.equal(els['ret-ddjj'].disabled,false,'Consulta filtrada vacía no implica mes vacío');
    els['ret-ddjj'].fire('click');assert.ok(els['ret-ddjj-periodo'].textContent.includes('TXT sin encabezado'));els['ret-ddjj-confirmar'].fire('click');await fileReply(200,'text/plain; charset=utf-8');assert.equal(downloads.at(-1),'DDJJ_Retenciones_GA_2026-10.txt');
    els['btnDetalle'].fire('click');assert.equal(els['ret-ddjj'].disabled,true);
    els['ret-impuesto'].value='IV';els['ret-filtros'].fire('submit');await reply(data(1));els['ret-ddjj'].fire('click');els['ret-ddjj-confirmar'].fire('click');await fileReply();assert.equal(downloads.at(-1),'DDJJ_Retenciones_IV_2026-10.xlsx');
    els['ret-filtros'].fire('reset');assert.equal(els['ret-ddjj'].disabled,true);
    console.log('PASS: DDJJ mensual IB/IV Excel y GA TXT, alcance confirmado, snapshot, permiso/errores, spinner, doble clic, sin filtros/paginación y consulta conservada.');
    console.log('PASS: filtros, contexto, paginación local completa y totales invariantes, XSS como texto, Filtro/Detalle/Inicializar, spinner, doble clic, vacío, errores y NULL.');
})().catch(e=>{console.error(e);process.exitCode=1;});
