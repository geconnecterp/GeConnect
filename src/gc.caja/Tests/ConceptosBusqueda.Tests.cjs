const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const root = path.resolve(__dirname, '..');
const read = rel => fs.readFileSync(path.join(root, rel), 'utf8').replace(/^\uFEFF/, '');
const source = {
    comun: read('wwwroot/js/app/siteGen.js'),
    producto: read('wwwroot/js/app/prodfact.js'),
    calculo: read('wwwroot/js/app/prodfactcalc.js'),
    nc: read('wwwroot/js/app/ncDevolucionProductos.js'),
    nd: read('wwwroot/js/app/ndNcFs.js')
};
function extract(file, name) {
    const found = source[file].match(new RegExp('^( *)function ' + name + '\\([^]*?^\\1}', 'm'));
    assert.ok(found, file + ':' + name);
    return found[0];
}
const common = extract('comun', 'ordenarConceptosCalculo');
const buttonFns = ['actualizarEstadoBusquedaProducto', 'buscarProductoPorCodigo', 'limpiarVentaCompleta'].map(n => extract('producto', n)).join('\n');
const view = read('Areas/Facturacion/Views/ProductoFact/_productoFactura.cshtml');
const button = view.match(/<button\b[^>]*id="btnBuscarProducto"[^]*?<\/button>/)[0];
const icon = button.slice(button.indexOf('>') + 1, button.lastIndexOf('</button>'));
const tester = [
    { orden: 2, tipo: 'CO', concepto: 'Fernet 3x2 rico', importe: -8201.87, id_aux: '006787' },
    { orden: 1, tipo: 'SU', concepto: 'Subtotal', importe: 82018.74, id_aux: '' }
];
let checks = 0;
function check(condition, message) { assert.ok(condition, message); checks++; }
function createDom() {
    const elements = new Map();
    const $ = selector => {
        if (!elements.has(selector)) elements.set(selector, {
            content: '', textValue: '', value: '', properties: {}, attributes: {}, writes: 0,
            html(value) { if (value === undefined) return this.content; this.content = value; this.writes++; return this; },
            text(value) { if (value === undefined) return this.textValue; this.textValue = value; return this; },
            empty() { this.content = ''; return this; },
            append(value) { this.content += value; return this; },
            prop(name, value) { if (value === undefined) return this.properties[name]; this.properties[name] = value; return this; },
            attr(name, value) { if (value === undefined) return this.attributes[name]; this.attributes[name] = value; return this; },
            val(value) { if (value === undefined) return this.value; this.value = value; return this; },
            remove() { return this; }, hasClass() { return false; }, addClass() { return this; }, removeClass() { return this; }, trigger() { return this; }, modal() { return this; }
        });
        return elements.get(selector);
    };
    return $;
}
const silent = { log() {}, error() {}, warn() {} };
const $ = createDom();
const context = { $, console: silent, escapeHtml: s => String(s).replaceAll('<', '&lt;'),
    escaparHtml: s => String(s).replaceAll('<', '&lt;'), formatearNumero: n => Number(n).toFixed(2),
    formatearImporte: n => Number(n).toFixed(2), formatearMoneda: n => '$ ' + Number(n).toFixed(2),
    parsearNumero: (n, fallback = 0) => Number.isFinite(Number(n)) ? Number(n) : fallback,
    SELECTORES: { conceptosCalculo: '#nc', tbodySubtotales: '#nd', totalCalculo: '#totalNd' },
    esFilaTotal: () => false, esTotal: () => false, logWarn() {}
};
vm.createContext(context);
vm.runInContext(common, context);
for (const input of [tester, [
    {orden: '10', concepto: 'Décimo'}, {orden: '2', concepto: 'Segundo'}, {Orden: 1, concepto: 'Primero'}
]]) {
    const original = JSON.stringify(input);
    const result = context.ordenarConceptosCalculo(input);
    check(result[0].concepto === (input === tester ? 'Subtotal' : 'Primero'), 'Orden numérico ascendente, no orden de llegada');
    check(JSON.stringify(input) === original && result !== input, 'Orden visual sin mutar respuesta original');
}
const irregular = [
    {concepto:'Sin orden'}, {orden:'',concepto:'Vacío'}, {orden:2,concepto:'Segundo A'},
    {orden:'2',concepto:'Segundo B'}, {orden:0,concepto:'Cero'}, {orden:null,concepto:'Nulo'},
    {orden:'abc',concepto:'Inválido'}, {Orden:1,concepto:'Primero'}, {orden:true,concepto:'Booleano'}
];
check(context.ordenarConceptosCalculo(irregular).map(x=>x.concepto).join('|') === 'Cero|Primero|Segundo A|Segundo B|Sin orden|Vacío|Nulo|Inválido|Booleano', 'Empates y valores ausentes/inválidos mantienen orden estable al final');
for (const input of [null, {}, undefined, [], '[]']) check(context.ordenarConceptosCalculo(input).length === 0, 'Colección ausente/vacía segura');
const frozen = Object.freeze(tester.map(x=>Object.freeze({...x})));
check(context.ordenarConceptosCalculo(frozen)[0].importe === 82018.74, 'Colección inmutable admitida');
vm.runInContext(extract('calculo','cargarConceptosCalculoFactura'), context);
// Caracterización de la causa previa: forEach sin ordenar conserva primero el CO.
vm.runInContext(extract('calculo','cargarConceptosCalculoFactura').replace('function cargarConceptosCalculoFactura(', 'function renderAnterior(').replace('ordenarConceptosCalculo(subtotales).forEach', 'subtotales.forEach'), context);
context.renderAnterior(tester);
check($('#tbodyConceptosCalculo').content.indexOf('Fernet') < $('#tbodyConceptosCalculo').content.indexOf('Subtotal'), 'Reproduce la falta de orden anterior');
for (const data of [tester, [...tester].reverse(), frozen]) {
    context.cargarConceptosCalculoFactura(data);
    const html = $('#tbodyConceptosCalculo').content;
    check(html.indexOf('Subtotal') < html.indexOf('Fernet'), 'Facturación muestra SU antes de CO');
    check(html.includes('-8201.87') && $('#tdTotalFinal').textValue === '$ 73816.87', 'Facturación conserva descuento y total');
}
context.cargarConceptosCalculoFactura([{orden:1,concepto:'Cero',importe:0,total:100}]);
check($('#tdTotalFinal').textValue === '$ 0.00', 'Importe cero no se sustituye por un total alternativo');
vm.runInContext(['obtenerValor','normalizarNumero','normalizarFilaSubtotal','renderizarConceptosCalculo'].map(n=>extract('nc',n)).join('\n'), context);
const ncRows = tester.map(x=>context.normalizarFilaSubtotal(x));
context.renderizarConceptosCalculo(ncRows);
check($('#nc').content.indexOf('Subtotal') < $('#nc').content.indexOf('Fernet'), 'NC devolución ordena conceptos');
check($('#nc').content.includes('-8201.87'), 'NC devolución conserva descuento negativo');
check(context.normalizarFilaSubtotal({Orden:2}).orden === 2 && context.normalizarFilaSubtotal({}).orden === null, 'NC conserva orden y distingue ausencia de cero');
vm.runInContext(['normalizarSubtotalesCalculo','parsearJsonSeguro','obtenerSubtotalesCalculo','calcularTotalSubtotales','renderSubtotales'].map(n=>extract('nd',n)).join('\n'), context);
for (const response of [{subtotales:tester}, {json_subtotal:JSON.stringify(tester)}, {calculo:{json_subtotal:JSON.stringify(tester)}}]) {
    context.renderSubtotales(response);
    check($('#nd').content.indexOf('Subtotal') < $('#nd').content.indexOf('Fernet'), 'ND/NC/FS ordena respuesta directa o JSON');
    check($('#nd').content.includes('-8201.87') && $('#totalNd').textValue === '$ 73816.87', 'ND/NC/FS conserva signos y total');
}
check(context.normalizarSubtotalesCalculo([{Orden:2,Concepto:'Segundo',Importe:-1}])[0].orden === 2, 'ND/NC/FS no descarta el campo orden');
// Ejecución real de inicio, respuesta y final de AJAX: el contenido nunca se reescribe.
const $button = createDom(); const search = $button('#btnBuscarProducto'); search.content = icon;
let request; $button.ajax = options => { request = options; };
const searchContext = { $:$button, console:silent, origenCargaActual:'directo',modoBloqueoGrilla:null,cajaAcumulaProductos:true,
    productosFactura:[], totalFactura:0, clienteActualFactura:null, ultimoCambioProducto:null,
    procesarRespuestaProducto() {}, mostrarMensajeEstado() {}, formatearNumero:n=>Number(n).toFixed(2),
    ocultarLoaderCalculando() {}, esSesionExpirada:()=>false, manejarSesionExpirada() {},
    resetearTotalesVisuales() {}, guardarDetalleLocal() {}, verificarBackupPendiente() {}
};
vm.createContext(searchContext); vm.runInContext(buttonFns, searchContext);
for (const origin of ['directo','busquedaAvanzada','prefactura','cotizacion'])
for (const outcome of ['ok','rechazo','red','sesion']) {
    request = undefined;
    searchContext.buscarProductoPorCodigo('P','031225',4.350,true,origin);
    check(search.properties.disabled === true && search.attributes['aria-busy'] === 'true' && search.content === icon, origin + '/' + outcome + ': lupa intacta durante espera');
    if (outcome === 'ok') request.success({ok:true,producto:{cantidad_tot:4.35,p_pvta:100},acumula:false});
    else if (outcome === 'rechazo') request.success({ok:false,mensaje:'Producto inexistente'});
    else { searchContext.esSesionExpirada = () => outcome === 'sesion'; request.error({status:outcome==='sesion'?401:500},'error','Simulado'); }
    request.complete();
    check(search.properties.disabled === false && search.attributes['aria-busy'] === 'false' && search.content === icon, origin + '/' + outcome + ': lupa intacta al regresar');
    searchContext.limpiarVentaCompleta(false);
    check(search.content === icon && search.writes === 0, 'Limpiar/regresar no sustituye el contenido del botón');
}
searchContext.modoBloqueoGrilla='cotizacion'; request=undefined;
searchContext.buscarProductoPorCodigo('P','031225',1,true,'directo');
check(request === undefined && search.content === icon, 'Bloqueo de cotización no altera la lupa');
check(button.includes('bx-search') && !button.includes('Cargar</') && button.includes('aria-label="Buscar y cargar producto"'), 'Vista original conserva botón pequeño de lupa y nombre accesible');
check(!extract('producto','actualizarEstadoBusquedaProducto').match(/\.html\(|\.text\(|innerHTML|replaceWith/), 'El gestor de estado no escribe contenido');
check(!source.producto.includes('$btnBuscar'), 'Eliminadas las tres reescrituras y el alias anterior');
console.log(`PASS: ${checks} comprobaciones de orden, importes y lupa fija en todos los desenlaces simulados. Sin API ni SP.`);

if (process.argv.includes('--serve')) {
    const http = require('node:http');
    const table = read('Areas/Facturacion/Views/ProductoFact/_productoFacturaCalculo.cshtml').match(/<table\b[^>]*id="tbConceptosCalculo"[^]*?<\/table>/)[0];
    const fixture = `<!doctype html><html lang="es"><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Caja · orden y lupa · simulación</title>
<link rel="stylesheet" href="/lib/bootstrap/css/bootstrap.min.css"><link rel="stylesheet" href="/lib/boxicons/css/boxicons.min.css"><link rel="stylesheet" href="/css/site.css">
<body class="bg-light"><main class="container-fluid p-4"><h1 class="h4">Facturación · verificación aislada</h1><p class="alert alert-info">Vista y funciones reales. Respuestas simuladas; sin acceso a SQL, clientes, facturas ni POS.</p>
<div class="card card-golden mb-3"><div class="card-header">Carga de productos</div><div class="card-body"><div class="input-group input-group-lg"><span class="input-group-text">Código</span><input class="form-control" id="txtCodigoProducto" value="031225">${button}<button class="btn btn-golden" id="btnBuscarProductos"><i class="bx bx-search"></i> Productos</button></div><div id="mensajeEstadoProducto" class="mt-2">Listo</div></div></div>
<div class="d-flex gap-2 flex-wrap mb-3"><button class="btn btn-secondary" id="espera">Iniciar búsqueda en espera</button><button class="btn btn-success" id="resolver">Resolver búsqueda</button><button class="btn btn-warning" id="rechazo">Producto inexistente</button><button class="btn btn-danger" id="red">Fallo de red</button><button class="btn btn-secondary" id="volver">Limpiar y volver</button><button class="btn btn-golden" id="reordenar">Reordenar respuesta invertida</button></div>
<div class="alert alert-success" id="verificacion" role="status"></div><div class="card card-golden"><div class="card-header">Detalle de Conceptos</div><div class="card-body p-0">${table}</div></div></main>
<script src="/lib/jquery/dist/jquery.min.js"></script><script>
${common}\n${extract('calculo','cargarConceptosCalculoFactura')}\n${buttonFns}
var origenCargaActual='directo',modoBloqueoGrilla=null,cajaAcumulaProductos=true,productosFactura=[],totalFactura=0,clienteActualFactura=null,ultimoCambioProducto=null;
function escapeHtml(s){return $('<div>').text(s).html();}function formatearNumero(n){return Number(n).toFixed(2);}function procesarRespuestaProducto(){}function ocultarLoaderCalculando(){}function esSesionExpirada(){return false;}function manejarSesionExpirada(){}function resetearTotalesVisuales(){}function guardarDetalleLocal(){}function verificarBackupPendiente(){}function mostrarMensajeEstado(m){$('#mensajeEstadoProducto').text(m);}
const fixtureRows=${JSON.stringify(tester)};const boton=document.getElementById('btnBuscarProducto'),original=boton.innerHTML,ancho=boton.getBoundingClientRect().width;let pending;
function verificar(){const filas=document.querySelector('#tbodyConceptosCalculo').textContent;const ok=boton.innerHTML===original && Math.abs(boton.getBoundingClientRect().width-ancho)<0.1 && filas.indexOf('Subtotal')<filas.indexOf('Fernet');document.getElementById('verificacion').textContent=(ok?'OK':'ERROR')+' · lupa fija · ancho '+boton.getBoundingClientRect().width.toFixed(1)+' px · orden 1 → 2 · CO -8201.87 · total 73816.87 · '+(boton.disabled?'búsqueda en curso':'listo');}
$.ajax=o=>{pending=o;verificar();};function buscar(){buscarProductoPorCodigo('P','031225',4.350,true,'busquedaAvanzada');verificar();}function resolver(tipo){if(!pending)buscar();const p=pending;pending=null;if(tipo==='red')p.error({status:500},'error','Simulado');else p.success(tipo==='rechazo'?{ok:false,mensaje:'Producto inexistente (simulado)'}:{ok:true,producto:{cantidad_tot:4.35,p_pvta:100}});p.complete();verificar();}
$('#btnBuscarProducto, #espera, #btnBuscarProductos').on('click',buscar);$('#resolver').on('click',()=>resolver('ok'));$('#rechazo').on('click',()=>resolver('rechazo'));$('#red').on('click',()=>resolver('red'));$('#volver').on('click',()=>{limpiarVentaCompleta(false);verificar();});$('#reordenar').on('click',()=>{cargarConceptosCalculoFactura(fixtureRows);verificar();});cargarConceptosCalculoFactura(fixtureRows);verificar();
</script></body></html>`;
    const www = path.join(root,'wwwroot');
    const server = http.createServer((req,res)=>{
        const url = new URL(req.url,'http://127.0.0.1');
        if(url.pathname==='/'){res.setHeader('Content-Type','text/html; charset=utf-8');res.end(fixture);return;}
        const file=path.resolve(www,'.'+decodeURIComponent(url.pathname));
        if(!file.startsWith(www+path.sep)||!fs.existsSync(file)||!fs.statSync(file).isFile()){res.writeHead(404);res.end();return;}
        const mime={'.js':'application/javascript','.css':'text/css','.woff2':'font/woff2','.woff':'font/woff','.ttf':'font/ttf','.svg':'image/svg+xml'}[path.extname(file)];
        if(!mime){res.writeHead(404);res.end();return;}
        res.setHeader('Content-Type',mime);res.end(fs.readFileSync(file));
    });
    server.listen(7301,'127.0.0.1',()=>console.log('Fixture visual aislada: http://127.0.0.1:7301/'));
}
