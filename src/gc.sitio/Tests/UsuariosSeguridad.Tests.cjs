// Pruebas sin red ni cuentas reales. No imprimir ni copiar la clave configurada.
const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const sitio = path.resolve(__dirname, '..');
const reglas = fs.readFileSync(path.join(sitio, 'wwwroot/js/app/areas/usuarios/userRules.js'), 'utf8');
const comunes = fs.readFileSync(path.join(sitio, 'wwwroot/js/app/siteGen.js'), 'utf8');
const funcion = reglas.slice(reglas.indexOf('function confirmarOperacionSeguridadUsuario('), reglas.indexOf('function confirmarDatosJsTree('));
const modal = comunes.slice(comunes.indexOf('function AbrirMensaje('), comunes.indexOf('//function AbrirMensaje('));

function preparar(usuario = 'prueba') {
    const elementos = new Map();
    const eventos = [];
    function $(selector) {
        if (!elementos.has(selector)) {
            const el = { clases: new Set(), propiedades: {}, contenido: '', visible: false };
            el.text = function (valor) {
                if (valor === undefined) return this.contenido;
                this.contenido = String(valor).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
                return this;
            };
            el.html = function (valor) { if (valor === undefined) return this.contenido; this.contenido = valor; return this; };
            el.val = () => '';
            el.show = function () { this.visible = true; return this; };
            el.hide = function () { this.visible = false; return this; };
            el.prop = function (nombre, valor) { this.propiedades[nombre] = valor; return this; };
            el.addClass = function (valor) { this.clases.add(valor); return this; };
            el.removeClass = function (valor) { valor.split(' ').forEach(c => this.clases.delete(c)); return this; };
            el.modal = function (accion) { eventos.push(['modal', accion]); return this; };
            elementos.set(selector, el);
        }
        return elementos.get(selector);
    }
    const contexto = {
        $, usuSelect: usuario, blanquearClaveUrl: '/usuarios/cgusuarios/BlanquearClave',
        desbloquearUsuarioUrl: '/usuarios/cgusuarios/DesbloquearUsuario',
        ControlaMensajeWarning: msg => eventos.push(['warn', msg]),
        ControlaMensajeError: msg => eventos.push(['error', msg]),
        ControlaMensajeSuccess: msg => eventos.push(['success', msg]),
        AbrirWaiting: () => eventos.push(['waiting']), CerrarWaiting: () => eventos.push(['closeWaiting']),
        actualizarAccionesSeguridadUsuario: () => eventos.push(['refresh']),
        PostGen: (data, url, callback) => eventos.push(['post', data, url, callback])
    };
    vm.createContext(contexto);
    vm.runInContext(modal + '\n' + funcion, contexto);
    return { contexto, $, eventos };
}

test('el blanqueo utiliza el modal real con estilo warn, icono y acciones claras', () => {
    const { contexto, $, eventos } = preparar();
    contexto.confirmarOperacionSeguridadUsuario('BLANQUEAR');
    assert.ok($('#msjHeader').clases.has('warn'));
    assert.match($('#msjIcono').html(), /bx-error/);
    assert.equal($('#btnMensajeAceptar').text(), 'Blanquear clave');
    assert.equal($('#btnMensajeCancelar').text(), 'Cancelar');
    assert.equal($('#btnMensajeCancelar').visible, true);
    assert.match($('#msjContenido').html(), /prueba/);
    assert.match($('#msjContenido').html(), /vencimiento/);
    assert.equal(eventos.filter(e => e[0] === 'post').length, 0);
});

test('cancelar no envía peticiones ni modifica credenciales', () => {
    const { contexto, eventos } = preparar();
    contexto.confirmarOperacionSeguridadUsuario('BLANQUEAR');
    contexto.FunctionCallback('NO');
    assert.equal(eventos.filter(e => e[0] === 'post' || e[0] === 'waiting').length, 0);
});

test('confirmar envía solo usuId, mantiene el contrato SI y procesa éxito', () => {
    const { contexto, eventos } = preparar();
    contexto.confirmarOperacionSeguridadUsuario('BLANQUEAR');
    contexto.FunctionCallback('SI');
    const envios = eventos.filter(e => e[0] === 'post');
    assert.equal(envios.length, 1);
    assert.equal(JSON.stringify(envios[0][1]), '{"usuId":"prueba"}');
    assert.equal(envios[0][2], contexto.blanquearClaveUrl);
    envios[0][3]({ error: false, warn: false, msg: 'Operación completada' });
    assert.ok(eventos.some(e => e[0] === 'success'));
    assert.ok(eventos.some(e => e[0] === 'refresh'));
});

test('un error o advertencia del servidor no se presenta como éxito', () => {
    for (const tipo of ['error', 'warn']) {
        const { contexto, eventos } = preparar();
        contexto.confirmarOperacionSeguridadUsuario('BLANQUEAR');
        contexto.FunctionCallback('SI');
        eventos.find(e => e[0] === 'post')[3]({ [tipo]: true, msg: 'No completado' });
        assert.ok(eventos.some(e => e[0] === tipo));
        assert.ok(!eventos.some(e => e[0] === 'success'));
    }
});

test('sin usuario no abre confirmación; el identificador se escapa como texto', () => {
    const vacio = preparar('');
    vacio.contexto.confirmarOperacionSeguridadUsuario('BLANQUEAR');
    assert.ok(vacio.eventos.some(e => e[0] === 'warn'));
    assert.ok(!vacio.eventos.some(e => e[0] === 'modal'));
    const html = preparar('<b>&');
    html.contexto.confirmarOperacionSeguridadUsuario('BLANQUEAR');
    assert.match(html.$('#msjContenido').html(), /&lt;b&gt;&amp;/);
});

test('desbloqueo mantiene su endpoint y utiliza aviso de advertencia', () => {
    const { contexto, $, eventos } = preparar();
    contexto.confirmarOperacionSeguridadUsuario('DESBLOQUEAR');
    assert.equal($('#btnMensajeAceptar').text(), 'Desbloquear usuario');
    assert.ok($('#msjHeader').clases.has('warn'));
    contexto.FunctionCallback('SI');
    assert.equal(eventos.find(e => e[0] === 'post')[2], contexto.desbloquearUsuarioUrl);
});

test('la API tiene un valor temporal válido sin exponerlo en salida ni frontend', () => {
    // appsettings admite comentarios JSONC. Extraer solo el literal JSON de esta
    // sección evita rechazar comentarios ajenos o mostrar secretos en errores.
    const config = fs.readFileSync(path.join(sitio, '../gc.api/appsettings.json'), 'utf8');
    const seccion = config.match(/"SecurityOperations"\s*:\s*\{\s*"TemporaryPassword"\s*:\s*("(?:\\.|[^"\\])*")/);
    assert.ok(seccion, 'No se encontró la configuración temporal esperada');
    const valor = JSON.parse(seccion[1]);
    assert.ok(typeof valor === 'string' && valor.trim().length > 0, 'Falta configurar la clave temporal');
    assert.ok(valor.length <= 128 && valor === valor.trim(), 'Formato de configuración no válido');
    assert.ok(!reglas.includes(valor), 'El secreto no debe estar en el JavaScript');
});
