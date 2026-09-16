const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const source = fs.readFileSync(path.join(__dirname, '../wwwroot/js/app/ncDevolucionProductos.js'), 'utf8');
const extract = name => {
    const match = source.match(new RegExp(`^    function ${name}\\([^]*?^    }`, 'm'));
    assert.ok(match, name);
    return match[0];
};

function setup() {
    const events = new Map();
    const visible = new Set(['productos']);
    const props = new Map();
    const requests = [];
    const messages = [];
    let callbacks;
    let blocked = false;
    let renders = 0;
    const selectors = { modal: 'productos', modalResumenFinal: 'final', btnFinalizar: 'emitir',
        btnSeguir: 'seguir', conceptosCalculo: 'conceptos' };
    const $ = selector => {
        const api = {
            hasClass: () => visible.has(selector),
            one(event, fn) { events.set(selector, fn); return api; },
            prop(name, value) { props.set(`${selector}.${name}`, value); return api; },
            empty() { return api; }
        };
        return api;
    };
    $.ajax = options => {
        requests.push(options);
        callbacks = {};
        const request = {
            done(fn) { callbacks.done = fn; return request; },
            fail(fn) { callbacks.fail = fn; return request; },
            always(fn) { callbacks.always = fn; return request; }
        };
        return request;
    };
    const modal = name => ({
        show() { visible.add(name); },
        hide() { visible.delete(name); },
    });
    const context = vm.createContext({ $, SELECTORES: selectors,
        window: { ncDevolucionSeguirUrl: '/calcular' },
        modalProductos: modal('productos'), modalResumenFinal: modal('final'),
        calculoActual: null, finalizacionEnCurso: false, calculoEnCurso: false,
        productosActuales: [{ p_id: '001', cantidad_tot: 2 }],
        bloquearEntradaManual: value => { blocked = value; },
        bloquearAccionSeguir: value => props.set('seguir.disabled', value),
        renderizarResumenFinal: () => { renders++; },
        obtenerSubtotalesDesdeRespuesta: response => response.subtotales || [],
        mostrarMensaje: (...args) => messages.push(args), logInfo() {}, logError() {}, logAdvertencia() {}
    });
    vm.runInContext(['abrirResumenFinal', 'volverACargaProductos', 'limpiarCalculoActual',
        'actualizarAccionesProductos', 'ejecutarSeguir'].map(extract).join('\n'), context);
    return { context, visible, props, requests, messages, events, get blocked() { return blocked; },
        get renders() { return renders; },
        respond(response) { callbacks.done(response); callbacks.always(); },
        hidden(name) { const fn = events.get(name); events.delete(name); fn(); }
    };
}

for (const tipo of ['AA', 'DV']) {
    const t = setup();
    const products = JSON.stringify(t.context.productosActuales);
    vm.runInContext('ejecutarSeguir({})', t.context);
    t.respond({ ok: true, co_tipo: tipo, subtotales: [{ concepto: 'Neto', importe: 100 }] });
    assert.equal(t.requests.length, 1);
    assert.equal(t.requests[0].url, '/calcular');
    assert.equal(t.renders, 1);
    assert.equal(t.visible.size, 0, 'Espera el cierre de productos antes de mostrar final');
    t.hidden('productos');
    assert.deepEqual([...t.visible], ['final']);
    assert.equal(t.props.get('emitir.disabled'), false);
    vm.runInContext('volverACargaProductos()', t.context);
    assert.equal(t.context.calculoActual, null);
    assert.equal(t.props.get('emitir.disabled'), true);
    assert.equal(t.blocked, false);
    t.hidden('final');
    assert.deepEqual([...t.visible], ['productos']);
    assert.equal(JSON.stringify(t.context.productosActuales), products);
    assert.equal(t.props.get('seguir.disabled'), false);
}
for (const response of [{ ok: false, mensaje: 'Error SP' }, { ok: true, subtotales: [] }]) {
    const t = setup();
    vm.runInContext('ejecutarSeguir({})', t.context);
    t.respond(response);
    assert.deepEqual([...t.visible], ['productos']);
    assert.equal(t.context.calculoActual, null);
    assert.equal(t.renders, 0);
    assert.equal(t.messages.length, 1);
    assert.equal(t.blocked, false);
}
const missing = setup();
missing.context.modalResumenFinal = null;
vm.runInContext('ejecutarSeguir({})', missing.context);
missing.respond({ ok: true, subtotales: [{ concepto: 'Neto', importe: 100 }] });
assert.equal(missing.context.calculoActual, null);
assert.equal(missing.blocked, false);
assert.deepEqual([...missing.visible], ['productos']);
const busy = setup();
busy.context.finalizacionEnCurso = true;
busy.context.calculoActual = { co_tipo: 'AA' };
vm.runInContext('volverACargaProductos()', busy.context);
assert.ok(busy.context.calculoActual);
assert.equal(busy.events.size, 0);
assert.ok(!source.includes('modalNcDevolucionCalculo'));
const productsView = fs.readFileSync(path.join(__dirname, '../Areas/Facturacion/Views/NotaCredito/_ProductoDevolucion.cshtml'), 'utf8');
const finalView = fs.readFileSync(path.join(__dirname, '../Areas/Facturacion/Views/NotaCredito/_ResumenConfirmacion.cshtml'), 'utf8');
assert.ok(!productsView.includes('modalNcDevolucionCalculo'));
assert.equal((finalView.match(/id="tbodyNcDevolucionConceptosCalculo"/g) || []).length, 1);
console.log('OK: salto directo AA/DV, regreso con recalculo, errores, bloqueo durante emision y desglose unico.');
