const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const reglas = require('../wwwroot/js/app/gestion/inv-conteo-reglas.js');
let passed = 0;
function test(nombre, ejecutar) { ejecutar(); passed++; console.log('OK:', nombre); }
test('Bultos x presentación + US', () => assert.equal(reglas.calcular('U', 15, 12, 2).invd_cantidad, 182));
test('Solo US enteras', () => assert.equal(reglas.calcular('U', 0, 12, 5).invd_cantidad, 5));
test('Solo bultos', () => assert.equal(reglas.calcular('U', 2, 6, 0).invd_cantidad, 12));
test('Pesable con milésimas', () => assert.equal(reglas.calcular('P', 0, 1, 0.125).invd_cantidad, 0.125));
for (const [nombre, valores] of [
    ['Sin up_tipo', ['', 1, 1, 0]], ['Cero total', ['U', 0, 12, 0]],
    ['Negativos', ['U', -1, 12, 0]], ['Bultos fraccionarios', ['U', 1.2, 12, 0]],
    ['Presentación cero', ['U', 1, 0, 0]], ['Presentación fuera de smallint', ['U', 1, 32768, 0]],
    ['US fraccionarias en unidades', ['U', 0, 12, 0.5]], ['Más de tres decimales', ['P', 0, 1, 0.1234]],
    ['Pesable con bultos', ['P', 1, 1, 0.5]], ['NaN', ['U', NaN, 1, 1]]
]) test(nombre + ' se rechaza', () => assert.throws(() => reglas.calcular(...valores)));
test('Acumular misma presentación', () => assert.equal(reglas.acumular('U', reglas.calcular('U', 2, 6, 1), reglas.calcular('U', 1, 6, 2)).invd_cantidad, 21));
test('Acumular distinta presentación conserva el total', () => {
    const r = reglas.acumular('U', reglas.calcular('U', 2, 6, 1), reglas.calcular('U', 1, 10, 2));
    assert.deepEqual(r, {invd_bulto: 2, invd_unidad_pres: 10, invd_unidad_suelta: 5, invd_cantidad: 25});
});
test('Acumular pesables sin error binario', () => assert.equal(reglas.acumular('P', reglas.calcular('P', 0, 1, 0.1), reglas.calcular('P', 0, 1, 0.2)).invd_cantidad, 0.3));

