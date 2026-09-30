const fs = require('node:fs'), path = require('node:path'), vm = require('node:vm'), assert = require('node:assert/strict');
const source = fs.readFileSync(path.join(__dirname, '../wwwroot/js/app/seguridad/cuenta-menu.js'), 'utf8');
function setup(operativo = true, cuenta = false) {
    const events = {}, redirects = [], mensajes = [];
    let ajaxError;
    const context = {
        document: { addEventListener: (n, f) => events[n] = f,
            body: { classList: { contains: () => operativo } }, getElementById: () => cuenta ? {} : null },
        window: { addEventListener: (n, f) => events[n] = f, location: { assign: u => redirects.push(u) } },
        $: () => ({ modal() {}, ajaxError: f => ajaxError = f }),
        AbrirMensaje: (...args) => mensajes.push(args), confirmacionSeguraActiva: null
    };
    vm.createContext(context); vm.runInContext(source, context);
    return { context, redirects, mensajes, events, ajax: r => ajaxError(null, {responseJSON:r}),
        click(marked = true) {
            const event = { target: { closest: () => ({href:'/cuenta', hasAttribute:()=>marked}) },
                prevented: false, preventDefault() {this.prevented=true;}, stopImmediatePropagation() {} };
            events.click(event); return event;
        } };
}
let count = 0; function test(name, fn) { fn(); count++; console.log('OK ' + name); }
test('Inicio no pide aviso operativo', () => {const t=setup(false); assert(!t.click().prevented);});
test('Cuenta no se considera carga pendiente', () => {const t=setup(true,true); assert(!t.click().prevented);});
test('Operación pide consulta estándar y predetermina cancelar', () => {
    const t=setup(); assert(t.click().prevented); assert.equal(t.mensajes[0][5],'warn!'); assert.equal(t.mensajes[0][7],'cancelar');
});
test('Cancelar conserva la vista', () => {const t=setup(); t.click(); t.mensajes[0][2]('NO'); assert.equal(t.redirects.length,0); t.click(); assert.equal(t.mensajes.length,2);});
test('Continuar navega únicamente al destino elegido', () => {const t=setup(); t.click(); t.mensajes[0][2]('SI'); assert.equal(t.redirects[0],'/cuenta');});
test('Doble pulsación no superpone consultas', () => {const t=setup(); t.click(); t.click(); assert.equal(t.mensajes.length,1);});
test('Durante guardado de clave bloquea enlaces', () => {const t=setup(); t.context.window.PocketCuentaEnviando=true; assert(t.click(false).prevented); assert.equal(t.mensajes.length,0);});
test('Durante confirmación operativa no interfiere con modal activo', () => {const t=setup(); t.context.confirmacionSeguraActiva={}; assert(t.click().prevented); assert.equal(t.mensajes.length,0);});
test('Respuesta de cambio obligatorio redirige', () => {const t=setup(); t.ajax({cambioClave:true,redirect:'/obligatoria'}); assert.equal(t.redirects[0],'/obligatoria');});
test('No redirige errores operativos comunes', () => {const t=setup(); t.ajax({error:true,msg:'Error de carga'}); assert.equal(t.redirects.length,0);});
console.log(count + ' pruebas del menú de cuenta aprobadas.');
