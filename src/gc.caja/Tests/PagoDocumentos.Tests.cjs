/* Pruebas aisladas de las funciones reales del motor de pagos, sin confirmar operaciones. */
'use strict';
const fs = require('node:fs'), path = require('node:path'), vm = require('node:vm');
const assert = require('node:assert/strict');
const source = fs.readFileSync(path.join(__dirname, '../wwwroot/js/app/pagoFactura.js'), 'utf8');
function extract(name) {
    const start = source.search(new RegExp('^function ' + name + '\\(', 'm'));
    assert(start >= 0, name);
    const end = /^}\r?$/m.exec(source.slice(start));
    return source.slice(start, start + end.index + 1);
}
const names = ['normalizarTexto', 'normalizarTextoUpper', 'esInstrumentoDocumento', 'fechaLocalDocumento',
    'fechaDocumentoValida', 'formatearVencimientoDocumento', 'importeDocumentoCentavos', 'validarDatosDocumento',
    'construirDatosDocumento', 'construirJsonValores', 'saldoPendienteDocumentoCentavos',
    'abrirModalDetalleDocumento', 'guardarDetalleDocumento', 'procesarInstrumentos', 'agregarValorDirecto',
    'abrirModalDetalleSegunTipo', 'validarDiferenciaParaFinalizar', 'volverACalculoFactura',
    'clonarObjetoNC', 'consumirBorradorPago', 'descartarBorradorPago', 'obtenerIdentidadClientePago'];
const elements = new Map();
let appended, updates, hidden;
function $(id) {
    if (!elements.has(id)) elements.set(id, { value: '', handlers: {}, props: {}, classes: new Set() });
    const state = elements.get(id);
    return { length: 1, val(v) { if (v === undefined) return state.value; state.value = v; return this; },
        text(v) { state.value = v; return this; }, prop(k, v) { state.props[k] = v; return this; },
        addClass(c) { state.classes.add(c); return this; }, removeClass(c) { state.classes.delete(c); return this; },
        off() { state.handlers = {}; return this; }, on(evt, cb) { state.handlers[evt] = cb; return this; },
        trigger() { return this; }, modal(action) { if (action === 'hide') { hidden++; state.handlers['hide.bs.modal.documento']?.(); } return this; } };
}
const closed = [];
let clockNow = '2026-09-18T12:00:00-03:00';
class ClockDate extends Date {
    constructor(...args) { super(...(args.length ? args : [clockNow])); }
    static now() { return new Date(clockNow).getTime(); }
}
const ctx = { $, Date: ClockDate,
    estadoNC: { cargando: false, disponibles: [] }, pagoParaRetomar: null,
    obtenerCantidadCreditosNCImputados: () => 0,
    ocultarVentanaPago: async selector => { closed.push(selector); },
    AbrirMensaje: (title, text, callback) => callback('SI'),
    console: { log(){}, warn(){}, error(){} }, window: {},
    revisionPago: 4, contextoDocumento: null, valorIdCounter: 0, conceptosPago: {}, valoresPago: [],
    formatearNumero: n => Number(n).toFixed(2), formatearMoneda: String,
    obtenerTotalNetoCentavos: () => 10000,
    obtenerTotalOtrosValoresCentavos: () => Math.round(ctx.valoresPago.reduce((t, v) => t + v.importe, 0) * 100),
    obtenerTotalCreditosNCCentavos: () => 3000,
    cerrarTecladoPago(){}, vincularEnterTecladoPago(){}, agregarFilaValor: v => appended.push(v),
    actualizarTotalesPago: () => updates++, actualizarTotalInstrumento(){}, mostrarMensajeError: e => { throw Error(e); } };
vm.createContext(ctx);
vm.runInContext(names.map(extract).join('\n'), ctx);
let passed = 0;
function test(name, fn) { ctx.valoresPago = []; elements.clear(); appended = []; updates = 0; hidden = 0; fn(); passed++; console.log('OK ' + name); }
const doc = () => ({ ins_id: 'DOC', ins_desc: 'Documento', tcf_id: 'DO', tcf_desc: 'Documento CtaCte',
    importe: 70, detalle: { fecha_vencimiento: '2026-10-15' } });
