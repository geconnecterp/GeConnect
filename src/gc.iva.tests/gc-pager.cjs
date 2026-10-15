// Contrato del componente real, sin navegador ni SQL: node --test gc.iva.tests/gc-pager.cjs
const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const source = fs.readFileSync(path.join(__dirname, '../gc.sitio/wwwroot/js/components/gc-pager.js'), 'utf8');
function fixture() {
    const handlers = {}, events = [];
    const pager = { dataset: { current: '1', pages: '307' }, busy: false,
        closest: () => pager.busy ? {} : null,
        querySelector: () => input, dispatchEvent: e => events.push(e) };
    const input = { value: '1', disabled: false, attrs: {},
        matches: () => true, closest: () => pager,
        setCustomValidity(text) { this.error = text; }, reportValidity() { return !this.error; },
        setAttribute(k, v) { this.attrs[k] = v; }, removeAttribute(k) { delete this.attrs[k]; } };
    const button = { disabled: false, dataset: { page: '70' }, go: false,
        hasAttribute(k) { return k === 'data-gc-page-go' ? this.go : !this.go; },
        closest: selector => selector === '.gc-pager button' ? button : pager };
    vm.runInNewContext(source, { document: { addEventListener: (name, fn) => handlers[name] = fn },
        CustomEvent: class { constructor(type, options) { this.type = type; Object.assign(this, options); } } });
    return { pager, input, button, events,
        click() { handlers.click({ target: button }); },
        jump(value, enter = false) { input.value = value; button.go = true;
            if (enter) handlers.keydown({ key: 'Enter', target: input, preventDefault() {} }); else this.click(); },
        edit() { handlers.input({ target: input }); } };
}
test('Número emite evento local sin cambiar página confirmada', () => {
    const f = fixture(); f.click();
    assert.equal(f.events[0].type, 'gc:page'); assert.equal(f.events[0].detail.page, 70);
    assert.equal(f.events[0].bubbles, true); assert.equal(f.pager.dataset.current, '1');
});
test('Ir y Enter permiten extremos e intermedias', () => {
    const f = fixture(); f.jump('307'); f.jump('70', true);
    assert.deepEqual(f.events.map(e => e.detail.page), [307, 70]);
});
test('Rechaza vacío, negativos, cero, fuera de rango, decimales y notación exponencial', () => {
    for (const value of ['', '-1', '0', '308', '2.5', '1e2', 'NaN', 'Infinity', '9007199254740992']) {
        const f = fixture(); f.jump(value);
        assert.equal(f.events.length, 0, value); assert.equal(f.input.attrs['aria-invalid'], 'true', value);
        f.edit(); assert.equal(f.input.error, ''); assert(!('aria-invalid' in f.input.attrs));
    }
});
test('No navega por blur ni a la página actual', () => {
    const f = fixture(); f.jump('1'); assert.equal(f.events.length, 0);
});
test('Bloqueo por carga, botón o input deshabilitado', () => {
    const f = fixture(); f.pager.busy = true; f.click(); f.jump('70', true);
    f.pager.busy = false; f.button.disabled = true; f.click();
    f.input.disabled = true; f.jump('70', true);
    assert.equal(f.events.length, 0);
});
test('Botones manipulados fuera de rango no emiten evento', () => {
    const f = fixture();
    for (const page of ['0', '308', '1.5', 'NaN', '1']) { f.button.dataset.page = page; f.click(); }
    assert.equal(f.events.length, 0);
});
