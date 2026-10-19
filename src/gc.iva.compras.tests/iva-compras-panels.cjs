// Pruebas del JS real en un DOM mínimo simulado, sin navegador, servicios ni SQL.
// Ejecutar: node --test gc.iva.compras.tests/iva-compras-panels.cjs
const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const source = fs.readFileSync(path.join(__dirname, '../gc.sitio/wwwroot/js/app/libros/iva-compras-detalle.js'), 'utf8');
const pagerSource = fs.readFileSync(path.join(__dirname, '../gc.sitio/wwwroot/js/components/gc-pager.js'), 'utf8');
const flush = async () => { for (let i = 0; i < 6; i++) await new Promise(setImmediate); };

function fixture(unavailable = false) {
    const nodes = {}, pending = [], calls = [], timers = [], downloads = [], documentHandlers = {};
    function element(id) {
        return nodes[id] = {
            disabled: false, hidden: false, checked: false, value: '', dataset: {}, attrs: {}, events: {},
            innerHTML: '', textContent: '', children: [], style: {},
            getBoundingClientRect() { return { top: 170, height: 60 }; },
            querySelector(selector) {
                if (selector === '[data-gc-pager]') return this.innerHTML.includes('data-total="0"') || !this.innerHTML.includes('data-total=')
                    ? null : { dataset: { current: this.page || '1', pages: '6' } };
                if (selector === '.iva-datos') {
                    const match = this.innerHTML.match(/data-total="(\d+)"/);
                    return match ? { dataset: { total: match[1] } } : null;
                }
                return null;
            },
            addEventListener(name, handler) { (this.events[name] ??= []).push(handler); },
            emit(name, event = {}) {
                event.target ??= this;
                event.preventDefault = () => { event.defaultPrevented = true; };
                for (const handler of this.events[name] || []) handler(event);
                return event;
            },
            setAttribute(k, v) { this.attrs[k] = v; }, removeAttribute(k) { delete this.attrs[k]; },
            replaceChildren() { this.innerHTML = ''; this.children = []; },
            appendChild(el) { this.children.push(el); },
            querySelectorAll() { return []; },
            setCustomValidity(text) { this.validationMessage = text; },
            focus() { this.focused = true; },
            click() { downloads.push(this.download); }, remove() {}
        };
    }
    const root = element('iva-compras'), form = element('iva-filtros'), outer = element('outer');
    for (const id of ['iva-resultado', 'iva-mensaje', 'divFiltro', 'divDetalle', 'btnFiltro', 'btnDetalle', 'iva-exportar', 'iva-cargando', 'iva-cargando-texto', 'footer']) element(id);
    root.dataset = { consulta: '/consulta', excel: '/excel' };
    outer.disabled = unavailable;
    form.querySelector = () => outer;
    form.elements = {};
    for (const name of ['Periodo', 'Mes', 'Registros', 'Desde', 'Hasta', 'Fechas', 'PorCuit', 'Cuit', 'PorTipoProveedor', 'TiposProveedor', 'PorComprobante', 'Comprobantes']) {
        form.elements[name] = element(name); form.elements[name].name = name;
    }
    const defaults = { Periodo: '2026', Mes: '10', Registros: '100' };
    Object.entries(defaults).forEach(([k, v]) => form.elements[k].value = v);
    const toggles = [['Fechas', 'iva-fechas'], ['PorCuit', 'iva-cuit'], ['PorTipoProveedor', 'iva-proveedores'], ['PorComprobante', 'iva-comprobantes']];
    toggles.forEach(([key, id]) => { form.elements[key].dataset.enable = id; element(id); });
    root.querySelectorAll = () => toggles.map(([key]) => form.elements[key]);
    form.valid = true;
    form.checkValidity = () => form.valid && !form.elements.Hasta.validationMessage;
    form.reportValidity = () => { form.reportedWithFilterVisible = !nodes.divFiltro.hidden; return form.checkValidity(); };
    class Data {
        constructor(f) {
            this.entries = [];
            if (f) Object.entries(f.elements).forEach(([k, el]) => this.append(k, el.value));
        }
        append(k, v) { this.entries.push([k, v]); }
        set(k, v) { this.entries = this.entries.filter(([key]) => key !== k); this.append(k, v); }
        get(k) { return this.entries.find(([key]) => key === k)?.[1]; }
        forEach(fn) { this.entries.forEach(([k, v]) => fn(v, k)); }
    }
    const context = {
        document: { getElementById: id => nodes[id], querySelector: () => nodes.footer, createElement: tag => element('created-' + tag), body: element('body'), addEventListener: (name, fn) => documentHandlers[name] = fn },
        CustomEvent: class { constructor(type, options) { this.type = type; Object.assign(this, options); } },
        window: { innerHeight: 855, scrollY: 0, requestAnimationFrame: fn => fn(), addEventListener() {} },
        FormData: Data, URL: { createObjectURL: () => 'blob:test', revokeObjectURL() {} },
        setTimeout: (fn, ms) => { if (ms < 60000) timers.push(fn); },
        fetch: (url, request) => new Promise(resolve => { calls.push({ url, data: request.body }); pending.push(resolve); })
    };
    vm.runInNewContext(source, context);
    vm.runInNewContext(pagerSource, context);
    return {
        nodes, form, outer, calls, downloads,
        click(id) { nodes[id].emit('click'); },
        submit() { form.emit('submit'); },
        page(number) {
            const result = nodes['iva-resultado'];
            const pager = { dataset: { current: result.page || '1', pages: '6' },
                closest: () => result.attrs['aria-busy'] === 'true' ? result : null,
                dispatchEvent: e => result.emit(e.type, e) };
            const button = { dataset: { page: String(number) }, disabled: false,
                closest: selector => selector === '.gc-pager button' ? button : pager,
                hasAttribute: name => name === 'data-page' };
            documentHandlers.click({ target: button });
        },
        async answer({ ok = true, html = '<section data-total="501">Resultados</section>', excel = false } = {}) {
            assert(pending.length, 'Debe haber una solicitud pendiente');
            pending.shift()({ ok, status: ok ? 200 : 503, redirected: false,
                headers: { get: () => excel ? 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' : 'text/html' },
                text: async () => html, json: async () => ({ mensaje: 'Fallo controlado' }), blob: async () => ({}) });
            await flush();
        },
        reset() {
            const event = form.emit('reset');
            if (!event.defaultPrevented) {
                Object.entries(form.elements).forEach(([k, el]) => { el.value = defaults[k] || ''; el.checked = false; });
                timers.splice(0).forEach(fn => fn());
            }
        }
    };
}

test('Inicial: filtro abierto, detalle cerrado y deshabilitado', () => {
    const { nodes } = fixture();
    assert.equal(nodes.divFiltro.hidden, false);
    assert.equal(nodes.divDetalle.hidden, true);
    assert.equal(nodes.btnDetalle.disabled, true);
    assert.equal(nodes.btnFiltro.attrs['aria-expanded'], 'true');
    assert.equal(nodes['iva-exportar'].disabled, true);
    assert.equal(nodes['iva-cargando'].hidden, true);
    assert.equal(nodes['iva-filtros'].hidden, false);
});
test('Consultar oculta filtro, presenta resultados y permite cerrar', async () => {
    const f = fixture(); f.submit();
    assert(f.outer.disabled && f.nodes.btnFiltro.disabled && f.nodes.btnDetalle.disabled);
    assert(f.nodes['iva-filtros'].hidden && !f.nodes['iva-cargando'].hidden && !f.nodes.divDetalle.hidden);
    assert.match(f.nodes['iva-cargando-texto'].textContent, /puede demorar/);
    f.click('btnFiltro'); f.click('btnDetalle'); f.reset(); f.submit(); f.click('iva-exportar');
    assert.equal(f.calls.length, 1);
    await f.answer();
    assert(f.nodes.divFiltro.hidden && !f.nodes.divDetalle.hidden);
    assert.equal(f.nodes.btnDetalle.disabled, false);
    assert.equal(f.nodes.btnDetalle.attrs['aria-expanded'], 'true');
    assert.equal(f.outer.disabled, false);
    assert.equal(f.nodes['iva-exportar'].disabled, false);
    assert.equal(f.nodes['iva-cargando'].hidden, true);
    assert.equal(f.nodes.divDetalle.style.height, '609px');
});
test('Filtro alterna paneles sin perder la consulta ni volver a consultar', async () => {
    const f = fixture(); f.submit(); await f.answer();
    f.click('btnFiltro');
    assert(!f.nodes.divFiltro.hidden && f.nodes.divDetalle.hidden);
    assert.equal(f.nodes.btnDetalle.disabled, false);
    f.click('btnFiltro');
    assert(f.nodes.divFiltro.hidden && !f.nodes.divDetalle.hidden);
    assert.match(f.nodes['iva-resultado'].innerHTML, /Resultados/);
    assert.equal(f.calls.length, 1);
});
test('Detalle descarta resultados y paginación conservando TODOS los criterios', async () => {
    const f = fixture();
    f.form.elements.Mes.value = '9'; f.form.elements.Registros.value = '500';
    f.form.elements.PorCuit.checked = true; f.form.elements.Cuit.value = '20%';
    f.form.elements.TiposProveedor.value = '01';
    const before = JSON.stringify(Object.values(f.form.elements).map(e => [e.value, e.checked]));
    f.submit(); await f.answer(); f.click('btnDetalle');
    assert.equal(JSON.stringify(Object.values(f.form.elements).map(e => [e.value, e.checked])), before);
    assert.equal(f.nodes['iva-resultado'].innerHTML, '');
    assert(!f.nodes.divFiltro.hidden && f.nodes.divDetalle.hidden && f.nodes.btnDetalle.disabled);
    assert(f.nodes['iva-exportar'].disabled && !f.nodes['iva-filtros'].hidden);
    f.submit(); await f.answer(); assert.equal(f.nodes.btnDetalle.disabled, false);
});
test('Inicializar descarta resultados y restablece período, tamaño y filtros opcionales', async () => {
    const f = fixture(); f.form.elements.Mes.value = '9'; f.form.elements.PorCuit.checked = true;
    f.submit(); await f.answer(); f.reset();
    assert.equal(f.form.elements.Mes.value, '10'); assert.equal(f.form.elements.Registros.value, '100');
    assert.equal(f.form.elements.PorCuit.checked, false); assert.equal(f.nodes['iva-cuit'].disabled, true);
    assert.equal(f.form.elements.Desde.value, '2026-10-01'); assert.equal(f.form.elements.Hasta.value, '2026-10-31');
    assert(f.nodes.btnDetalle.disabled && !f.nodes.divFiltro.hidden && f.nodes.divDetalle.hidden);
    assert(f.nodes['iva-exportar'].disabled);
});
test('Exportar con filtro cerrado conserva consulta y bloquea navegación hasta terminar', async () => {
    const f = fixture(); f.submit(); await f.answer(); f.click('iva-exportar');
    assert(f.nodes.divFiltro.hidden && f.outer.disabled && f.nodes.btnFiltro.disabled && f.nodes.btnDetalle.disabled);
    assert(f.nodes['iva-exportar'].disabled && !f.nodes['iva-cargando'].hidden);
    assert.match(f.nodes['iva-cargando-texto'].textContent, /todos los registros/);
    f.click('btnDetalle'); f.click('btnFiltro'); f.reset();
    assert(f.nodes.divFiltro.hidden);
    await f.answer({ excel: true });
    assert.equal(f.calls[1].url, '/excel'); assert.equal(f.calls[1].data.get('Pagina'), '1');
    assert.equal(f.downloads[0], 'Libro_IVA_Compras_2026-10.xlsx');
    assert(f.nodes.divFiltro.hidden && !f.nodes.divDetalle.hidden && !f.nodes.btnDetalle.disabled);
    assert.equal(f.outer.disabled, false);
});
test('Paginación conserva los filtros aplicados', async () => {
    const f = fixture(); f.form.elements.Cuit.value = '20%'; f.submit(); await f.answer();
    f.nodes['iva-resultado'].emit('gc:page', { detail: { page: 2 } });
    assert.equal(f.calls[1].data.get('Pagina'), '2'); assert.equal(f.calls[1].data.get('Cuit'), '20%');
    await f.answer(); assert(!f.nodes.divDetalle.hidden);
});
test('Sin registros sigue siendo consulta válida y se puede cerrar', async () => {
    const f = fixture(); f.submit(); await f.answer({ html: '<p>No se encontraron comprobantes</p>' });
    assert.equal(f.nodes.btnDetalle.disabled, false); assert.equal(f.nodes.divDetalle.hidden, false);
    assert(f.nodes['iva-exportar'].disabled);
    f.click('btnDetalle'); assert(f.nodes.divDetalle.hidden);
});
test('Error de consulta vuelve al filtro sin dejar datos anteriores', async () => {
    const f = fixture(); f.submit(); await f.answer(); f.submit(); await f.answer({ ok: false });
    assert(!f.nodes.divFiltro.hidden && f.nodes.divDetalle.hidden && f.nodes.btnDetalle.disabled);
    assert.equal(f.nodes['iva-resultado'].innerHTML, ''); assert.equal(f.nodes['iva-mensaje'].textContent, 'Fallo controlado');
    assert(f.nodes['iva-exportar'].disabled && f.nodes['iva-cargando'].hidden);
});
test('Error de exportación no descarta consulta ni filtros', async () => {
    const f = fixture(); f.submit(); await f.answer(); f.click('iva-exportar'); await f.answer({ ok: false });
    assert(f.nodes.divFiltro.hidden && !f.nodes.divDetalle.hidden && !f.nodes.btnDetalle.disabled);
    assert.match(f.nodes['iva-resultado'].innerHTML, /Resultados/);
});
test('Validación de filtros ocultos los abre antes de mostrar el error', () => {
    const f = fixture(); f.click('btnFiltro'); f.form.valid = false; f.submit();
    assert.equal(f.form.reportedWithFilterVisible, true); assert.equal(f.calls.length, 0);
});
test('Editar criterios invalida la consulta previa', async () => {
    const f = fixture(); f.submit(); await f.answer(); f.click('btnFiltro'); f.form.emit('input');
    assert(f.nodes.btnDetalle.disabled && f.nodes.divDetalle.hidden);
    assert.equal(f.nodes['iva-resultado'].innerHTML, '');
});
test('Fallo inicial de catálogos mantiene controles bloqueados', () => {
    const f = fixture(true); f.click('btnFiltro'); f.submit(); f.click('iva-exportar');
    assert(f.nodes.btnFiltro.disabled && f.nodes.btnDetalle.disabled && f.outer.disabled);
    assert.equal(f.calls.length, 0);
});
test('Razor: Exportar está en navegación, Inicializar y Ver dentro del formulario ocultable', () => {
    const view = fs.readFileSync(path.join(__dirname, '../gc.sitio/Areas/Libros/Views/IvaComprasDetalle/_Filtro.cshtml'), 'utf8');
    const index = fs.readFileSync(path.join(__dirname, '../gc.sitio/Areas/Libros/Views/IvaComprasDetalle/Index.cshtml'), 'utf8');
    assert.doesNotMatch(view, /id="iva-exportar"/);
    assert.match(index, /<nav[\s\S]*id="iva-exportar"[\s\S]*<\/nav>/);
    assert.match(view, /<form[\s\S]*type="reset"[\s\S]*type="submit"[\s\S]*<\/form>/);
    assert.match(view, /id="iva-filtros" novalidate/);
});

test('Sin consulta no se puede exportar ni invocando directamente el evento', () => {
    const f = fixture(); f.click('iva-exportar');
    assert.equal(f.calls.length, 0);
});
test('Consulta vacía explícita deshabilita Exportar', async () => {
    const f = fixture(); f.submit(); await f.answer({ html: '<section class="iva-datos" data-total="0">Sin datos</section>' });
    f.click('iva-exportar'); assert.equal(f.calls.length, 1);
    assert(f.nodes['iva-exportar'].disabled);
});
test('Exportar conserva instantánea y multiselección, no los valores actuales del DOM', async () => {
    const f = fixture(); f.form.elements.Mes.value = '11'; f.form.elements.TiposProveedor.value = '01';
    f.submit(); await f.answer();
    f.calls[0].data.append('TiposProveedor', '02');
    // Simula cambio externo sin evento; nunca debe contaminar la consulta exportada.
    f.form.elements.Mes.value = '12'; f.click('iva-exportar');
    assert.equal(f.calls[1].data.get('Mes'), '11');
    assert.deepEqual(f.calls[1].data.entries.filter(([k]) => k === 'TiposProveedor'), [['TiposProveedor', '01'], ['TiposProveedor', '02']]);
    await f.answer({ excel: true });
});
test('Cambio de página conserva la grilla bajo el spinner y bloquea peticiones duplicadas', async () => {
    const f = fixture(); f.submit(); await f.answer();
    const event = { detail: { page: 2 } };
    f.nodes['iva-resultado'].emit('gc:page', event);
    assert.match(f.nodes['iva-resultado'].innerHTML, /Resultados/);
    assert(!f.nodes.divDetalle.hidden && !f.nodes['iva-cargando'].hidden);
    assert.equal(f.nodes['iva-resultado'].attrs['aria-busy'], 'true');
    f.nodes['iva-resultado'].emit('gc:page', event); f.click('iva-exportar');
    assert.equal(f.calls.length, 2);
    await f.answer({ ok: false });
    assert(f.nodes['iva-cargando'].hidden && f.nodes['iva-exportar'].disabled);
    assert.equal(f.nodes['iva-resultado'].innerHTML, '');
});
test('Abrir filtro muestra acciones; volver al detalle conserva Exportar habilitado', async () => {
    const f = fixture(); f.submit(); await f.answer(); f.click('btnFiltro');
    assert(!f.nodes['iva-filtros'].hidden && !f.nodes['iva-exportar'].disabled);
    f.click('btnFiltro'); assert(f.nodes['iva-filtros'].hidden && !f.nodes['iva-exportar'].disabled);
});
test('Evento change sin input también invalida la consulta exportable', async () => {
    const f = fixture(); f.submit(); await f.answer();
    f.form.emit('change', { target: f.form.elements.Registros });
    assert(f.nodes['iva-exportar'].disabled && !f.nodes['iva-filtros'].hidden);
});

test('Volver a primera página conserva la instantánea y no toma filtros externos', async () => {
    const f = fixture(); f.form.elements.Mes.value = '11'; f.submit(); await f.answer();
    f.nodes['iva-resultado'].page = '4'; f.form.elements.Mes.value = '12';
    f.nodes['iva-resultado'].emit('gc:page', { detail: { page: 1 } });
    assert.equal(f.calls[1].data.get('Mes'), '11');
    assert.equal(f.calls[1].data.get('Pagina'), '1');
    await f.answer();
});

test('Páginas inválidas, actual o eventos sin consulta no disparan solicitudes', async () => {
    const f = fixture();
    f.nodes['iva-resultado'].emit('gc:page', { detail: { page: 2 } });
    assert.equal(f.calls.length, 0);
    f.submit(); await f.answer();
    for (const page of [0, -1, 1, 7, 1.5, NaN, Infinity, '2'])
        f.nodes['iva-resultado'].emit('gc:page', { detail: { page } });
    assert.equal(f.calls.length, 1);
});

test('Exportar después de paginar sigue solicitando página 1 y mantiene CUIT/período', async () => {
    const f = fixture(); f.form.elements.Mes.value = '11'; f.form.elements.Cuit.value = '20139511221';
    f.submit(); await f.answer();
    f.nodes['iva-resultado'].emit('gc:page', { detail: { page: 4 } }); await f.answer();
    f.click('iva-exportar');
    assert.equal(f.calls[2].url, '/excel');
    assert.equal(f.calls[2].data.get('Pagina'), '1');
    assert.equal(f.calls[2].data.get('Mes'), '11');
    assert.equal(f.calls[2].data.get('Cuit'), '20139511221');
    await f.answer({ excel: true });
});

test('Integración del componente con IVA: un clic, una solicitud, bloqueo durante espera', async () => {
    const f = fixture(); f.form.elements.Cuit.value = '20139511221'; f.submit(); await f.answer();
    f.page(4); f.page(5);
    assert.equal(f.calls.length, 2);
    assert.equal(f.calls[1].data.get('Pagina'), '4');
    assert.equal(f.calls[1].data.get('Cuit'), '20139511221');
    assert(!f.nodes['iva-cargando'].hidden);
    await f.answer(); assert(f.nodes['iva-cargando'].hidden);
});
