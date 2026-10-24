// Ejecuta el JS instalado en DOM/red simulados. Sin SQL ni navegador.
const fs=require('node:fs'),vm=require('node:vm'),assert=require('node:assert/strict'),path=require('node:path');
function setup(available=true){
    const controls=[],calls=[],pending=[],scroll={scrollTop:0};
    class Element{
        constructor(tag='div'){this.tagName=tag.toUpperCase();this.children=[];this.events={};this.attrs={};this.dataset={};this.style={};this.value='';this.defaultValue='';this.checked=false;this.defaultChecked=false;this.disabled=false;this.hidden=false;this.type='';this.textContent='';this.options=[];this.validity='';}
        addEventListener(n,fn){this.events[n]=fn;} fire(n,arg={}){return this.events[n]?.({preventDefault(){},...arg});}
        setAttribute(n,v){this.attrs[n]=String(v);} appendChild(e){this.children.push(e);return e;}
        replaceChildren(...nodes){this.children=nodes.flatMap(n=>n.tagName==='FRAGMENT'?n.children:[n]);}
        getBoundingClientRect(){return {top:200,height:30};} setCustomValidity(v){this.validity=v;} reportValidity(){return !this.validity;}
        querySelectorAll(){return controls;} querySelector(selector){return selector?.includes('__RequestVerificationToken')?{value:'token-sintetico'}:this.children[0]||null;} closest(){return scroll;} focus(){this.focused=true;}
        showModal(){this.open=true;} close(){this.open=false;this.fire('close');} click(){downloads.push(this.download);} remove(){}
    }
    const names=['percepciones-emitidas','per-filtros','divFiltro','divDetalle','per-panel','per-campos','btnFiltro','btnDetalle','per-mensaje','per-cargando','per-fechas','per-por-fecha','per-cuit-campos','per-por-cuit','per-periodo','per-mes','per-desde','per-hasta','per-tamanio','per-filas','per-columnas','per-paginador','per-cantidad','per-vacio','per-contexto','per-base','per-total','per-lh-bloque','per-lh','per-impuesto','per-cuit','per-ddjj','per-imprimir'];
    names.push('per-ddjj-dialogo','per-ddjj-periodo','per-ddjj-cancelar','per-ddjj-confirmar','per-espera-texto');
    const downloads=[],els=Object.fromEntries(names.map(n=>[n,new Element()]));names.forEach(n=>els[n].id=n);
    function select(id,values,current){const e=els[id];e.tagName='SELECT';e.value=current;e.options=values.map(value=>({value,defaultSelected:value===current}));controls.push(e);}
    select('per-periodo',['2026','2025','2024','2023','2022','2021'],'2026');select('per-mes',Array.from({length:12},(_,i)=>String(i+1)),'10');select('per-tamanio',['50','100','250','500'],'100');select('per-impuesto',['IB','IV'],'IB');
    for(const n of ['per-por-fecha','per-por-cuit']){els[n].type='checkbox';controls.push(els[n]);}
    for(const n of ['per-desde','per-hasta','per-cuit'])controls.push(els[n]);
    els['per-filtros'].reportValidity=()=>controls.every(e=>!e.validity);
    els['percepciones-emitidas'].dataset={disponible:String(available),consulta:'/gcsitio/Libros/CertificadosPercepcionEmitidos/Consultar',ddjj:'/gcsitio/Libros/CertificadosPercepcionEmitidos/Ddjj'};
    els['per-ddjj'].disabled=els['per-imprimir'].disabled=true;
    class FormDataFake{constructor(form){this.map=new Map();if(form){assert.equal(els['per-campos'].disabled,false);this.map=new Map(['Impuesto','Periodo','Mes'].map((name,i)=>[name,els[['per-impuesto','per-periodo','per-mes'][i]].value]));}}get(n){return this.map.get(n);}append(n,v){this.map.set(n,v);}}
    vm.runInNewContext(fs.readFileSync(path.join(__dirname,'../gc.sitio/wwwroot/js/app/libros/certificados-percepcion-emitidos.js'),'utf8'),{
        document:{body:new Element('body'),getElementById:n=>els[n],querySelector:()=>null,createElement:t=>new Element(t),createDocumentFragment:()=>new Element('FRAGMENT')},
        window:{innerHeight:768,addEventListener(){}},requestAnimationFrame:fn=>fn(),FormData:FormDataFake,
        fetch:(url,opt)=>{calls.push({url,opt});return new Promise((resolve,reject)=>pending.push({resolve,reject}));},console,
        URL:{createObjectURL:()=> 'blob:sintetico',revokeObjectURL(){}},setTimeout:fn=>fn()
    });
    return {els,calls,pending,downloads};
}
const tick=()=>new Promise(r=>setImmediate(r));
function data(count=251,impuesto='IB'){
    const n=impuesto==='IB'?16:14;
    return {columnas:Array.from({length:n},(_,i)=>({titulo:'Col '+i,numero:i>=8&&i<=11,textoLargo:i===2||i===4})),
        filas:Array.from({length:count},(_,i)=>Array.from({length:n},(_,c)=>c===2?'<script>NO EJECUTAR</script>':`${i}:${c}`)),total:count,contexto:'IB · Libro IVA 2026-10',
        totalBase:'9.999.999.999.999,98',totalPercepcion:'8,00',totalLH:impuesto==='IB'?'No disponible':null,incompletos:impuesto==='IB'};
}
(async()=>{
    const {els:e,calls,pending,downloads}=setup();
    async function reply(body,status=200,mime='application/json',redirected=false,blob={size:10}){pending.shift().resolve({ok:status===200,status,redirected,headers:{get:()=>mime},json:async()=>body,blob:async()=>blob});await tick();}
    const form=e['per-filtros'],root=e['percepciones-emitidas'];
    assert.equal(calls.length,0);assert.equal(e['btnDetalle'].disabled,true);assert.equal(e['per-fechas'].disabled,true);assert.equal(e['per-desde'].value,'2026-10-01');assert.equal(e['per-hasta'].value,'2026-10-31');
    form.fire('submit');form.fire('submit');assert.equal(calls.length,1);assert.equal(e['per-cargando'].hidden,false);assert.equal(e['per-campos'].disabled,true);assert.equal(e['per-panel'].attrs['aria-busy'],'true');
    root.fire('gc:page',{detail:{page:2}});assert.equal(calls.length,1);await reply(data());
    assert.equal(e['per-filas'].children.length,100);assert.equal(e['divFiltro'].hidden,true);assert.equal(e['btnDetalle'].disabled,false);assert.equal(e['per-base'].textContent,'$ 9.999.999.999.999,98');assert.equal(e['per-lh'].textContent,'No disponible');assert.ok(e['per-mensaje'].textContent.includes('nulos'));
    assert.equal(e['per-filas'].children[0].children[2].textContent,'<script>NO EJECUTAR</script>');assert.equal(e['per-filas'].children[0].children[2].className,'per-long');
    root.fire('gc:page',{detail:{page:3}});assert.equal(e['per-filas'].children.length,51);assert.equal(calls.length,1);assert.equal(e['per-total'].textContent,'$ 8,00');
    for(const page of [0,4,1.5,undefined]){root.fire('gc:page',{detail:{page}});assert.equal(e['per-filas'].children.length,51);}
    e['btnFiltro'].fire('click');assert.equal(e['divFiltro'].hidden,false);e['btnFiltro'].fire('click');assert.equal(e['divFiltro'].hidden,true);assert.equal(e['per-filas'].children.length,51);
    e['btnDetalle'].fire('click');assert.equal(e['divFiltro'].hidden,false);assert.equal(e['divDetalle'].hidden,true);assert.equal(e['per-filas'].children.length,0);assert.equal(e['per-impuesto'].value,'IB');assert.equal(e['btnDetalle'].disabled,true);
    form.fire('submit');await reply(data(0));assert.equal(e['per-vacio'].hidden,false);assert.equal(e['per-paginador'].children.length,0);assert.equal(e['btnDetalle'].disabled,false);
    e['btnFiltro'].fire('click');e['per-periodo'].value='2025';form.fire('change',{target:e['per-periodo']});assert.equal(e['divDetalle'].hidden,true);assert.equal(e['per-hasta'].value,'2025-10-31');
    e['per-por-fecha'].checked=true;form.fire('change',{target:e['per-por-fecha']});assert.equal(e['per-fechas'].disabled,false);
    e['per-desde'].value='2025-10-20';e['per-hasta'].value='2025-10-10';const antes=calls.length;form.fire('submit');assert.equal(calls.length,antes);assert.ok(e['per-hasta'].validity.includes('anterior'));
    // El navegador valida ANTES de emitir submit: corregir debe limpiar el error sin Inicializar.
    e['per-hasta'].value='2025-10-25';form.fire('input',{target:e['per-hasta']});
    assert.equal(e['per-hasta'].validity,'','Hasta corregido limpia el error durante input');
    assert.equal(form.reportValidity(),true,'El navegador vuelve a permitir submit');
    form.fire('submit');assert.equal(calls.length,antes+1);await reply(data(1));
    assert.equal(e['per-periodo'].value,'2025');assert.equal(e['per-por-fecha'].checked,true);assert.equal(e['per-hasta'].value,'2025-10-25');
    e['per-desde'].value='2025-10-28';form.fire('input',{target:e['per-desde']});assert.ok(e['per-hasta'].validity.includes('anterior'));
    assert.equal(form.reportValidity(),false,'Rango todavía invertido sigue bloqueado antes de submit');
    e['per-desde'].value='2025-10-05';form.fire('input',{target:e['per-desde']});form.fire('change',{target:e['per-desde']});
    assert.equal(e['per-hasta'].validity,'','También se recupera corrigiendo Desde');
    assert.equal(e['divDetalle'].hidden,true);assert.equal(e['per-ddjj'].disabled,true,'Cambiar fecha invalida la consulta anterior como antes');
    const antesCorregirDesde=calls.length;form.fire('submit');assert.equal(calls.length,antesCorregirDesde+1);await reply(data(1));
    e['per-desde'].value='2025-10-26';form.fire('input',{target:e['per-desde']});assert.ok(e['per-hasta'].validity);
    e['per-por-fecha'].checked=false;form.fire('change',{target:e['per-por-fecha']});
    assert.equal(e['per-hasta'].validity,'','Apagar rango elimina el error personalizado obsoleto');assert.equal(e['per-fechas'].disabled,true);
    const antesApagar=calls.length;form.fire('submit');assert.equal(calls.length,antesApagar+1);await reply(data(1));
    e['per-por-fecha'].checked=true;form.fire('change',{target:e['per-por-fecha']});assert.ok(e['per-hasta'].validity);
    e['per-hasta'].value='2025-10-26';form.fire('change',{target:e['per-hasta']});
    assert.equal(e['per-hasta'].validity,'','Change y fechas iguales son válidos');
    e['per-por-cuit'].checked=true;e['per-cuit'].value='%ABC%';form.fire('change',{target:e['per-por-cuit']});assert.equal(e['per-cuit-campos'].disabled,false);
    form.fire('reset');assert.equal(e['per-periodo'].value,'2026');assert.equal(e['per-mes'].value,'10');assert.equal(e['per-por-cuit'].checked,false);assert.equal(e['per-cuit'].value,'');assert.equal(e['per-fechas'].disabled,true);assert.equal(e['per-desde'].value,'2026-10-01');
    for(const status of [400,401,403,503,504]){form.fire('submit');await reply({mensaje:'Error controlado '+status},status);assert.equal(e['per-mensaje'].textContent,'Error controlado '+status);assert.equal(e['per-filas'].children.length,0);assert.equal(e['divFiltro'].hidden,false);assert.equal(e['per-cargando'].hidden,true);}
    form.fire('submit');pending.shift().reject(new TypeError('red'));await tick();assert.equal(e['divDetalle'].hidden,true);assert.equal(e['per-campos'].disabled,false);
    form.fire('submit');await reply({},200,'text/html');assert.ok(e['per-mensaje'].textContent.includes('sesión'));
    form.fire('submit');await reply({},200,'application/json',true);assert.ok(e['per-mensaje'].textContent.includes('sesión'));
    for(const bad of [{...data(1),total:2},{...data(1),totalPercepcion:8},{...data(1),columnas:[null]}, {...data(1),filas:[[123]]}]){
        form.fire('submit');await reply(bad);assert.equal(e['per-filas'].children.length,0);assert.ok(e['per-mensaje'].textContent.includes('No se muestran datos parciales'));
    }
    e['per-tamanio'].value='500';form.fire('submit');const many=data(5001);many.filas[1]=many.filas[0].slice();await reply(many);
    const countCalls=calls.length,seen=[];
    for(let page=1;page<=11;page++){
        root.fire('gc:page',{detail:{page}});assert.ok(e['per-filas'].children.length<=500);seen.push(...e['per-filas'].children.map(r=>r.children[0].textContent));assert.equal(e['per-total'].textContent,'$ 8,00');
    }
    assert.equal(seen.length,5001);assert.equal(new Set(seen).size,5000,'se preserva duplicado exacto del SP');assert.equal(seen.at(-1),'5000:0');assert.equal(calls.length,countCalls);
    e['btnFiltro'].fire('click');e['per-impuesto'].value='IV';form.fire('change',{target:e['per-impuesto']});assert.equal(e['per-filas'].children.length,0);
    form.fire('submit');await reply({...data(14,'IV'),incompletos:false});assert.equal(e['per-filas'].children.length,14);assert.equal(e['per-columnas'].children[0].children.length,14);assert.equal(e['per-lh-bloque'].hidden,true);
    assert.equal(e['per-ddjj'].disabled,false);
    const antesDdjj=calls.length; e['per-ddjj'].fire('click');assert.equal(e['per-ddjj-dialogo'].open,true);assert.equal(e['per-campos'].disabled,true);
    assert.ok(e['per-ddjj-periodo'].textContent.includes('IV'));e['per-ddjj-cancelar'].fire('click');assert.equal(calls.length,antesDdjj);assert.equal(e['per-campos'].disabled,false);
    e['per-ddjj'].fire('click');e['per-ddjj-dialogo'].close();assert.equal(e['per-ddjj'].disabled,false);
    // El pedido usa la consulta aplicada, no controles que pudieran cambiar sin un evento.
    e['per-impuesto'].value='IB';e['per-ddjj'].fire('click');e['per-ddjj-confirmar'].fire('click');e['per-ddjj-confirmar'].fire('click');assert.equal(calls.length,antesDdjj+1);
    const req=calls.at(-1);assert.match(req.url,/\/gcsitio\/.*\/Ddjj$/);assert.deepEqual([...req.opt.body.map.keys()],['__RequestVerificationToken','Impuesto','Periodo','Mes']);assert.equal(req.opt.body.get('Impuesto'),'IV');assert.equal(req.opt.credentials,'same-origin');assert.equal(e['per-cargando'].hidden,false);assert.equal(e['per-campos'].disabled,true);
    const mimeExcel='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';await reply(null,200,mimeExcel);
    assert.equal(downloads.at(-1),'DDJJ_Percepciones_IV_2026-10.xlsx');assert.equal(e['per-filas'].children.length,14);assert.equal(e['per-cargando'].hidden,true);assert.equal(e['per-ddjj'].disabled,false);
    for(const status of [400,401,403,422,429,503,504]){e['per-ddjj'].fire('click');e['per-ddjj-confirmar'].fire('click');await reply({mensaje:'DDJJ '+status},status);assert.equal(e['per-mensaje'].textContent,'DDJJ '+status);assert.equal(downloads.length,1);assert.equal(e['per-filas'].children.length,14);}
    for(const [mime,redirected,blob] of [['text/html',false,{size:10}],[mimeExcel,true,{size:10}],[mimeExcel,false,{size:0}]]){e['per-ddjj'].fire('click');e['per-ddjj-confirmar'].fire('click');await reply(null,200,mime,redirected,blob);assert.equal(downloads.length,1);assert.equal(e['per-ddjj'].disabled,false);}
    e['per-impuesto'].value='IV';form.fire('change',{target:e['per-impuesto']});assert.equal(e['per-ddjj'].disabled,true);
    form.fire('submit');await reply({...data(0,'IV'),incompletos:false});assert.equal(e['per-ddjj'].disabled,false,'vacío filtrado permite DDJJ mensual');
    e['btnDetalle'].fire('click');assert.equal(e['per-ddjj'].disabled,true);
    const blocked=setup(false);blocked.els['per-filtros'].fire('submit');assert.equal(blocked.calls.length,0);assert.equal(blocked.els['btnFiltro'].disabled,true);
    const razor=fs.readFileSync(path.join(__dirname,'../gc.sitio/Areas/Libros/Views/CertificadosPercepcionEmitidos/Index.cshtml'),'utf8');
    assert.match(razor,/id="per-ddjj"[^>]* disabled/);assert.doesNotMatch(razor,/per-imprimir|bx-printer|DDJJ e impresión pendientes/);assert.match(razor,/data-ddjj="@Url.Action\("Ddjj"\)"/);assert.equal((razor.match(/src="~\/js\/components\/gc-pager.js"/g)||[]).length,1);
    console.log('PASS: Percepciones IB/IV, 5001 filas, totales, LH NULL, spinner, filtros, botones, vacíos y errores; DDJJ mensual, confirmación/cancelación, pedido aplicado, doble envío, antiforgery, errores y recuperación; Imprimir ausente.');
})().catch(e=>{console.error(e);process.exitCode=1;});
