const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const assert = require('node:assert/strict');

const script = fs.readFileSync(path.join(__dirname, '../wwwroot/js/app/areas/docmngr/docmanager.js'), 'utf8');
const actions = ['EnviarEmail', 'GenerateOutlookWebLink', 'GenerateMailtoLink',
    'GenerateWhatsAppWebLink', 'GenerarPaqueteImpresion', 'GenerarURLsDocumentos'];

async function verify(base) {
    const requests = [];
    const revoked = [];
    const messages = [];
    let sequence = 0;
    let unified = true;
    let opened = 0;
    const ui = new Proxy({}, { get: (_, key) => key === 'is' ? () => unified : () => ui });
    const jq = () => ui; // DOM adapter only; real queue/printing functions execute.
    jq.ajax = options => {
        requests.push(options);
        options.success?.({ enlaces: [] });
    };
    jq.param = values => new URLSearchParams(values).toString();
    const endpoints = Object.fromEntries(actions.map(action =>
        [action, base + '/ControlComun/GestorImpresion/' + action]));
    const savedEndpoints = JSON.stringify(endpoints);
    const sandbox = {
        window: {
            gestorImpresionUrls: endpoints,
            currentModuleConfig: { moduloId: 'CCUENTAS', moduloTitulo: 'Cuenta corriente' },
            gestorDocumentalContexto: { contextoId: 'test-context', moduloGestor: 'CCUENTAS' },
            open: () => ({
                document: { write() {} },
                location: { replace() { opened++; } },
                close() {}
            })
        },
        document: {}, $: jq, console: { log() {}, warn() {}, error() {}, info() {} },
        administracion: '0000',
        arrRepoParams: [{ reporte: 'CuentaCorriente', parametros: { cuenta: 'TEST', Ids: [{ id: 1 }] } }],
        generadorArchivoUrl: base + '/ControlComun/GestorImpresion/GeneradorArchivo',
        PostGen: (_data, _url, success) => success({ base64: 'JVBERi0xLjQ=' }),
        Blob, Uint8Array, atob,
        AbrirWaiting() {}, CerrarWaiting() {},
        AbrirMensaje: (...args) => messages.push(args),
        URL: {
            createObjectURL: () => 'blob:test-' + (++sequence),
            revokeObjectURL: url => revoked.push(url)
        },
        setTimeout() {},
        fetch: async (url, options) => {
            requests.push({ url, ...options });
            return { ok: true, blob: async () => ({}), headers: { get: () => '1' } };
        }
    };
    vm.createContext(sandbox);
    vm.runInContext(script, sandbox);
    sandbox.obtenerReportesSeleccionados = () => [{ id: '1', text: 'Informe de prueba' }];

    const assertEndpoints = () => assert.equal(
        JSON.stringify(sandbox.window.gestorImpresionUrls), savedEndpoints,
        'La cola temporal no debe sobrescribir las rutas del servidor');

    // Closing/reopening the modal clears temporary PDFs, not endpoint configuration.
    sandbox.limpiarColaImpresionIndividual();
    sandbox.limpiarColaImpresionIndividual();
    assertEndpoints();

    await sandbox.imprimirArchivoSeleccionado();
    assert.equal(messages.length, 0, JSON.stringify(messages));
    assertEndpoints();
    assert.equal(requests[0].url, endpoints.GenerarPaqueteImpresion);
    assert.equal(requests[0].method, 'POST');
    assert.equal(requests[0].credentials, 'same-origin');
    const body = JSON.parse(requests[0].body);
    assert.equal(body.ContextoId, 'test-context');
    assert.equal(body.ModuloGestor, 'CCUENTAS');
    assert.equal(body.Solicitudes[0].Parametros.cuenta, 'TEST');
    assert.deepEqual(body.ReportesIds, [1]);
    assert.equal(body.Unificar, true);

    // Real individual generation, blob creation and cleanup, followed by unified printing.
    unified = false;
    await sandbox.imprimirArchivoSeleccionado();
    assert.equal(messages.length, 0, JSON.stringify(messages));
    assertEndpoints();
    const temporary = Array.from(sandbox.window.gestorImpresionBlobUrls);
    assert.equal(temporary.length, 1);
    unified = true;
    await sandbox.imprimirArchivoSeleccionado();
    assert.equal(messages.length, 0, JSON.stringify(messages));
    assertEndpoints();
    assert.deepEqual(revoked, temporary);
    assert.equal(requests[1].url, endpoints.GenerarPaqueteImpresion);
    assert.equal(opened, 2);

    await sandbox.generarURLsDocumentos([{ id: '1', text: 'Informe de prueba' }]);
    assert.equal(requests[2].url, endpoints.GenerarURLsDocumentos +
        '?contextoId=test-context&moduloGestor=CCUENTAS');
    assertEndpoints();
    console.log('PASS ' + (base || '/') +
        ': limpieza repetida, impresion global, individual -> global, liberacion de blobs y enlaces.');
}

(async () => {
    await verify('');
    await verify('/gcsitio');
})().catch(error => {
    console.error(error);
    process.exitCode = 1;
});

