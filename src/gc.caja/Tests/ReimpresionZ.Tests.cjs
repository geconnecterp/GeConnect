const assert = require('node:assert/strict');
const fs = require('node:fs');
const { validar } = require('../wwwroot/js/app/reimpresion-z.js');
const casos = JSON.parse(fs.readFileSync(__dirname + '/ReimpresionZ.Casos.json', 'utf8'));
for (const c of casos) assert.equal(validar(c.porFecha, c.desde, c.hasta, '2026-09-29', '2021-09-29') === '', c.valido, c.nombre);
console.log(casos.length + ' pruebas de rangos JavaScript aprobadas.');

// Prueba de los eventos reales del formulario con DOM/transporte simulados.
const vm = require('node:vm');
function pantalla(habilitado = true) {
    class Elemento {
        constructor() { this.value = ''; this.events = {}; this.disabled = false; this.hidden = false; this.dataset = {}; this.classList = { toggle() {} }; }
        addEventListener(evento, callback) { this.events[evento] = callback; }
        setAttribute(k, v) { this[k] = v; }
        removeAttribute(k) { delete this[k]; }
        focus() {} select() {}
    }
    const ids = Object.fromEntries(['rzForm', 'rzCampos', 'rzDesde', 'rzHasta', 'rzImprimir', 'rzNueva', 'rzVolver', 'rzEstado', 'rzAyuda'].map(k => [k, new Elemento()]));
    const modo = { value: 'numero' }, token = { value: 'csrf-simulado' }, radios = [new Elemento(), new Elemento()];
    ids.rzForm.dataset = { habilitado: String(habilitado), hoy: '2026-09-29', minHasta: '2021-09-29', url: '/Imprimir' };
    ids.rzForm.querySelector = selector => selector.includes('rzModo') ? modo : token;
    ids.rzForm.querySelectorAll = () => radios;
    const mensajes = [], solicitudes = [];
    const $ = () => ({ modal() {} });
    $.ajax = opciones => {
        const operacion = { opciones, done(f) { this.ok = f; return this; }, fail(f) { this.error = f; return this; }, always(f) { this.fin = f; return this; } };
        solicitudes.push(operacion); return operacion;
    };
    const document = { getElementById: k => ids[k], addEventListener: (n, f) => f(), createElement: () => ({ textContent: '', get innerHTML() { return this.textContent; } }) };
    vm.runInNewContext(fs.readFileSync(__dirname + '/../wwwroot/js/app/reimpresion-z.js', 'utf8'), {
        window: { document, addEventListener() {} }, document, $, AbrirMensaje: (...args) => mensajes.push(args), Date
    });
    return { ids, mensajes, solicitudes, modo, radios,
        submit(d='5000', h='5005') { ids.rzDesde.value=d; ids.rzHasta.value=h; ids.rzForm.events.submit({ preventDefault() {} }); },
        confirmar(r='SI') { mensajes.at(-1)[2](r); },
        resolver(r) { const s=solicitudes.at(-1); s.ok(r); s.fin(); },
        fallar() { const s=solicitudes.at(-1); s.error({status:0}); s.fin(); }
    };
}
let flujos = 0;
function check(ok, nombre) { assert.ok(ok, nombre); flujos++; }
{
    const p=pantalla(); p.submit('5000','5006'); check(p.mensajes.length===0 && p.solicitudes.length===0, 'Rango inválido no llega a confirmar ni enviar');
    p.submit(); check(p.ids.rzCampos.disabled && p.ids.rzImprimir.disabled, 'Bloqueo previo a confirmar');
    p.submit(); check(p.mensajes.length===1, 'Doble submit no duplica confirmación');
    p.confirmar('NO'); check(!p.ids.rzCampos.disabled && p.solicitudes.length===0, 'Cancelar permite editar sin enviar');
    p.submit(); p.confirmar(); p.confirmar(); check(p.solicitudes.length===1, 'Doble aceptación envía una sola vez');
    check(JSON.parse(p.solicitudes[0].opciones.data).Desde==='5000' && p.solicitudes[0].opciones.headers.RequestVerificationToken==='csrf-simulado', 'Rango y antiforgery enviados');
    p.resolver({ok:true,incierto:false,mensaje:'OK'}); p.submit(); check(p.solicitudes.length===1 && p.ids.rzCampos.disabled, 'Éxito mantiene bloqueo hasta preparar otra');
    p.ids.rzNueva.events.click(); check(!p.ids.rzCampos.disabled && p.ids.rzDesde.value==='', 'Otra solicitud comienza limpia');
}
{
    const p=pantalla(); p.submit(); p.confirmar(); p.fallar(); check(p.ids.rzCampos.disabled && p.ids.rzEstado.textContent.includes('Revise el controlador'), 'Fallo de transporte deja resultado incierto');
    p.ids.rzNueva.events.click(); p.confirmar('NO'); check(p.ids.rzCampos.disabled && p.solicitudes.length===1, 'Resultado incierto no se reintenta automáticamente');
    p.ids.rzNueva.events.click(); p.confirmar(); check(!p.ids.rzCampos.disabled, 'Verificación explícita permite preparar otra');
}
{
    const p=pantalla(false); p.submit(); check(p.mensajes.length===0 && p.solicitudes.length===0, 'Controlador no compatible no puede enviar');
}
{
    const p=pantalla(); p.modo.value='fecha'; p.radios[1].events.change(); check(p.ids.rzDesde.value==='2026-09-29' && p.ids.rzHasta.min==='2021-09-29', 'Fechas iniciales y límite desde servidor');
}
console.log(flujos + ' pruebas de interacción aprobadas. DOM y transporte simulados.');

const caja = fs.readFileSync(__dirname + '/../wwwroot/js/app/caja.js', 'utf8');
const configurar = caja.match(/function configurarMenuSegunAcceso\(\) \{[\s\S]*?\r?\n    \}/)[0];
let casosMenu = 0;
for (const nivel of ['completo','parcial','solo-cierre','ninguno','desconocido']) {
    for (const disponible of [true,false]) {
        let deshabilitado;
        const generico = { prop() { return this; }, removeClass() { return this; }, addClass() { return this; }, not() { return this; }, filter(s) { return s === '[data-action="reportes-z"]' ? z : this; } };
        const z = { prop(n,v) { if(n==='disabled') deshabilitado=v; return this; }, toggleClass() { return this; }, attr() { return this; } };
        vm.runInNewContext(configurar + '; configurarMenuSegunAcceso();', { $:()=>generico, nivelAccesoMenu:nivel, reimpresionZDisponible:disponible, console:{log(){},warn(){}} });
        assert.equal(deshabilitado, !(disponible && ['completo','parcial','solo-cierre'].includes(nivel)), 'Disponibilidad Z: '+nivel+'/'+disponible);
        casosMenu++;
    }
}
console.log(casosMenu + ' pruebas de disponibilidad en menú aprobadas.');
