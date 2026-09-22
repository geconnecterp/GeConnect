const fs = require('node:fs'), path = require('node:path'), vm = require('node:vm'), assert = require('node:assert/strict');
const read = name => fs.readFileSync(path.join(__dirname, '../wwwroot/js/app', name), 'utf8');
function extract(source, name) {
 const start = source.search(new RegExp('^function ' + name + '\\(', 'm')); assert(start >= 0, name);
 const end = /^}\r?$/m.exec(source.slice(start)); return source.slice(start, start + end.index + 1);
}
let moduleName = 'CobranzaCtaCte', rows = '', message, callback, destination, loadingClosed = 0;
function $(selector) { return { length: 1, attr: () => moduleName, hasClass: () => false,
 empty(){rows='';return this;}, append(s){rows+=s;return this;}, html(s){rows=s;return this;},
 val(){return this;}, prop(){return this;}, modal(){return this;} }; }
const ctx = { $, console: {log(){},warn(){},error(message,error){throw error || Error(message);}},
 window: {location:{replace:url=>destination=url}},
 accesoModuloCCUrl:'/Facturacion/CobranzaCtaCte',
 escapeHtml: s=>String(s).replaceAll('<','&lt;').replaceAll('>','&gt;'),
 poblarDatosClienteCC(){}, seleccionarTodosMovimientosCC(){},
 ocultarLoadingGlobal(){loadingClosed++;},
 AbrirMensaje(title,body,fn){message={title,body};callback=fn;},
 obtenerTipoComprobante:()=> 'Factura B', reiniciaPantallaAlVolver:()=>{throw Error('CC no debe reiniciar ventas');}
};
vm.createContext(ctx);
const functions = {
 'fact.js':['obtenerEmiteClientePorModulo'],
 'pagoCtaCte.js':['mostrarCtaCtePendientes','normalizarMontoCC','redondearMontoCC','normalizarFechaParaServidorCC','escaparHtmlCC','obtenerPrimerValorCC','formatearFechaCC','formatearMontoCC'],
 'pagoFactura.js':['mostrarResultadoCobranzaCtaCte','procesarPagoExitoso','ejecutarReinicioDespuesDePagoExitoso']
};
for (const [file,names] of Object.entries(functions)) vm.runInContext(names.map(n=>extract(read(file),n)).join('\n'), ctx);
let count=0;function test(name,fn){fn();count++;console.log('OK '+name);}
test('Emite vacío en CC, incluso con Factura B en el cliente',()=>{assert.equal(ctx.obtenerEmiteClientePorModulo({emite:'Factura B'}),'');});
test('Anulación de Cobranza tampoco informa Emite',()=>{moduleName='AnulacionCobranza';assert.equal(ctx.obtenerEmiteClientePorModulo({emite:'Factura B'}),'');moduleName='CobranzaCtaCte';});
test('Facturación conserva su Emite',()=>{moduleName='Facturacion';assert.equal(ctx.obtenerEmiteClientePorModulo({emite:'Factura B'}),'Factura B');moduleName='CobranzaCtaCte';});
const debit={tco_id:'006',tco_desc:'Factura <B>',cm_compte:'0001-00000123',cv_importe:80,cv_importe_ori:100,cv_fecha_vto:'2026-09-21',cta_id:'CLIENTE_OCULTO',cv_concepto:'CONCEPTO_NO_ES_TIPO'};
test('Grilla muestra descripción escapada y siete columnas, conservando código interno y saldo',()=>{
 ctx.mostrarCtaCtePendientes({nombre:'NOMBRE_OCULTO'},[debit]);
 assert.equal((rows.match(/<td(?:\s|>)/g)||[]).length,7);
 assert(rows.includes('Factura &lt;B&gt;'));assert(rows.includes('data-tco-id="006"'));
 assert(!rows.includes('<td>006</td>'));assert(!rows.includes('NOMBRE_OCULTO'));assert(!rows.includes('<td>CLIENTE_OCULTO'));
 assert(rows.includes('data-importe-bak="80.00"'));assert(rows.includes('data-importe-ori="100.00"'));
});
test('Sin descripción no inventa el tipo usando el concepto o el código',()=>{ctx.mostrarCtaCtePendientes({},[{...debit,tco_desc:''}]);assert(rows.includes('Sin descripción'));assert(!rows.includes('<td>006</td>'));});
test('Grilla vacía conserva alineación de siete columnas',()=>{ctx.mostrarCtaCtePendientes({},[]);assert(rows.includes('colspan="7"'));});
test('CC confirma con número de recibo y vuelve al propio módulo',()=>{
 assert.equal(ctx.mostrarResultadoCobranzaCtaCte({rb_compte:'00000123',cm_compte:'NO_MOSTRAR'},'CC'),true);
 assert(message.body.includes('Recibo de cobranza'));assert(message.body.includes('00000123'));assert(!message.body.includes('NO_MOSTRAR'));
 callback();assert.equal(destination,'/Facturacion/CobranzaCtaCte');assert.equal(loadingClosed,1);
});
test('La bandera del servidor reconoce CC aunque el contexto global no esté',()=>{assert(ctx.mostrarResultadoCobranzaCtaCte({rb_compte:'RC-7',es_cobranza_cuenta_corriente:true},''));assert(message.body.includes('RC-7'));});
test('CC sin número no muestra una factura como recibo',()=>{ctx.mostrarResultadoCobranzaCtaCte({cm_compte:'FACTURA-99'},'CC');assert(!message.body.includes('FACTURA-99'));assert(message.body.includes('No informado por el servidor'));});
test('Facturación y CD no se desvían a la salida especial CC',()=>{assert.equal(ctx.mostrarResultadoCobranzaCtaCte({},'CR'),false);assert.equal(ctx.mostrarResultadoCobranzaCtaCte({},'CD'),false);});
test('Confirmación de CD conserva el número de recibo',()=>{ctx.procesarPagoExitoso({rb_compte:'CD-12'},true);assert(message.body.includes('CD-12'));assert(message.body.includes('Recibo de cobranza'));});
console.log(`${count} escenarios de presentación CC aprobados.`);
