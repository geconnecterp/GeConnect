// Prueba aislada: no conecta con Caja/API ni ejecuta operaciones reales.
const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const { chromium } = require(process.argv[2] || 'playwright');
const output = process.argv[3];
const root = path.join(__dirname, '..');
const read = p => fs.readFileSync(path.join(root, p), 'utf8');
function extract(source, name) {
    const match = source.match(new RegExp(`^function ${name}\\([^]*?^}`, 'm'));
    assert.ok(match, name);
    return match[0];
}
const markup = read('Areas/Facturacion/Views/ProductoFact/_facturaEmitidaModal.cshtml')
    .replace('@Html.AntiForgeryToken()', '<input type="hidden" name="__RequestVerificationToken" value="TEST-TOKEN">');
const site = read('wwwroot/js/app/siteGen.js');
const html = `<!doctype html><html lang="es"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
${['lib/bootstrap/dist/css/bootstrap.min.css', 'css/site.css', 'css/golden-overlay.css', 'lib/virtual-keyboard/css/virtual-keyboard.css', 'css/facturaEmitida.css'].map(p => `<style>${read('wwwroot/' + p)}</style>`).join('')}
</head><body><div class="modal fade modal-golden" id="modalProductosFactura" tabindex="-1"><div class="modal-dialog modal-fullscreen"><div class="modal-content"><div class="modal-header bg-gradient-green"><h4>Facturaci&oacute;n</h4></div><div class="modal-body"><button id="btnFacturaEmitida" class="btn btn-golden" onclick="cargarFacturaEmitida()">Factura emitida</button><input id="txtCodigoProducto"><div id="mensajeEstadoProducto"></div></div></div></div></div>${markup}
<script>${read('wwwroot/lib/jquery/dist/jquery.min.js')}</script>
<script>${read('wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js')}</script>
<script>${read('wwwroot/lib/virtual-keyboard/js/virtual-keyboard.js')}</script>
<script>
let modoBloqueoGrilla = '', origenCargaActual = 'directo', cajaAcumulaProductos = true;
window.cargados = []; window.ObtenerTiposFacturaEmitidaUrl = '/tipos'; window.CargarFacturaEmitidaUrl = '/cargar';
function agregarProductoAGrilla(p) { window.cargados.push(p); return {accion:'agregado'}; }
function mostrarMensajeEstado(texto,nivel) { $('#mensajeEstadoProducto').text(texto); }
${extract(site, 'posicionarTecladoVirtual')}
${extract(site, 'cerrarTecladoDigital')}
</script><script>${read('wwwroot/js/app/factFacturaEmitida.js')}</script>
<script>$(function(){bootstrap.Modal.getOrCreateInstance(document.getElementById('modalProductosFactura')).show();});</script>
</body></html>`;

