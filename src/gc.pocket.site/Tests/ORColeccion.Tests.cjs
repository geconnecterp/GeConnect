const fs = require('node:fs'), vm = require('node:vm'), path = require('node:path'), assert = require('node:assert/strict');
const script = fs.readFileSync(path.join(__dirname, '../wwwroot/js/app/or/orvalidaProducto.js'), 'utf8');
let passed = 0;
function fixture() {
    const calls = [], dialogs = [], values = {'#up':'10','#box':'30','#unid':'0','#fvto':'2027-01-01'};
    function $(selector) {
        let text = '';
        return { text(v) { text=v; return this; }, html() { return String(text).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;'); },
            prop() { return this; }, modal() { return this; }, trigger() { return this; },
            val(v) { if (v === undefined) return values[selector]; values[selector]=v; return this; }, is() { return true; } };
    }
    const c = { $, console, Number, setTimeout, productoActualOR: {item:2,or_compte:'OR-1',p_id:'000001',p_desc:'A < B',box_id:'01000000000',colectado:200,bulto:20,us:0,unidad_pres:10},
        productoBase:{p_id:'000001',p_desc:'A < B',up_id:'07'}, esReemplazoOR:false, contextoCargaOR:'contexto-prueba',
        NormalizarNumeroEntrada: x=>x, AbrirWaiting(){}, CerrarWaiting(){},
        AbrirMensaje(...a){ dialogs.push(a); }, PostGen(...a){ calls.push(a); },
        ResguardarProductoCarritoORUrl:'/guardar', proximoProductoUrl:'/lista', window:{location:{href:''}} };
    vm.createContext(c); vm.runInContext(script,c);
    return {c,calls,dialogs,values};
}
function test(name,run) { run(); passed++; console.log('OK:',name); }
test('Consulta antes de enviar y texto escapado',()=>{ const f=fixture(); f.c.cargarCarritoOR(); assert.equal(f.calls.length,0); assert.deepEqual(Array.from(f.dialogs[0][4]),['Sobrescribir carga','Acumular','Cancelar']); assert.match(f.dialogs[0][1],/A &lt; B/); });
test('Cancelar no carga',()=>{ const f=fixture(); f.c.cargarCarritoOR(); f.dialogs[0][2]('NO'); assert.equal(f.calls.length,0); });
test('Acumular envía incremento y snapshot',()=>{ const f=fixture(); f.c.cargarCarritoOR(); f.dialogs[0][2]('SI2'); const d=f.calls[0][0]; assert.equal(d.cantidad,300); assert.equal(d.cantidadPrevia,200); assert.equal(d.modoCarga,'acumular'); assert.equal(d.item,2); assert.equal(d.orCompte,'OR-1'); });
test('Sobrescribir envía cantidad nueva',()=>{ const f=fixture(); f.c.cargarCarritoOR(); f.dialogs[0][2]('SI'); assert.equal(f.calls[0][0].modoCarga,'sobrescribir'); assert.equal(f.calls[0][0].cantidad,300); });
test('Reemplazo no acumula el original',()=>{ const f=fixture(); f.c.esReemplazoOR=true; f.c.productoBase.p_id='000002'; f.c.cargarCarritoOR(); assert.equal(f.calls[0][0].modoCarga,'nueva'); assert.equal(f.calls[0][0].productoOriginal,'000001'); assert.equal(f.calls[0][0].p_id,'000002'); });
test('Mismo producto en reemplazo',()=>{ const f=fixture(); f.c.esReemplazoOR=true; f.c.cargarCarritoOR(); assert.equal(f.calls.length,1); });
test('Exceso llega al servidor',()=>{ const f=fixture(); f.c.productoActualOR.colectado=0; f.c.productoActualOR.pedido=20; f.c.cargarCarritoOR(); assert.equal(f.calls.length,1); });
test('Fracción 0,5 sin bultos',()=>{ const f=fixture(); f.c.productoActualOR.colectado=0; f.c.productoBase.up_id='KG'; f.values['#up']='1'; f.values['#unid']='0.5'; f.c.cargarCarritoOR(); assert.equal(f.calls[0][0].cantidad,0.5); assert.equal(f.calls[0][0].bulto,0); });
test('NaN no se envía',()=>{ const f=fixture(); f.values['#box']='abc'; f.c.cargarCarritoOR(); assert.equal(f.calls.length,0); });
test('Doble click no duplica envío',()=>{ const f=fixture(); f.c.esReemplazoOR=true; f.c.cargarCarritoOR(); f.c.cargarCarritoOR(); assert.equal(f.calls.length,1); });
test('Cambio concurrente exige otra confirmación',()=>{ const f=fixture(); f.c.productoActualOR.colectado=0; f.c.cargarCarritoOR(); f.calls[0][2]({warn:true,reconsultar:true,producto:{...f.c.productoActualOR,colectado:100},msg:'Cambió'}); assert.equal(f.c.productoActualOR.colectado,100); assert.equal(f.calls.length,1); assert.equal(f.c.window.location.href,''); });
test('Rechazo no simula éxito',()=>{ const f=fixture(); f.c.esReemplazoOR=true; f.c.cargarCarritoOR(); f.calls[0][2]({warn:true,msg:'Rechazado'}); assert.equal(f.c.window.location.href,''); assert.equal(f.dialogs.at(-1)[5],'warn!'); });
test('Éxito regresa al listado',()=>{ const f=fixture(); f.c.esReemplazoOR=true; f.c.cargarCarritoOR(); f.calls[0][2]({warn:false,error:false,msg:'OK'}); f.dialogs.at(-1)[2](); assert.equal(f.c.window.location.href,'/lista'); });
test('Error de red no reenvía',()=>{ const f=fixture(); f.c.esReemplazoOR=true; f.c.cargarCarritoOR(); f.calls[0][3](); f.c.cargarCarritoOR(); assert.equal(f.calls.length,1); f.dialogs.at(-1)[2](); assert.equal(f.c.window.location.href,'/lista'); });
console.log(passed+' pruebas JS OR aprobadas (servicios simulados, sin movimientos).');