test('Fecha local: no adelanta el día al convertir Argentina a UTC', () => {
    process.env.TZ = 'America/Argentina/Buenos_Aires';
    assert.equal(ctx.fechaLocalDocumento(new Date('2026-09-18T23:30:00-03:00')), '2026-09-18');
});
test('Formato de fecha civil: bisiestos, vacías e imposibles', () => {
    for (const date of ['2026-09-18', '2026-10-15', '2028-02-29', '2020-01-01']) assert(ctx.fechaDocumentoValida(date), date);
    for (const date of ['', undefined, '0001-01-01', '2026-02-29', '2026-02-30', '2026-13-01', '2026-04-31', '18/09/2026', '2026-09-18T00:00:00Z']) assert.equal(ctx.fechaDocumentoValida(date), false, date);
});
test('Vencimiento: acepta hoy y mañana; rechaza ayer incluso escrito manualmente', () => {
    assert.equal(ctx.validarDatosDocumento('70', '2026-09-18'), '');
    assert.equal(ctx.validarDatosDocumento('70', '2026-09-19'), '');
    assert.match(ctx.validarDatosDocumento('70', '2026-09-17'), /anterior/);
    ctx.abrirModalDetalleDocumento(doc(), doc());
    $('#txtVencimientoDocumento').val('2026-09-17');
    ctx.guardarDetalleDocumento();
    assert.equal(ctx.valoresPago.length, 0);
    assert.equal(hidden, 0);
    assert.match(elements.get('#errorDetalleDocumento').value, /anterior/);
});
test('Al cambiar de día revalida al guardar y al construir el JSON de un borrador', () => {
    ctx.abrirModalDetalleDocumento(doc(), doc());
    const draft = { ...doc(), detalle: { fecha_vencimiento: '2026-09-18' } };
    ctx.valoresPago = [draft];
    clockNow = '2026-09-19T00:01:00-03:00';
    try {
        assert.throws(() => ctx.construirJsonValores(), /anterior/);
        ctx.valoresPago = [];
        ctx.guardarDetalleDocumento();
        assert.equal(ctx.valoresPago.length, 0);
        assert.equal(elements.get('#txtVencimientoDocumento').props.min, '2026-09-19');
        assert.match(elements.get('#errorDetalleDocumento').value, /anterior/);
    } finally { clockNow = '2026-09-18T12:00:00-03:00'; }
});
test('Montos estrictos: acepta coma/punto y rechaza parseos parciales y exceso de precisión', () => {
    assert.equal(ctx.importeDocumentoCentavos('12,34'), 1234);
    assert.equal(ctx.importeDocumentoCentavos('0.01'), 1);
    for (const amount of ['', '0', '-10', '12abc', '1.234', '1,234.50', 'NaN', 'Infinity', '1e2', '99999999999999999']) assert.equal(ctx.importeDocumentoCentavos(amount), null, amount);
});
test('No permite superar el saldo después de NC y otros medios', () => {
    ctx.valoresPago = [{ importe: 20 }];
    assert.equal(ctx.saldoPendienteDocumentoCentavos(), 5000);
    assert.equal(ctx.validarDatosDocumento('50', '2026-10-15', 5000), '');
    assert(ctx.validarDatosDocumento('50.01', '2026-10-15', 5000));
});
test('Contrato JSON real conserva vencimiento, estado N, cuota 1 y campos vacíos', () => {
    ctx.valoresPago = [doc()];
    const value = ctx.construirJsonValores()[0];
    assert.equal(value.rb_nro_valor, '001');
    assert.equal(value.ins_id, 'DOC'); assert.equal(value.rb_fecha_valor, '2026-10-15');
    assert.equal(value.rb_importe, 70); assert.equal(value.rb_estado, 'N'); assert.equal(value.rb_opcion_cuota, '1');
    for (const k of ['rb_rec', 'rb_aux']) assert.equal(value[k], 0);
    for (const k of ['rb_cupon_manual', 'rb_ch_dif']) assert.equal(value[k], 'N');
    for (const k of ['rb_dato1_valor', 'rb_dato2_valor', 'rb_dato3_valor', 'id_externo']) assert.equal(value[k], '');
});
test('No se sustituye silenciosamente un vencimiento faltante al finalizar', () => {
    const value = doc(); value.detalle = null; ctx.valoresPago = [value];
    assert.throws(() => ctx.construirJsonValores(), /vencimiento/);
});
test('Efectivo y cheque conservan su contrato', () => {
    ctx.valoresPago = [{ ...doc(), ins_id: 'PES', tcf_id: 'EF', detalle: null },
        { ...doc(), ins_id: 'CHE', tcf_id: 'CH', detalle: { fecha_cheque: '2027-01-04', nro_cheque: '123' } }];
    const [cash, cheque] = ctx.construirJsonValores();
    assert.equal(cash.ins_id, 'PES'); assert.equal(cash.rb_estado, 'A'); assert.equal(cash.rb_opcion_cuota, '0');
    assert.equal(cheque.rb_fecha_valor, '2027-01-04'); assert.equal(cheque.rb_dato2_valor, '00000123');
});
test('Abrir propone saldo neto, hoy y limpia el estado anterior', () => {
    ctx.abrirModalDetalleDocumento(doc(), doc());
    assert.equal($('#txtMontoDocumento').val(), '70.00');
    assert.equal($('#txtVencimientoDocumento').val(), ctx.fechaLocalDocumento());
    assert.equal(elements.get('#txtVencimientoDocumento').props.min, ctx.fechaLocalDocumento());
    $('#txtMontoDocumento').val('12'); $('#txtVencimientoDocumento').val('2027-01-01');
    $('#modalDetalleDocumento').modal('hide');
    ctx.abrirModalDetalleDocumento(doc(), doc());
    assert.equal($('#txtMontoDocumento').val(), '70.00');
    assert.equal($('#txtVencimientoDocumento').val(), ctx.fechaLocalDocumento());
});
test('Guardar agrega una sola fila con vencimiento y recalcula; doble clic no duplica', () => {
    ctx.abrirModalDetalleDocumento(doc(), doc()); $('#txtVencimientoDocumento').val('2026-10-15');
    ctx.guardarDetalleDocumento(); ctx.guardarDetalleDocumento();
    assert.equal(ctx.valoresPago.length, 1); assert.equal(appended.length, 1); assert.equal(updates, 1);
    assert.equal(ctx.valoresPago[0].detalle.fecha_vencimiento, '2026-10-15'); assert.equal(hidden, 1);
});
test('Cancelar no agrega valores y una ventana de otra operación no puede guardar', () => {
    ctx.abrirModalDetalleDocumento(doc(), doc()); $('#modalDetalleDocumento').modal('hide'); ctx.guardarDetalleDocumento();
    assert.equal(ctx.valoresPago.length, 0);
    ctx.abrirModalDetalleDocumento(doc(), doc()); ctx.revisionPago++; ctx.guardarDetalleDocumento();
    assert.equal(ctx.valoresPago.length, 0);
});
test('Dato inválido mantiene abierta la ventana y no altera valores', () => {
    ctx.abrirModalDetalleDocumento(doc(), doc()); $('#txtVencimientoDocumento').val(''); ctx.guardarDetalleDocumento();
    assert.equal(ctx.valoresPago.length, 0); assert.equal(hidden, 0);
    assert.match(elements.get('#errorDetalleDocumento').value, /vencimiento/);
});
test('DOC siempre abre su modal, incluso con selección directa o múltiple', () => {
    let opened = 0; const original = ctx.abrirModalDetalleDocumento;
    ctx.abrirModalDetalleDocumento = () => opened++;
    ctx.procesarInstrumentos([doc()], doc()); ctx.agregarValorDirecto(doc(), doc()); ctx.abrirModalDetalleSegunTipo(doc(), doc());
    assert.equal(opened, 3); ctx.abrirModalDetalleDocumento = original;
});
test('DOC combinado con otros pagos tampoco permite vuelto', () => {
    ctx.valoresPago = [doc(), { ins_id: 'CHE', tcf_id: 'CH', importe: 1 }];
    assert.equal(ctx.validarDiferenciaParaFinalizar().permitir, false);
});
(async () => {
    ctx.valoresPago = [doc()]; ctx.window._coTipoActual = 'CD';
    $('#txtClienteIdPago').val('TEST-DOC');
    ctx.volverACalculoFactura();
    await new Promise(setImmediate);
    assert(closed.includes('#modalDetalleDocumento'));
    const resumed = ctx.consumirBorradorPago('CD');
    assert(resumed);
    assert.equal(ctx.construirDatosDocumento(resumed.valores[0]).rb_fecha_valor, '2026-10-15');
    assert.equal(ctx.formatearVencimientoDocumento(resumed.valores[0].detalle.fecha_vencimiento), '15/10/2026');
    assert.equal(ctx.consumirBorradorPago('CD'), null);
    passed++; console.log('OK Volver y retomar conservan vencimiento y consumen el borrador una sola vez');
    console.log(`${passed} pruebas DOC aprobadas.`);
})().catch(error => { console.error(error); process.exitCode = 1; });
