const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const assert = require('node:assert/strict');

const source = fs.readFileSync(path.join(__dirname, '../wwwroot/js/app/ncDevolucion.js'), 'utf8');
function extract(name) {
    const match = source.match(new RegExp(`^    function ${name}\\([^]*?^    }`, 'm'));
    assert.ok(match, `Funcion no encontrada: ${name}`);
    return match[0];
}

let dialog;
let selected;
let mode;
let checks = 0;
const context = vm.createContext({
    Number,
    logInfo() {},
    logAdvertencia() {},
    resumirComprobante: value => value,
    notificarComprobanteOrigenValidado: value => { selected = value; },
    modalIdentificacion: null,
    modalRepetidos: null,
    setTimeout: callback => callback(),
    AbrirMensaje: (...args) => { dialog = args; },
    normalizarRespuestaMensaje: value => value,
    esRespuestaPositiva: value => value === 'SI',
    esRespuestaNegativa: value => value === 'NO',
    definirModalidadCargaInicial: value => { mode = value; }
});
vm.runInContext(`let comprobanteConNcPrevia = false;\n${extract('preguntarCargaInicialDetalle')}\n${extract('procesarComprobanteSeleccionado')}`, context);
for (const value of [1, '1', 0, null, undefined, 1, 0]) {
    context.input = { nc_ya_emitida: value };
    vm.runInContext('procesarComprobanteSeleccionado(input)', context);
    assert.equal(selected, context.input);
    assert.equal(dialog[1].includes('ya tiene una Nota de Crédito asociada'), Number(value) === 1);
    dialog[2]('SI');
    assert.equal(mode, true);
    dialog[2]('NO');
    assert.equal(mode, false);
    checks += 4;
}
console.log(`OK: ${checks} verificaciones de aviso NC previa y continuidad de carga.`);
