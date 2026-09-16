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
const elements = new Map();
const timers = new Map();
let nextId = 0;
const $ = selector => {
    if (!elements.has(selector)) elements.set(selector, { classes: new Set(), content: '' });
    const el = elements.get(selector);
    const api = {
        addClass(value) { value.split(' ').forEach(c => el.classes.add(c)); return api; },
        removeClass(value) { value.split(' ').forEach(c => el.classes.delete(c)); return api; },
        html(value) { el.content = value; return api; }
    };
    return api;
};
const context = vm.createContext({
    $, SELECTORES: { estadoEntradaManual: '#entrada' },
    setTimeout(fn, ms) { assert.equal(ms, 10000); timers.set(++nextId, fn); return nextId; },
    clearTimeout(id) { timers.delete(id); }
});
vm.runInContext('const temporizadoresMensajes = new Map();\n' + [
    'escaparHtml', 'crearCierreMensaje', 'ocultarMensajeTemporal',
    'programarOcultamientoMensaje', 'limpiarMensajesTemporales',
    'mostrarEstadoEntradaManual', 'limpiarEstadoEntradaManual'
].map(extract).join('\n'), context);
const run = code => vm.runInContext(code, context);
run("mostrarEstadoEntradaManual('<rechazo>', 'danger')");
assert.ok(elements.get('#entrada').content.includes('&lt;rechazo&gt;'));
assert.ok(elements.get('#entrada').content.includes('aria-label="Cerrar mensaje"'));
assert.equal(timers.size, 1);
run("mostrarEstadoEntradaManual('Otro rechazo', 'danger')");
assert.equal(timers.size, 1, 'Reinicia el plazo para el nuevo mensaje');
[...timers.values()][0]();
assert.ok(elements.get('#entrada').classes.has('d-none'));
assert.equal(timers.size, 0);
run("mostrarEstadoEntradaManual('Error', 'danger'); ocultarMensajeTemporal('#entrada')");
assert.equal(timers.size, 0, 'Cerrar cancela el temporizador');
run("mostrarEstadoEntradaManual('Error', 'danger'); mostrarEstadoEntradaManual('Consultando', 'info')");
assert.equal(timers.size, 0, 'Un error anterior no debe ocultar un estado nuevo');
assert.ok(!elements.get('#entrada').classes.has('d-none'));
run("programarOcultamientoMensaje('#rechazos'); mostrarEstadoEntradaManual('Error', 'danger'); limpiarMensajesTemporales()");
assert.equal(timers.size, 0);
assert.ok(elements.get('#rechazos').classes.has('d-none'));
run('limpiarEstadoEntradaManual()');
assert.ok(!elements.get('#entrada').classes.has('d-none'));
console.log('OK: cierre, 10 segundos, reinicio, escape HTML y limpieza de temporizadores.');

context.SELECTORES.tablaProductos = '#tabla';
context.tieneAdvertenciaProducto = () => false;
context.formatearCantidad = String;
context.formatearImporte = String;
vm.runInContext(extract('renderizarTablaProductos'), context);
context.productos = [{ p_id_barrado: '7798304840448', p_desc: 'Descripcion "larga" <producto>',
    cmd_cmb_desc: 'Combo', cantidad_tot: 1234.567, p_pvta: 1234567.89, p_pneto: 1000000, p_iva: 210000 }];
run('renderizarTablaProductos(productos)');
const html = elements.get('#tabla').content;
assert.ok(html.includes('title="Descripcion &quot;larga&quot; &lt;producto&gt; - Combo"'));
assert.ok(html.includes('ncdev-producto-descripcion'));
assert.ok(html.includes('7798304840448'));
assert.ok(html.includes('1234567.89'));
assert.equal((html.match(/<td /g) || []).length, 8);
assert.ok(!html.includes('small text-muted mt-1'), 'Combo en la misma linea');
console.log('OK: descripcion con tooltip seguro, combo en linea, codigo e importes completos.');
