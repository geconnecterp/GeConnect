const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const root = path.join(__dirname, '..');
const source = fs.readFileSync(path.join(root, 'wwwroot/js/app/factFacturaEmitida.js'), 'utf8');
const grilla = fs.readFileSync(path.join(root, 'wwwroot/js/app/prodfact.js'), 'utf8');
function extract(text, name) {
    const match = text.match(new RegExp(`^function ${name}\\([^]*?^}`, 'm'));
    assert.ok(match, name);
    return match[0];
}
const noop = () => {};
const context = vm.createContext({
    console: { log: noop, warn: noop, error: noop },
    $: () => ({ text: noop, val: noop }),
    normalizarNumero: (value, fallback) => Number.isFinite(Number(value)) ? Number(value) : fallback,
    formatearNumero: String,
    calcularPrecioTotal: p => Math.round(p.p_pvta * p.cantidad_tot * 100) / 100,
    recalcularTotalFactura: noop, actualizarGrillaProductos: noop,
    registrarUltimoCambioProducto: noop, actualizarEstadoBotonUltimoDetalle: noop,
    setTimeout: noop, clearTimeout: noop,
    clienteActualFactura: { id: 'CLIENTE-ACTUAL' }, posicionamientoTimer: null
});
vm.runInContext(`let productosFactura = [], origenCargaActual = 'directo', modoBloqueoGrilla = '', cajaAcumulaProductos = true;
${extract(source, 'normalizarDatosFacturaEmitida')}
${extract(source, 'aplicarDetalleFacturaEmitida')}
${['normalizarClaveProducto', 'buscarProductoExistente', 'incrementarCantidadProducto', 'agregarProductoAGrilla'].map(n => extract(grilla, n)).join('\n')}`, context);
const run = code => vm.runInContext(code, context);
let checks = 0;
function check(code, expected) { assert.equal(run(code), expected, code); checks++; }
check("normalizarDatosFacturaEmitida('081', '2', '1234').datos.puntoVenta", '0002');
check("normalizarDatosFacturaEmitida('081', '2', '1234').datos.numero", '00001234');
for (const [tipo, pv, nro] of [['', '2', '3'], ['1', '2', '3'], ['081','12345','3'], ['081','2','123456789'], ['081','2a','3'], ['081','2','-3'], ['081','2','3.1']]) {
    context.args = [tipo, pv, nro];
    check('!!normalizarDatosFacturaEmitida(...args).error', true);
}
const product = (id, q, extra = {}) => ({ p_id: id, p_desc: id, cantidad_tot: q, p_pvta: 100, respuesta: 0, ...extra });
for (const acumula of [true, false]) {
    run('productosFactura = []; modoBloqueoGrilla = ""; origenCargaActual = "directo";');
    context.response = { ok: true, acumula, productos: [product('001', 2), product('002', 1), product('001', 3)] };
    check('aplicarDetalleFacturaEmitida(response)', 3);
    check('productosFactura.length', acumula ? 2 : 3);
    check('productosFactura[0].cantidad_tot', acumula ? 5 : 2);
    check('productosFactura.map(p => p.p_id).join(",")', acumula ? '001,002' : '001,002,001');
    check('productosFactura[0].cta_id', 'CLIENTE-ACTUAL');
    check('origenCargaActual', 'directo');
    context.response.productos = [product('003', 0.125, { up_tipo: 'P', p_unidad_pres: 12 }), product('003', 0.375, { up_tipo: 'P', p_unidad_pres: 12 })];
    check('aplicarDetalleFacturaEmitida(response)', 2);
    check('productosFactura.filter(p => p.p_id === "003").length', acumula ? 1 : 2);
    check('productosFactura.filter(p => p.p_id === "003").reduce((s,p) => s+p.cantidad_tot,0)', 0.5);
    check('productosFactura.filter(p => p.p_id === "003").reduce((s,p) => s+p.precioTotal,0)', 50);
}
run('productosFactura = []; cajaAcumulaProductos = true; origenCargaActual = "directo";');
context.p = product('004', 1, { up_tipo: 'P' });
run('agregarProductoAGrilla(p); agregarProductoAGrilla(p);');
check('productosFactura.length', 2); // No altera las excepciones existentes fuera de V.
context.response = { ok: true, productos: [product('005', 1), product('006', 1, { respuesta: 2 }), product('007', 0)] };
check('aplicarDetalleFacturaEmitida(response)', 1);
check('productosFactura.length', 3);
run('modoBloqueoGrilla = "cotizacion"');
check('aplicarDetalleFacturaEmitida(response)', 0);
check('productosFactura.length', 3);
run('modoBloqueoGrilla = ""');
check('aplicarDetalleFacturaEmitida({ok:false, productos:[]})', 0);
check('aplicarDetalleFacturaEmitida({ok:true, productos:[]})', 0);
check('aplicarDetalleFacturaEmitida(null)', 0);
console.log(`OK: ${checks} verificaciones de carga, acumulacion, orden, precios, cantidades y bloqueos.`);