(async () => {
    const browser = await chromium.launch({ headless: true });
    try {
        for (const viewport of [{width:1440,height:1000}, {width:390,height:844}]) {
            const page = await browser.newPage({ viewport });
            const errors = [];
            page.on('pageerror', e => errors.push(e.message));
            let requests = 0;
            let mode = 'ok';
            let payload;
            await page.route('**/*', async route => {
                const url = new URL(route.request().url());
                if (url.pathname === '/') return route.fulfill({contentType:'text/html',body:html});
                if (url.pathname === '/tipos') return route.fulfill({json:{ok:true,datos:[{tco_id:'081',tco_desc:'Ticket Factura A'}]}});
                if (url.pathname === '/cargar') {
                    requests++;
                    payload = route.request().postDataJSON();
                    assert.equal(route.request().headers()['requestverificationtoken'], 'TEST-TOKEN');
                    await new Promise(resolve => setTimeout(resolve, 150));
                    return route.fulfill({json: mode === 'ok' ? {
                        ok:true, acumula:true, productos:[{p_id:'TEST',cantidad_tot:2,respuesta:0}],
                        errores:['TEST-2: PRODUCTO INACTIVO'], mensaje:'Factura 0002-00001234, repeticion 2: 1 productos cargados.'
                    } : {ok:false,mensaje:'No se encontro la factura emitida.'}});
                }
                return route.fulfill({status:204});
            });
            await page.goto('https://factura-emitida.test/');
            await page.locator('#btnFacturaEmitida').click();
            await page.locator('#ddlFacturaEmitidaTipo').selectOption('081');
            await page.locator('#txtFacturaEmitidaPv').fill('2');
            await page.waitForFunction(() => document.querySelector('#virtual-keyboard')?.dataset.type === 'integer');
            assert.equal(await page.locator('#virtual-keyboard').isVisible(), true);
            await page.locator('#virtual-keyboard [data-key="ENTER"]').click();
            assert.equal(await page.evaluate(() => document.activeElement.id), 'txtFacturaEmitidaNumero');
            await page.locator('#txtFacturaEmitidaNumero').fill('1234');
            for (const id of ['txtFacturaEmitidaPv', 'txtFacturaEmitidaNumero']) {
                assert.equal(await page.locator('#' + id).evaluate(input => getComputedStyle(input).textAlign), 'center');
            }
            await page.waitForFunction(() => {
                const t = document.querySelector('#virtual-keyboard').getBoundingClientRect();
                return t.left >= 0 && t.right <= window.innerWidth && t.bottom <= window.innerHeight;
            });
            const noOverlap = await page.evaluate(() => {
                const t = document.querySelector('#virtual-keyboard').getBoundingClientRect();
                const c = document.querySelector('#modalFacturaEmitida .modal-footer').getBoundingClientRect();
                return !(t.left < c.right && t.right > c.left && t.top < c.bottom && t.bottom > c.top);
            });
            assert.equal(noOverlap, true, 'Teclado tapa las acciones');
            assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
            if (output) await page.screenshot({path:path.join(output, `factura-emitida-${viewport.width}.png`)});
            await page.locator('#virtual-keyboard [data-key="ENTER"]').click();
            await page.waitForFunction(() => document.querySelector('#modalProductosFactura').classList.contains('show'));
            assert.equal(requests, 1);
            assert.deepEqual(payload, {tcoId:'081',puntoVenta:'0002',numero:'00001234'});
            assert.equal(await page.evaluate(() => window.cargados.length), 1);
            await page.waitForFunction(() => document.querySelector('#mensajeEstadoProducto').textContent.includes('PRODUCTO INACTIVO'));
            assert.match(await page.locator('#mensajeEstadoProducto').innerText(), /PRODUCTO INACTIVO/);
            assert.equal(await page.locator('#virtual-keyboard').isVisible(), false);
            // Cancelar conserva los productos y no consulta el SP de productos.
            await page.locator('#btnFacturaEmitida').click();
            await page.locator('#ddlFacturaEmitidaTipo').selectOption('081');
            await page.locator('#modalFacturaEmitida .modal-footer [data-factura-emitida-cancelar]').click();
            await page.waitForFunction(() => document.querySelector('#modalProductosFactura').classList.contains('show'));
            assert.equal(requests, 1);
            assert.equal(await page.evaluate(() => window.cargados.length), 1);
            // Factura inexistente: permanece en el modal y permite corregir.
            mode = 'error';
            await page.locator('#btnFacturaEmitida').click();
            await page.locator('#ddlFacturaEmitidaTipo').selectOption('081');
            await page.locator('#txtFacturaEmitidaPv').fill('2');
            await page.locator('#txtFacturaEmitidaNumero').fill('99');
            await page.locator('#txtFacturaEmitidaNumero').press('Enter');
            await page.waitForFunction(() => document.querySelector('#estadoFacturaEmitida').textContent.includes('No se encontro'));
            assert.equal(await page.locator('#modalFacturaEmitida').isVisible(), true);
            assert.equal(await page.locator('#btnCargarFacturaEmitida').isEnabled(), true);
            assert.equal(await page.evaluate(() => window.cargados.length), 1);
            assert.deepEqual(errors, []);
            console.log(`OK: modal, Enter digital/fisico, carga, cancelacion, error y layout ${viewport.width}x${viewport.height}.`);
            await page.close();
        }
    } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
