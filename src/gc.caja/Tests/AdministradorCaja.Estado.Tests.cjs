const fs=require('node:fs'), vm=require('node:vm'), assert=require('node:assert/strict');
async function probar(respuesta, resultado) {
    class Elemento {
        constructor(){this.events={};this.dataset={};this.hidden=true;this.children=[];this.value='token';}
        addEventListener(n,f){this.events[n]=f;} setAttribute(){} replaceChildren(){this.children=[];} appendChild(e){this.children.push(e);} querySelector(){return {value:'token'};}
        get innerHTML(){return this.textContent;}
    }
    const ids=Object.fromEntries(['admCaja','admAbrir','admCerrar','admActualizar','admVolver','admConsulta','admPuestos','admEstado','admSeccionApertura','admSeccionCierre','admSeccionPuestos'].map(n=>[n,new Elemento()]));
    const modales=[], navegaciones=[], posts=[];
    ids.admCaja.dataset={puestos:'/puestos',apertura:'/apertura',cierre:'/cierre',solicitud:'prueba',sucursal:'0000'};
    ids.admVolver.href='https://caja.test/geco/';
    const document={getElementById:n=>ids[n],createElement:()=>new Elemento(),addEventListener:(_,f)=>f()};
    vm.runInNewContext(fs.readFileSync(__dirname+'/../wwwroot/js/app/administrador-caja.js','utf8'),{
        document,window:{addEventListener(){},location:{assign:url=>navegaciones.push(url)}},
        fetch:async(url, opciones)=>{
            if(opciones.method==='POST'){
                posts.push(url);
                if(resultado instanceof Error) throw resultado;
                return {ok:true,json:async()=>resultado};
            }
            return {ok:true,json:async()=>respuesta};
        },URLSearchParams,
        $:()=>({modal(){}}),AbrirMensaje(...args){modales.push(args);}
    });
    await new Promise(resolve=>setImmediate(resolve));
    assert.equal(modales.length,0);
    return Object.assign(ids,{modales,navegaciones,posts});
}
(async()=>{
    for(const c of [
        {ok:true,habilitada:false,puestos:[],a:true,c:false,p:false,abre:true,cierra:false},
        {ok:true,habilitada:true,puestos:[],a:false,c:true,p:true,abre:false,cierra:true},
        {ok:true,habilitada:true,puestos:[{caja_id:'0001'}],a:false,c:true,p:true,abre:false,cierra:false},
        {ok:false,habilitada:true,puestos:[],a:false,c:true,p:true,abre:false,cierra:false},
        {ok:false,habilitada:null,puestos:[],a:false,c:false,p:false,abre:false,cierra:false}
    ]) {
        const v=await probar({...c,mensaje:'Estado de prueba'});
        assert.equal(!v.admSeccionApertura.hidden,c.a);assert.equal(!v.admSeccionCierre.hidden,c.c);assert.equal(!v.admSeccionPuestos.hidden,c.p);
        assert.equal(!v.admAbrir.disabled,c.abre);assert.equal(!v.admCerrar.disabled,c.cierra);
    }
    let flujos=0;
    for(const apertura of [true,false]) {
        for(const decision of ['SI','NO']) {
            const v=await probar({ok:true,habilitada:!apertura,puestos:[],mensaje:'OK'},{ok:true,mensaje:'Operación OK'});
            const boton=apertura?v.admAbrir:v.admCerrar;
            boton.events.click();
            assert.equal(v.navegaciones.length,0);
            await v.modales[0][2]('SI');
            assert.deepEqual(v.posts,[apertura?'/apertura':'/cierre']);
            assert.equal(v.modales[1][0],'Operación completada');
            assert.equal(v.navegaciones.length,0,'Esperar cierre del mensaje de resultado');
            assert.equal(v.admAbrir.disabled,true);assert.equal(v.admCerrar.disabled,true);
            boton.events.click();assert.equal(v.posts.length,1);
            v.modales[1][2](decision);
            v.modales[1][2](decision);
            assert.deepEqual(v.navegaciones,[v.admVolver.href],'Aceptar o X vuelve una sola vez al inicio');
            flujos++;
        }
        for(const resultado of [{ok:false,mensaje:'Rechazado'},{ok:false,incierto:true,mensaje:'Verificar'},new Error('Sin conexión')]) {
            const v=await probar({ok:true,habilitada:!apertura,puestos:[],mensaje:'OK'},resultado);
            (apertura?v.admAbrir:v.admCerrar).events.click();
            await v.modales[0][2]('SI');v.modales[1][2]('SI');
            assert.equal(v.navegaciones.length,0,'Errores y resultados inciertos permanecen en Administración');
            const bloqueado=resultado.incierto===true||resultado instanceof Error;
            assert.equal(v.admAbrir.disabled,apertura?bloqueado:true);
            assert.equal(v.admCerrar.disabled,apertura?true:bloqueado);
            flujos++;
        }
        const v=await probar({ok:true,habilitada:!apertura,puestos:[],mensaje:'OK'},{ok:true,mensaje:'OK'});
        (apertura?v.admAbrir:v.admCerrar).events.click();await v.modales[0][2]('NO');
        assert.equal(v.posts.length,0);assert.equal(v.navegaciones.length,0);flujos++;
    }
    console.log(`5 escenarios de estado y ${flujos} flujos de resultado/navegación aprobados con JavaScript real y HTTP simulado.`);
})().catch(e=>{console.error(e);process.exitCode=1;});
