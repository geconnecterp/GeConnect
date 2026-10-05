const fs = require('node:fs'), path = require('node:path'), vm = require('node:vm'), assert = require('node:assert/strict');
const read = f => fs.readFileSync(path.join(__dirname, '../wwwroot/js/app', f), 'utf8');
function extract(file, name, indent = '') {
    const source = read(file);
    const match = source.match(new RegExp(`^${indent}function ${name}[(][^]*?^${indent}}`, 'm'));
    assert.ok(match, name); return match[0];
}
const escape = x => String(x || '').replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;');
let passed = 0;
async function scenario(module, fe, reportFailure = false, warning = false, missingReports = false) {
    let reports = 0, html = '', ajax, loadingClosed = 0, alerts = 0;
    const cadena = { modal(){return this;}, hasClass(){return false;}, length:0, is(){return false;},
        prop(){return this;}, off(){return this;}, one(){return this;} };
    const $ = () => cadena;
    $.ajax = options => { ajax = options; const req = {done(fn){ajax.done=fn;return req;},fail(){return req;}};return req; };
    const ctx = vm.createContext({ $, confirmacionPagoEnCurso:false, confirmacionPagoConfirmada:false, console:{log(){},error(){},warn(){}},
        window:{_coTipoActual:'CR', location:{href:'/actual'}},
        setTimeout(fn, ms){if(ms < 1000) fn();},
        escapeHtml:escape, escaparHtml:escape,
        AbrirMensaje(t,m,fn){html=m; if(t.includes('Advertencia')) {alerts++;fn();}},
        mostrarMensaje(t,m){html=m;},
        mostrarAdvertenciaPV(m,fn){alerts++;fn();},
        mostrarLoaderNdcfs(){}, ocultarLoaderNdcfs(){},
        logPaso(){},logError(){},
        obtenerTipoComprobante(){return 'Factura A';},
        actualizarMensajeLoadingGlobal(){},ocultarLoadingGlobal(){loadingClosed++;},
        bloquearPantallaCalculoFactura(){},desbloquearPantallaCalculoFactura(){loadingClosed++;},
        mostrarResultadoCobranzaCtaCte(){return false;},
        DiferirPagoUrl:'/diferir',
        ModuloReportes:{generarYVisualizarReporte(){reports++;return reportFailure ? Promise.reject(Error('Sin PDF')) : Promise.resolve(true);}}
    });
    if(missingReports) delete ctx.ModuloReportes;
    const response = {ok:true,debe_imprimir:fe, mensaje:'Comprobante emitido',
        mensaje_emision:fe ? 'Comprobante emitido mediante facturación electrónica.' : 'Comprobante emitido mediante controlador fiscal. Verifique la impresión en el equipo.',
        mostrar_mensaje_pv:warning,mensaje_advertencia:warning?'Aviso del PV':'',
        data:[{tco_letra:'A',tco_id:'001',cm_compte:'0001-00000065',cm_repetido:0}]};
    response.data[0].mensaje_emision=response.mensaje_emision;
    if(module==='NC') {
        vm.runInContext(['renderizarMensajeEmisionExitosa','procesarFinalizacionExitosa'].map(n=>extract('ncDevolucionProductos.js',n,'    ')).join('\n'),ctx);
        ctx.procesarFinalizacionExitosa(response);
    } else if(module==='NDNCFS') {
        vm.runInContext(['obtenerUrlReinicioModulo','construirMensajeConfirmacion','procesarConfirmacionExitosa'].map(n=>extract('ndNcFs.js',n,'    ')).join('\n'),ctx);
        ctx.procesarConfirmacionExitosa(response);
    } else if(module==='VENTA') {
        vm.runInContext(['enviarPayloadAlServidor','procesarPagoExitoso'].map(n=>extract('pagoFactura.js',n)).join('\n'),ctx);
        ctx.enviarPayloadAlServidor([],[],'Facturacion',[]);ajax.done(response);
    } else {
        vm.runInContext(['ejecutarDiferirPago','escaparMensajeEmision','mostrarMensajeExitoDiferirPago'].map(n=>extract('prodfactcalc.js',n)).join('\n'),ctx);
        ctx.ejecutarDiferirPago();ajax.success(response);
    }
    await new Promise(resolve=>setImmediate(resolve));
    assert.equal(reports, fe && !missingReports ? 1 : 0, `${module}: llamadas al reporte`);
    assert.match(html,/0001-00000065/,`${module}: conserva número`);
    assert.doesNotMatch(html,/visualizado exitosamente|impreso exitosamente/);
    if(!fe) { assert.match(html,/controlador fiscal/); assert.doesNotMatch(html,/facturación electrónica/); }
    if(warning && ['VENTA','DIFERIR'].includes(module)) assert.equal(alerts,1,'Conserva advertencia PV');
    if(['VENTA','DIFERIR'].includes(module)) assert.ok(loadingClosed,'Desbloquea al terminar');
    passed++;
}
(async()=>{
    for(const module of ['NC','NDNCFS','VENTA','DIFERIR']) {
        await scenario(module,false);
        await scenario(module,true);
        await scenario(module,true,true);
        await scenario(module,false,false,false,true);
    }
    for(const module of ['VENTA','DIFERIR']) {
        await scenario(module,false,false,true);
        await scenario(module,true,false,true);
        await scenario(module,true,false,false,true);
    }
    for(const [file, fn, indent] of [['ncDevolucionProductos.js','renderizarMensajeEmisionExitosa','    '],['ndNcFs.js','construirMensajeConfirmacion','    ']]) {
        const ctx=vm.createContext({escaparHtml:escape});vm.runInContext(extract(file,fn,indent),ctx);
        const html=ctx[fn]({mensaje_emision:'<img onerror=alert(1)>'},{cm_compte:'123'});
        assert.ok(html.includes('&lt;img'));assert.ok(!html.includes('<img'));passed++;
    }
    console.log(`PASS: ${passed} escenarios de presentación NC, ND/NC/FS, Facturación y Diferir Pago (FE/CF, avisos y fallas de reporte).`);
})().catch(e=>{console.error(e);process.exitCode=1;});
