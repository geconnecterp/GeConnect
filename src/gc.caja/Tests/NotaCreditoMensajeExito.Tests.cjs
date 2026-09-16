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
const messages = [];
const context = vm.createContext({
    mostrarMensaje: (...args) => messages.push(args),
    window: { ncDevolucionIndexUrl: '/nc', ncDevolucionMenuCajaUrl: '/', location: { href: '/actual' } }
});
vm.runInContext(['escaparHtml', 'renderizarMensajeEmisionExitosa', 'procesarFinalizacionExitosa'].map(extract).join('\n'), context);

const response = {
    ok: true, debe_imprimir: false,
    mensaje: 'Emitida. Aviso de stock: <pendiente>',
    data: [{ tco_letra: 'B', cm_compte: '0002-00000014', cm_repetido: 0 }]
};
context.procesarFinalizacionExitosa(response);
const [title, html, type, accept] = messages.pop();
assert.equal(type, 'succ!');
assert.match(title, /emitida/);
assert.match(html, /alert alert-success/);
assert.match(html, /bx-check-circle text-golden/);
assert.match(html, /badge bg-primary fs-6">B</);
assert.match(html, /0002-00000014/);
assert.match(html, /Aviso de stock: &lt;pendiente&gt;/);
assert.doesNotMatch(html, /Comprobante Repetido/);
assert.equal(context.window.location.href, '/actual');
accept();
assert.equal(context.window.location.href, '/', 'Vuelve al menu y no al modulo NC');

const fallback = context.renderizarMensajeEmisionExitosa({}, null);
assert.match(fallback, /Sin n\u00famero/);
assert.doesNotMatch(fallback, /undefined|null|badge bg-primary/);
for (const cm_repetido of [1, '1']) {
    const result = context.renderizarMensajeEmisionExitosa({}, {
        tco_letra: '<img>', cm_compte: '<script>', cm_repetido
    });
    assert.match(result, /&lt;img&gt;/);
    assert.match(result, /&lt;script&gt;/);
    assert.match(result, /Comprobante Repetido/);
}
context.procesarFinalizacionExitosa({ debe_imprimir: true, mensaje: 'Emitida', data: [] });
const noData = messages.pop();
assert.equal(noData[2], 'warn!', 'Conserva la advertencia por falta de datos de reporte');
context.window.location.href = '/actual';
noData[3]();
assert.equal(context.window.location.href, '/');

context.window.ncDevolucionMenuCajaUrl = '/Caja/Home';
context.window.location.href = '/actual';
context.procesarFinalizacionExitosa({ ...response, debe_imprimir: true });
const noReports = messages.pop();
assert.equal(noReports[2], 'warn!');
assert.match(noReports[1], /&lt;pendiente&gt;/);
assert.equal(context.window.location.href, '/actual', 'Espera que se acepte el mensaje');
noReports[3]();
assert.equal(context.window.location.href, '/Caja/Home');

async function testReports() {
    for (const fails of [false, true]) {
        let reportRequest;
        context.ModuloReportes = {
            generarYVisualizarReporte(comprobante, options) {
                reportRequest = { comprobante, options };
                return fails ? Promise.reject(new Error('Reporte')) : Promise.resolve();
            }
        };
        context.window.location.href = '/actual';
        context.procesarFinalizacionExitosa({ ...response, debe_imprimir: true });
        await new Promise(resolve => setImmediate(resolve));
        assert.equal(reportRequest.comprobante.cm_compte, '0002-00000014');
        assert.equal(context.window.location.href, '/actual');
        const success = messages.pop();
        assert.equal(success[2], 'succ!');
        success[3]();
        assert.equal(context.window.location.href, '/Caja/Home');
    }
    delete context.window.ncDevolucionMenuCajaUrl;
    context.procesarFinalizacionExitosa(response);
    messages.pop()[3]();
    assert.equal(context.window.location.href, '/', 'Fallback al menu, no a una NC ya emitida');

    let callbacks;
    const failureContext = vm.createContext({
        window: { ncDevolucionFinalizarUrl: '/finalizar', location: { href: '/operacion' } },
        $: { ajax() {
            callbacks = {};
            const request = {
                done(fn) { callbacks.done = fn; return request; },
                fail(fn) { callbacks.fail = fn; return request; },
                always(fn) { callbacks.always = fn; return request; }
            };
            return request;
        } },
        finalizacionEnCurso: false,
        productosActuales: [{ p_id: '001', cantidad_tot: 2 }],
        calculoActual: { total: 100 },
        bloquearAccionFinalizar() {}, bloquearAccionSeguir() {}, bloquearEntradaManual() {},
        logInfo() {}, logError() {}, mostrarMensaje: (...args) => messages.push(args),
        procesarFinalizacionExitosa() { assert.fail('Un rechazo no debe tratarse como emision'); }
    });
    vm.runInContext(extract('ejecutarFinalizar'), failureContext);
    failureContext.ejecutarFinalizar();
    callbacks.done({ ok: false, mensaje: '(0) Error en los datos consulta Ult. Comprobante' });
    callbacks.always();
    assert.equal(messages.pop()[2], 'warn!');
    assert.equal(failureContext.window.location.href, '/operacion');
    assert.equal(failureContext.productosActuales.length, 1);
    assert.equal(failureContext.calculoActual.total, 100);
    assert.equal(failureContext.finalizacionEnCurso, false);
    failureContext.ejecutarFinalizar();
    callbacks.fail({ status: 500 }, 'error', 'Error de red');
    callbacks.always();
    assert.equal(messages.pop()[2], 'error!');
    assert.equal(failureContext.window.location.href, '/operacion');
    assert.equal(failureContext.productosActuales.length, 1);
    console.log('PASS: mensaje NC, avisos, reportes y retorno al menu despues de aceptar.');
}
testReports().catch(error => { console.error(error); process.exitCode = 1; });
