// Fixture visual aislada: usa las vistas, estilos y renderizadores reales, sin API ni SP.
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const { chromium } = require(process.argv[2] || 'playwright');
const output = process.argv[3];
const root = path.join(__dirname, '..');
const read = p => fs.readFileSync(path.join(root, p), 'utf8').replace(/^\uFEFF/, '');
const partial = read('Areas/Facturacion/Views/Shared/_ResumenCuentaCompacto.cshtml');
const fields = [...partial.matchAll(/\(Id: "([^"]+)", Etiqueta: "([^"]+)", Clase: "([^"]+)"\)/g)];
assert.equal(fields.length, 9);
function summary(suffix) {
    return `<div class="cuenta-resumen-compacto">${fields.map(([,id,label,cls]) =>
        `<div class="cuenta-resumen-campo cuenta-resumen-${cls}"><label for="txtNdcfs${id}${suffix}">${label}</label><input class="form-control" id="txtNdcfs${id}${suffix}" readonly></div>`).join('')}</div>`;
}
function view(file, suffix) {
    return read('Areas/Facturacion/Views/NotaDebitoCredito/' + file)
        .replace(/@await Html.PartialAsync\([^\n]+/, summary(suffix));
}
let script = read('wwwroot/js/app/ndNcFs.js');
const start = script.indexOf('    $(function () {');
const end = script.indexOf('    function reiniciarEstadoModulo()', start);
assert.ok(start > 0 && end > start);
script = script.slice(0, start) + script.slice(end);
script = script.replace('})();', `window.ndcfsTest = {
    init() { tiposComprobanteOrigenCargados = true; registrarEventos(); },
    seed(items) { conceptos = items; renderConceptos(); },
    hydrate: hidratarDatosCuentaModulo, renderSubtotales,
    readyToConfirm() { calculoActual = {ok:true}; },
    confirm: ejecutarConfirmacion
}; })();`);
const css = ['lib/bootstrap/css/bootstrap.min.css', 'lib/boxicons/css/boxicons.min.css', 'css/site.css', 'css/golden-overlay.css', 'css/cuentaResumenCompacto.css', 'css/ndNcFs.css'];
const html = `<!doctype html><html lang="es"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
${css.map(p => `<link rel="stylesheet" href="/${p}">`).join('')}</head><body>
${view('_OperacionConceptos.cshtml', 'Conceptos')}${view('_Calculo.cshtml', 'Calculo')}
<script>${read('wwwroot/lib/jquery/dist/jquery.min.js')}</script>
<script>${read('wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js')}</script>
<script>${script}</script></body></html>`;

(async () => {
    const browser = await chromium.launch({ headless: true });
    try {
        for (const viewport of [{width:1920,height:1080}, {width:1366,height:768}, {width:1024,height:768}, {width:390,height:844}]) {
            const page = await browser.newPage({viewport});
            const errors = [];
            page.on('pageerror', e => errors.push(e.message));
            await page.route('**/*', route => {
                const url = new URL(route.request().url());
                if (url.origin !== 'https://ndcfs.test') return route.abort();
                if (url.pathname === '/') return route.fulfill({contentType:'text/html', body:html});
                const asset = path.resolve(root, 'wwwroot', '.' + url.pathname);
                if (!asset.startsWith(path.join(root, 'wwwroot') + path.sep) || !fs.existsSync(asset)) return route.abort();
                return route.fulfill({body:fs.readFileSync(asset), contentType:asset.endsWith('.css') ? 'text/css' : 'application/octet-stream'});
            });
            await page.goto('https://ndcfs.test/');
            await page.evaluate(() => {
                $('#cmbNdcfsOperacion').html('<option value="ND">Nota de Debito</option><option value="NC">Nota de Credito</option><option value="FS">Factura de Servicio</option>');
                $('#cmbNdcfsTcoOri').html('<option value="001">Factura A</option>');
                $('#cmbNdcfsIva').html('<option value="21">21.00</option>');
                ndcfsTest.init();
                const account = {cta_denominacion:'UNILEVER ARGENTINA S.A. FOODS ( E )', cta_id:'C0030226', cta_domicilio:'TUCUMAN 117 CAPITAL FEDERAL', cta_documento:'30501092966', afip_desc:'RESPONSABLE INSCRIPTO', cta_email:'JOSE.FAMAR@UNILEVER.COM', origen_desc:'PROVEEDOR'};
                ndcfsTest.hydrate(account, 'Conceptos'); ndcfsTest.hydrate(account, 'Calculo');
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalNdcfsConceptos')).show();
            });
            await page.waitForTimeout(350);
            for (const mode of ['ND','NC','FS']) {
                await page.evaluate(() => ndcfsTest.seed([]));
                await page.locator('#cmbNdcfsOperacion').selectOption(mode);
                assert.equal(await page.locator('#txtNdcfsPuntoVentaOri').isVisible(), mode === 'NC');
                assert.equal(await page.locator('#txtNdcfsEmiteConceptos').inputValue(), await page.locator('#cmbNdcfsOperacion option:checked').innerText());
                await page.evaluate(() => ndcfsTest.seed(Array.from({length:40}, (_,i) => ({concepto:'Concepto ' + (i+1) + ' "especial" <detalle> ' + 'texto extenso '.repeat(15), cantidad:2, netoGravado:1234567.89}))));
                assert.equal(await page.locator('#tbodyNdcfsConceptos tr').count(), 40);
                assert.equal(await page.locator('#btnNdcfsSeguirConceptos').isEnabled(), true);
                const metrics = await page.evaluate(() => {
                    const row = document.querySelector('#tbodyNdcfsConceptos tr');
                    const grid = document.querySelector('.ndcfs-tabla-conceptos');
                    const amountCells = [...row.querySelectorAll('.text-end')];
                    const input = document.querySelector('#txtNdcfsConcepto').getBoundingClientRect();
                    const add = document.querySelector('#btnNdcfsAgregarConcepto').getBoundingClientRect();
                    return {
                        overflow: document.documentElement.scrollWidth > innerWidth,
                        rowHeight: row.getBoundingClientRect().height,
                        numbersFull: amountCells.every(el => el.scrollWidth <= el.clientWidth),
                        aligned: Math.abs(input.bottom - add.bottom) < 2,
                        gridHeight: grid.getBoundingClientRect().height,
                        totalBottom: document.querySelector('.ndcfs-total-footer').getBoundingClientRect().bottom,
                        footerTop: document.querySelector('#modalNdcfsConceptos .modal-footer').getBoundingClientRect().top,
                        tooltip: row.querySelector('.ndcfs-descripcion span').title
                    };
                });
                assert.equal(metrics.overflow, false);
                assert.equal(metrics.numbersFull, true);
                assert.equal(metrics.aligned, true);
                assert.ok(metrics.rowHeight <= 40, JSON.stringify(metrics));
                assert.ok(metrics.gridHeight > 150);
                assert.match(metrics.tooltip, /"ESPECIAL" <DETALLE>/);
                if (viewport.width >= 768) assert.ok(metrics.totalBottom <= metrics.footerTop + 1, JSON.stringify(metrics));
                if (output && mode === 'NC') await page.screenshot({path:path.join(output, `ndcfs-nc-${viewport.width}.png`)});
                await page.locator('.btn-ndcfs-eliminar-concepto').first().click();
                assert.equal(await page.locator('#tbodyNdcfsConceptos tr').count(), 39);
            }
            // Importes pequenos y grandes deben conservar exactamente los mismos anchos.
            const widths = () => page.locator('#tbNdcfsConceptos th').evaluateAll(cells => cells.map(cell => Math.round(cell.getBoundingClientRect().width)));
            await page.evaluate(() => ndcfsTest.seed([]));
            const emptyWidths = await widths();
            await page.evaluate(() => ndcfsTest.seed([{concepto:'CORTO', cantidad:1, netoGravado:1, alicuotaIva:0}]));
            assert.deepEqual(await widths(), emptyWidths);
            await page.evaluate(() => ndcfsTest.seed([
                {concepto:'CONCEPTO EXTENSO '.repeat(20), cantidad:1, netoGravado:100000000000, alicuotaIva:0},
                {concepto:'CANTIDAD GRANDE', cantidad:999999999999, netoGravado:.01, alicuotaIva:27}
            ]));
            assert.deepEqual(await widths(), emptyWidths, 'Los importes grandes desplazaron las columnas');
            assert.deepEqual((await widths()).slice(1), [190,72,190,120,210,64]);
            const large = await page.locator('#tbNdcfsConceptos').evaluate(table => {
                const rows = [...table.tBodies[0].rows];
                const description = rows[0].querySelector('.ndcfs-descripcion span');
                return {
                    numbersFull: rows.every(row => [...row.querySelectorAll('.text-end')].every(cell => cell.scrollWidth <= cell.clientWidth)),
                    ellipsis: getComputedStyle(description).textOverflow === 'ellipsis' && description.scrollWidth > description.clientWidth,
                    tooltip: description.title,
                    total: rows[0].cells[5].textContent.trim(),
                    internalScroll: table.parentElement.scrollWidth > table.parentElement.clientWidth,
                    pageOverflow: document.documentElement.scrollWidth > innerWidth
                };
            });
            assert.equal(large.numbersFull, true, 'Se recorto un importe o cantidad');
            assert.equal(large.ellipsis, true);
            assert.equal(large.tooltip, 'CONCEPTO EXTENSO '.repeat(20));
            assert.match(large.total, /100,000,000,000\.00/);
            assert.equal(large.pageOverflow, false);
            if (viewport.width <= 1024) assert.equal(large.internalScroll, true);
            if (output) await page.screenshot({path:path.join(output, `ndcfs-importes-grandes-${viewport.width}.png`)});
            console.log(`OK anchos estables e importes de 100.000 millones: ${viewport.width}px.`);
            await page.evaluate(() => {
                ndcfsTest.renderSubtotales({subtotales:Array.from({length:30},(_,i) => ({concepto:'Subtotal '+i+' descripcion '.repeat(15),importe:1234567.89}))});
                bootstrap.Modal.getInstance(document.getElementById('modalNdcfsConceptos')).hide();
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalNdcfsCalculo')).show();
            });
            await page.waitForTimeout(350);
            assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
            const calc = await page.locator('#tbNdcfsSubtotales').evaluate(table => ({
                numbersFull:[...table.querySelectorAll('td.text-end')].every(el => el.scrollWidth <= el.clientWidth),
                bottom:table.querySelector('tfoot td').getBoundingClientRect().bottom,
                footer:document.querySelector('#modalNdcfsCalculo .modal-footer').getBoundingClientRect().top
            }));
            assert.equal(calc.numbersFull, true);
            if (viewport.width >= 768) assert.ok(calc.bottom <= calc.footer + 1, JSON.stringify(calc));
            if (output) await page.screenshot({path:path.join(output, `ndcfs-calculo-${viewport.width}.png`)});
            assert.deepEqual(errors, []);
            console.log(`OK ND/NC/FS: ${viewport.width}x${viewport.height}, filas compactas, origen NC, importes, total, tooltip y eliminar.`);
            await page.close();
        }
        for (const outcome of ['rechazo', 'exito', 'incierto']) {
            const page = await browser.newPage({viewport:{width:1366,height:768}});
            await page.route('**/*', route => {
                const url = new URL(route.request().url());
                if (url.pathname === '/') return route.fulfill({contentType:'text/html',body:html});
                const asset = path.resolve(root,'wwwroot','.'+url.pathname);
                if (!asset.startsWith(path.join(root,'wwwroot')+path.sep) || !fs.existsSync(asset)) return route.abort();
                return route.fulfill({body:fs.readFileSync(asset),contentType:asset.endsWith('.css')?'text/css':'application/octet-stream'});
            });
            await page.goto('https://ndcfs.test/');
            await page.evaluate(() => {
                $('#cmbNdcfsOperacion').html('<option value="ND">Nota de Debito</option>');
                $('#cmbNdcfsIva').html('<option value="21">21.00</option>');
                ndcfsTest.init(); ndcfsTest.seed([]);
                window.requests = []; window.messages = [];
                window.AbrirMensaje = (...args) => { messages.push(args[1]); window.messageCallback = args[2]; };
                window.ndcfsCalcularConceptosUrl = '/calcular'; window.ndcfsConfirmarOperacionUrl = '/confirmar';
                $.ajax = options => { window.deferred = $.Deferred(); requests.push(options); return deferred.promise(); };
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalNdcfsConceptos')).show();
            });
            for (const [text,neto,qty] of [['publicidad','5000','1'],['Otra Publicidad','60000','5'],['publicidad gondola','1223.33','4']]) {
                await page.locator('#txtNdcfsConcepto').fill(text);
                await page.locator('#txtNdcfsNeto').fill(neto);
                await page.locator('#txtNdcfsCantidad').fill(qty);
                await page.locator('#btnNdcfsAgregarConcepto').click();
            }
            assert.deepEqual(await page.locator('#tbNdcfsConceptos th').allTextContents(), ['Concepto','Neto Unitario','IVA %','P. Venta Unitario','Cantidad','Precio Total','Accion']);
            const rows = await page.locator('#tbodyNdcfsConceptos tr').evaluateAll(rows => rows.map(row => [...row.cells].map(cell=>cell.textContent.trim())));
            assert.equal(rows[0][0],'PUBLICIDAD'); assert.equal(rows[1][0],'OTRA PUBLICIDAD');
            assert.match(rows[1][3], /72,600\.00/); assert.equal(rows[1][4],'5'); assert.match(rows[1][5],/363,000\.00/);
            assert.match(rows[2][3], /1,480\.23/); assert.match(rows[2][5], /5,920\.92/);
            assert.match(await page.locator('#lblNdcfsTotalConceptos').innerText(),/374,970\.92/);
            await page.locator('#btnNdcfsSeguirConceptos').click();
            const payload = await page.evaluate(() => JSON.parse(requests[0].data));
            assert.equal(payload.conceptos[1].cantidad,5); assert.equal(payload.conceptos[1].concepto,'OTRA PUBLICIDAD');
            await page.evaluate(() => deferred.resolve({ok:false,mensaje:'Existen registros de productos de Cantidades por precio de venta distinto del total'}));
            assert.match(await page.locator('#ndcfsMensajeConcepto').innerText(),/Cantidades por precio/);
            assert.equal(await page.locator('#btnNdcfsSeguirConceptos').isEnabled(),true);
            assert.equal(await page.locator('#modalNdcfsCalculo').isVisible(),false);
            assert.match(await page.evaluate(() => messages.at(-1)),/Cantidades por precio/);
            await page.evaluate(() => {
                requests = []; messages = []; ndcfsTest.readyToConfirm();
                bootstrap.Modal.getInstance(document.getElementById('modalNdcfsConceptos')).hide();
                bootstrap.Modal.getOrCreateInstance(document.getElementById('modalNdcfsCalculo')).show();
                $('#btnNdcfsFinalizar').trigger('click').trigger('click');
            });
            assert.equal(await page.evaluate(() => messages.length),1,'Doble dialogo de confirmacion');
            await page.evaluate(() => { messageCallback('SI'); messageCallback('SI'); ndcfsTest.confirm(); });
            assert.equal(await page.evaluate(() => requests.length),1,'Doble envio');
            assert.equal(await page.locator('#ndcfsEsperaFinalizacion').isVisible(),true);
            assert.equal(await page.locator('#modalNdcfsCalculo').evaluate(el=>el.inert),true);
            assert.equal(await page.locator('#modalNdcfsConceptos').evaluate(el=>el.inert),true);
            await page.keyboard.press('Enter'); await page.keyboard.press('Tab');
            assert.equal(await page.evaluate(() => requests.length),1);
            if (output && outcome === 'exito') await page.screenshot({path:path.join(output,'ndcfs-bloqueo.png')});
            await page.evaluate(outcome => {
                if (outcome === 'incierto') deferred.reject({status:0});
                else deferred.resolve(outcome === 'exito' ? {ok:true,mensaje:'Operacion confirmada',debe_imprimir:false} : {ok:false,mensaje:'Rechazo del SP'});
            }, outcome);
            assert.equal(await page.locator('#ndcfsEsperaFinalizacion').count(),0);
            assert.equal(await page.locator('#modalNdcfsCalculo').evaluate(el=>el.inert),false);
            assert.equal(await page.locator('#btnNdcfsFinalizar').isEnabled(),outcome==='rechazo');
            if (outcome !== 'rechazo') {
                await page.evaluate(() => ndcfsTest.confirm());
                assert.equal(await page.evaluate(() => requests.length),1,'Se pudo reenviar una emision');
            }
            console.log(`OK funcional: ejemplo tester, mayusculas, error calculo, bloqueo/doble envio y ${outcome}.`);
            await page.close();
        }
    } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
