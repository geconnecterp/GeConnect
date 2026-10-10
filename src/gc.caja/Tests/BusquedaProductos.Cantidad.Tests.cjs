const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const productos = fs.readFileSync(__dirname + '/../wwwroot/js/app/prodfact.js', 'utf8');
const busqueda = fs.readFileSync(__dirname + '/../wwwroot/js/app/busquedasV02.js', 'utf8');
function funcion(texto, nombre) {
    const inicio = texto.indexOf('function ' + nombre + '(');
    assert.ok(inicio >= 0, nombre);
    const fin = texto.indexOf('\n}', inicio);
    return texto.slice(inicio, fin + 2);
}
const codigo = ['selectRegDbl'].map(n => funcion(busqueda,n)).concat(
    ['BuscarProductos','procesarEntradaCodigo','procesarCodigoConCantidad','buscarProductoPorCodigo'].map(n => funcion(productos,n))
).join('\n');
function escenario(entrada, ean = '-', cancelar = false) {
    let valor = entrada, solicitud, textoEnviado, avisos = [], foco = 0;
    const campo = {
        val(v) { if (v === undefined) return valor; valor = v; return this; },
        prop() { return this; }, trigger(n) { if(n === 'focus') foco++; return this; }
    };
    const otro = { length:1, find(){return this;}, val(){return this;}, on(){return this;}, prop(){return this;}, removeClass(){return this;}, addClass(){return this;}, modal(){return this;}, html(){return this;} };
    const $ = selector => selector === '#txtCodigoProducto' ? campo : otro;
    $.ajax = opciones => { solicitud = opciones.data; textoEnviado = valor; opciones.complete(); };
    const contexto = { $, console:{log(){},error(){},warn(){}},setTimeout:f=>f(),
        origenCargaActual:'directo', modoBloqueoGrilla:null,cajaAcumulaProductos:false,
        TIPO_CARGA:{PRODUCTO:'P'}, REGEX_CANTIDAD_COMODIN:/^(\d+(?:\.\d{1,3})?)\+(.+)$/,
        mostrarMensajeEstado:(...a)=>avisos.push(a),
        procesarCodigoSimple: id => contexto.buscarProductoPorCodigo('P',id,1,true,contexto.origenCargaActual)
    };
    vm.createContext(contexto); vm.runInContext(codigo,contexto);
    if (cancelar) {
        contexto.BuscarProductos(); otro.modal('hide');
        return {valor, solicitud};
    }
    contexto.selectRegDbl({cells:['031225','SANTA ANA QUESO CUARTIROLO',ean].map(innerText=>({innerText}))});
    return {valor, solicitud, textoEnviado, avisos, foco, origen:contexto.origenCargaActual};
}
let casos = 0;
for (const [entrada,cantidad,texto] of [
    ['4.350',4.350,'4.350+031225'], ['2.357+',2.357,'2.357+031225'],
    ['2',2,'2+031225'], ['2+',2,'2+031225'], ['0.001',0.001,'0.001+031225'],
    [' 4.350+ ',4.350,'4.350+031225']
]) {
    const r = escenario(entrada);
    assert.equal(r.solicitud.cantidad,cantidad); assert.equal(r.solicitud.valor,'031225');
    assert.equal(r.solicitud.bulto,true); assert.equal(r.textoEnviado,texto);
    assert.equal(r.origen,'busquedaAvanzada'); assert.equal(r.avisos.length,0); casos++;
}
// Recorrido anterior: sin cantidad, sin EAN agrega uno; con EAN prepara confirmación manual.
for(const entrada of ['', 'codigo-anterior']) {
    const r = escenario(entrada); assert.equal(r.solicitud.cantidad,1); casos++;
    const e = escenario(entrada,'7791234567890'); assert.equal(e.solicitud,undefined);
    assert.equal(e.valor,'031225'); assert.equal(e.avisos.length,0); casos++;
}
for(const entrada of ['4.350','2.357+','2','2+']) {
    const r = escenario(entrada,'7791234567890'); assert.equal(r.solicitud,undefined);
    assert.equal(r.valor,entrada); assert.equal(r.avisos.length,1); assert.ok(r.foco); casos++;
}
for(const entrada of ['0','0+','-2','1.2345','2++','2,357','+']) {
    const r=escenario(entrada); assert.equal(r.solicitud,undefined); assert.equal(r.valor,entrada);
    assert.equal(r.avisos.length,1); casos++;
}
for(const entrada of ['4.350','2.357+']) {
    const r=escenario(entrada,'-',true); assert.equal(r.valor,entrada); assert.equal(r.solicitud,undefined); casos++;
}
console.log(casos + ' escenarios aprobados: cantidades, decimales, EAN, valores inválidos y recorridos anteriores.');
