const fs = require('node:fs'), path = require('node:path'), vm = require('node:vm'), assert = require('node:assert/strict');
const source = fs.readFileSync(path.join(__dirname, '../wwwroot/js/app/prodfactcalc.js'), 'utf8');
function extract(name) {
    const start = source.indexOf('function ' + name + '(');
    const end = /^}\r?$/m.exec(source.slice(start));
    assert(start >= 0 && end, name);
    return source.slice(start, start + end.index + 1);
}
const elements = new Map();
function $(selector) {
    if (!elements.has(selector)) elements.set(selector, { html: '', classes: new Set() });
    const state = elements.get(selector);
    return {
        empty() { state.html = ''; return this; },
        append(html) { state.html += html; return this; },
        toggleClass(name, active) { if (active) state.classes.add(name); else state.classes.delete(name); return this; }
    };
}
const ctx = { $, escapeHtml: value => value.replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c])) };
vm.createContext(ctx);
vm.runInContext(extract('cargarSorteosCalculoFactura'), ctx);
const sample = [{so_sorteo:'0060',so_desc:'ARCOR BAGLEY'},{so_sorteo:'0064',so_desc:'prueba 321'}];
let count = 0;
function test(name, run) { run(); count++; console.log('OK ' + name); }
const html = () => elements.get('#tbodySorteos').html;
const visible = () => !elements.get('#panelSorteosCalculo').classes.has('d-none');
test('JSON del tester muestra dos nombres y no altera los datos', () => {
    const before = JSON.stringify(sample); ctx.cargarSorteosCalculoFactura(sample);
    assert.equal((html().match(/<tr>/g) || []).length, 2);
    assert(html().includes('ARCOR BAGLEY')); assert(html().includes('prueba 321'));
    assert(!html().includes('Sorteo sin nombre')); assert(visible());
    assert.equal(JSON.stringify(sample), before);
});
for (const empty of [[], null, undefined, {}, [{}], [null, {}]]) test('Sin datos oculta el panel: ' + JSON.stringify(empty), () => {
    ctx.cargarSorteosCalculoFactura(sample); ctx.cargarSorteosCalculoFactura(empty);
    assert.equal(html(), ''); assert(!visible());
    assert(elements.get('#panelConceptosCalculo').classes.has('col-md-12'));
    assert(!elements.get('#panelConceptosCalculo').classes.has('col-md-8'));
});
test('Un nuevo cálculo reemplaza los sorteos anteriores', () => {
    ctx.cargarSorteosCalculoFactura(sample); ctx.cargarSorteosCalculoFactura([{so_sorteo:'0080',so_desc:'Nuevo'}]);
    assert(!html().includes('ARCOR')); assert(html().includes('Nuevo'));
});
test('Reaparece después de un cálculo sin sorteos', () => {
    ctx.cargarSorteosCalculoFactura([]); ctx.cargarSorteosCalculoFactura(sample);
    assert(visible()); assert(elements.get('#panelConceptosCalculo').classes.has('col-md-8'));
    assert(!elements.get('#panelConceptosCalculo').classes.has('col-md-12'));
});
test('Escapa descripciones y detalles HTML', () => {
    ctx.cargarSorteosCalculoFactura([{so_sorteo:'0001',so_desc:'<img src=x onerror=alert(1)>',detalle:'<script>bad</script>'}]);
    assert(!html().includes('<img')); assert(!html().includes('<script>')); assert(html().includes('&lt;img'));
});
test('Sin descripción identifica el sorteo por su código completo', () => {
    ctx.cargarSorteosCalculoFactura([{so_sorteo:'0060',so_desc:''}]); assert(html().includes('Sorteo 0060'));
});
test('Tolera filas vacías junto con sorteos válidos', () => {
    ctx.cargarSorteosCalculoFactura([null, {}, ...sample]); assert.equal((html().match(/<tr>/g)||[]).length, 2);
});
console.log(count + ' pruebas de sorteos en pantalla aprobadas.');