// DOM y transporte simulados: ejecuta las funciones reales, sin base de datos.
function fixture(conFila = true) {
    const filas = conFila ? [{attrs: {'data-p-id':'000001', 'data-up-id':'07', 'data-up-tipo':'U', 'data-bultos':2,
        'data-presentacion':6, 'data-unidad-suelta':1, 'data-cantidad':13, 'data-box-id':'01000000000', 'data-carga-nro':1},
        cells: ['000001','A < B',2,6,1,13]}] : [];
    const values = {'#pId':'000001', '#btos':'1', '#up':'6', '#uns':'2'};
    const props = {}, dialogs = [], calls = [], ready = [];
    function wrapper(items = [], selector = '') {
        let content = '';
        const w = {length:items.length,
            attr(k,v) { if (typeof k === 'object') { items.forEach(i=>Object.assign(i.attrs,k)); return w; }
                if(v === undefined) return items[0]?.attrs[k]; items.forEach(i=>i.attrs[k]=v); return w; },
            val(v) {if(v === undefined) return values[selector]; values[selector]=v; return w;},
            prop(k,v) { if(v === undefined) return props[selector+':'+k]; props[selector+':'+k]=v; return w; },
            text(v) { if(v === undefined) return items[0]?.cells ? items[0].cells[1] : content; content=String(v); return w; },
            html(v) { if(v === undefined) return content.replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;'); return w; },
            find(s) { const m=s.match(/td:eq\((\d+)\)/); if(m) return {text(v) { if(v===undefined) return items[0]?.cells[+m[1]];
                items.forEach(i=>i.cells[+m[1]]=String(v)); return this; }}; return wrapper([],s); },
            filter(fn) {return wrapper(items.filter(i=>fn.call(i)),selector);}, each(fn) {items.forEach(i=>fn.call(i)); return w;},
            off(){return w;}, on(){return w;}, hide(){return w;}, show(){return w;}, modal(){return w;}, trigger(){return w;}, toggle(){return w;},
            closest(){return wrapper(items);}, remove(){items.forEach(i=>filas.splice(filas.indexOf(i),1)); return w;}
        }; return w;
    }
    const $ = s => { if(typeof s==='function') {ready.push(s); return;}
        if(typeof s==='object') return wrapper([s]);
        return wrapper(s==='#tbGridConteoProductos tbody tr[data-p-id]' ? filas : [],s); };
    $.ajax = o=>calls.push(o);
    const c = { $, console, URL, InventarioCantidades:reglas, NormalizarNumeroEntrada:x=>String(x).replaceAll(',',''),
        estado:{inv_nro:'INV-1',tipo:'B',tipo_id:'01000000000'}, productoBase:{p_id:'000001',p_desc:'A < B',up_id:'07',up_tipo:'U',p_unidad_pres:6},
        invValidarProductoUrl:'/validar', AbrirMensaje:(...a)=>dialogs.push(a), ConfigurarEntradaCantidadProducto(){}, analizaEnterInput(){},
        IniciarConfirmacionSegura:()=>({}), FinalizarConfirmacionSegura(){}, window:{location:{href:'https://localhost/Gestion/Inventario/CargaConteo'}} };
    vm.createContext(c); vm.runInContext(fs.readFileSync(path.join(__dirname,'../wwwroot/js/app/gestion/inv-conteo.js'),'utf8'),c);
    const seleccionar = () => vm.runInContext('productoValidadoInventario = productoBase',c);
    return {c,filas,props,values,dialogs,calls,ready,seleccionar};
}
test('Conteo recuperado habilita confirmar',()=>{const f=fixture(); f.c.actualizarContadorProductos(); assert.equal(f.props['#btnConfirmarConteo:disabled'],false);});
test('Conteo vacío deshabilita y no envía',()=>{const f=fixture(false); f.c.actualizarContadorProductos(); f.c.confirmarConteo(); f.c.ejecutarConfirmacionConteo([]); assert.equal(f.props['#btnConfirmarConteo:disabled'],true); assert.equal(f.calls.length,0);});
test('Snapshot no pierde US ni presentación',()=>{const f=fixture(); const p=f.c.obtenerProductosDelGrid()[0]; assert.equal(p.invd_unidad_suelta,1); assert.equal(p.invd_unidad_pres,6); assert.equal(p.invd_cantidad,13);});
test('Modal tiene tres opciones y escapa descripción',()=>{const f=fixture(); f.seleccionar(); f.c.cargarConteoEnGrid(); assert.deepEqual(Array.from(f.dialogs[0][4]),['Acumular','Reemplazar','Cancelar carga']); assert.match(f.dialogs[0][1],/A &lt; B/);});
for(const [opcion,total] of [['SI',21],['SI2',8],['NO',13]]) test('Duplicado '+opcion,()=>{
    const f=fixture(); f.seleccionar(); f.c.cargarConteoEnGrid(); f.dialogs[0][2](opcion);
    assert.equal(Number(f.filas[0].attrs['data-cantidad']),total); assert.equal(f.calls.length,0);
});
test('Validación rechazada no prepara producto',()=>{const f=fixture(); f.c.verificaEstadoCont(); f.calls[0].success({error:false,warn:true,msg:'Fuera del rubro'}); f.c.cargarConteoEnGrid(); assert.equal(f.filas[0].attrs['data-cantidad'],13);});
test('Validación aceptada prepara el producto',()=>{const f=fixture(); f.c.verificaEstadoCont(); f.calls[0].success({error:false,warn:false,msg:'OK'}); assert.equal(f.props['#btnCargaConteo:disabled'],false);});
test('up_tipo P prevalece sobre up_id 07',()=>{const f=fixture(); f.c.productoBase.up_tipo='P'; f.c.verificaEstadoCont(); f.calls[0].success({error:false,warn:false}); assert.equal(f.props['#btos:disabled'],true); assert.equal(f.values['#up'],1);});
test('Doble envío y snapshot completo',()=>{const f=fixture(); const ps=f.c.obtenerProductosDelGrid(); f.c.ejecutarConfirmacionConteo(ps); f.c.ejecutarConfirmacionConteo(ps); assert.equal(f.calls.length,1); assert.equal(JSON.parse(f.calls[0].data).json[0].invd_cantidad,13);});
test('Error de red impide reenvío y conserva la grilla',()=>{const f=fixture(); const ps=f.c.obtenerProductosDelGrid(); f.c.ejecutarConfirmacionConteo(ps); f.calls[0].error(); f.c.ejecutarConfirmacionConteo(ps); assert.equal(f.calls.length,1); assert.equal(f.filas.length,1); assert.equal(f.dialogs.at(-1)[0],'Verificar conteo');});
test('Respuesta incompleta no simula éxito',()=>{const f=fixture(); f.c.ejecutarConfirmacionConteo(f.c.obtenerProductosDelGrid()); f.calls[0].success({}); assert.equal(f.dialogs.at(-1)[0],'Verificar conteo');});
test('Rechazo funcional permite corregir sin perder filas',()=>{const f=fixture(); f.c.ejecutarConfirmacionConteo(f.c.obtenerProductosDelGrid()); f.calls[0].success({error:false,warn:true,msg:'No pertenece'}); assert.equal(f.props['#btnConfirmarConteo:disabled'],false); assert.equal(f.filas.length,1);});
test('Éxito no permite confirmar nuevamente',()=>{const f=fixture(); const ps=f.c.obtenerProductosDelGrid(); f.c.ejecutarConfirmacionConteo(ps); f.calls[0].success({error:false,warn:false,msg:'OK'}); f.c.ejecutarConfirmacionConteo(ps); assert.equal(f.calls.length,1); assert.equal(f.props['#btnConfirmarConteo:disabled'],true);});
test('Reiniciar BOX navega sin escribir en base',()=>{const f=fixture(); f.c.preguntarRecuperacion=true; f.ready[0](); f.dialogs[0][2]('SI2'); assert.match(f.c.window.location.href,/reiniciar=true/); assert.equal(f.calls.length,0);});
console.log(passed+' pruebas de Inventario aprobadas (DOM/API simulados, sin persistencia).');
