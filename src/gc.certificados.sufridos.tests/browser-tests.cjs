// Navegador real, endpoints simulados: prueba los assets instalados sin SQL.
// NODE_PATH debe apuntar a los paquetes del runtime que contiene playwright.
const fs = require('node:fs'), path = require('node:path'), assert = require('node:assert/strict');
const { chromium } = require('playwright');
const repo = path.resolve(__dirname, '..'), web = path.join(repo, 'gc.sitio/wwwroot');
const out = path.resolve(repo, '.artifacts/certificados-sufridos/qa');
const payload = JSON.parse(fs.readFileSync(path.join(out, 'consulta-sintetica.json'), 'utf8'));
const payloadPorCuit = JSON.parse(fs.readFileSync(path.join(out, 'consulta-por-CUIT-sintetica.json'), 'utf8'));
const year = /Período (\d{4})/.exec(payload.contexto)[1];
let available = true, estado = 200, gate = null, queries = [], pdfs = [], checks = 0, mismatch = false, empty = false;
function check(value, text) { assert.ok(value, text); checks++; console.log('OK: ' + text); }
function html() {
    let filtro = fs.readFileSync(path.join(repo, 'gc.sitio/Areas/Libros/Views/CertificadosSufridos/_Filtro.cshtml'), 'utf8');
    filtro = filtro.slice(filtro.indexOf('<form'));
    filtro = filtro.replace('@Html.AntiForgeryToken()', '<input type="hidden" name="__RequestVerificationToken" value="sintetico" />');
    filtro = filtro.replace(/disabled="@\(ViewBag.Error != null\)"/g, available ? '' : 'disabled');
    filtro = filtro.replace(/disabled="@\(Model.Impuesto != "IB"\)"/g, '');
    const select = (id, values, selected) => {
        const regex = new RegExp('(<select[^>]*id="' + id + '"[^>]*>)[\\s\\S]*?(<\\/select>)');
        filtro = filtro.replace(regex, (_, a, b) => a + values.map(([v,t]) => `<option value="${v}" ${v === selected ? 'selected' : ''}>${t}</option>`).join('') + b);
    };
    select('suf-impuesto', [['IB','Ingresos Brutos (IB)'],['GA','Ganancias (GA)']], 'IB');
    select('suf-periodo', Array.from({ length: 6 }, (_, i) => [String(+year-i),String(+year-i)]), year);
    select('suf-mes', Array.from({ length: 12 }, (_, i) => [String(i+1),['Enero','Febrero','Marzo','Abril','Mayo','Junio','Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre'][i]]), '2');
    let index = fs.readFileSync(path.join(repo, 'gc.sitio/Areas/Libros/Views/CertificadosSufridos/Index.cshtml'), 'utf8');
    index = index.slice(index.indexOf('<section'), index.indexOf('@section scripts'));
    index = index.replace(/<partial[^>]*\/>/, filtro).replace(/@Url.Action\("Consultar"\)/g, '/consultar').replace(/@Url.Action\("Pdf"\)/g, '/pdf')
        .replace(/@\(ViewBag.Error == null \? "true" : "false"\)/g, String(available)).replace(/disabled="@\(ViewBag.Error != null\)"/g, available ? '' : 'disabled');
    check(!index.includes('@'), 'Fixture usa el DOM de las vistas sin instrucciones Razor residuales');
    const css = ['/lib/bootstrap/css/bootstrap.css','/css/core.css','/css/golden-overlay.css','/css/site.css','/css/components/gc-filter.css','/css/components/gc-pager.css','/css/modules/certificados-sufridos.css'];
    return '<!doctype html><html lang="es"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">' + css.map(u=>`<link rel="stylesheet" href="${u}">`).join('') +
        '</head><body><header style="padding:12px;background:#fbf4df"><strong>GECO · Certificados Sufridos · Prueba sintética</strong></header><main>' + index +
        '</main><footer style="height:24px">Prueba de interfaz sin conexión a SQL</footer><script src="/js/components/gc-pager.js"></script><script src="/js/app/libros/certificados-sufridos.js"></script></body></html>';
}
(async () => {
    const browser = await chromium.launch({ headless: true });
    try {
        const context = await browser.newContext({ acceptDownloads: true, viewport: { width: 1366, height: 900 } });
        const page = await context.newPage(), errors = [];
        page.on('pageerror', error => errors.push(error.message));
        await page.route('http://geco.test/**', async route => {
            const u = new URL(route.request().url());
            if (u.pathname === '/') return route.fulfill({ contentType: 'text/html', body: html() });
            if (u.pathname === '/consultar') {
                queries.push(route.request().postData()); if (gate) await gate;
                const agrupado = route.request().postData().includes('name="AgrupadoPorCuit"');
                const body = structuredClone(agrupado ? payloadPorCuit : payload); body.filas[0].celdas[agrupado ? 1 : 5] = '<img src=x onerror=alert(1)>';
                if (mismatch) body.agrupadoPorCuit = !body.agrupadoPorCuit;
                if (empty) { body.filas = []; body.grupos = []; body.total = 0; body.totalImporte = body.totalFinal = '0,00'; body.registrosOrigen = agrupado ? '0' : null; }
                return route.fulfill({ status: estado, contentType: 'application/json', body: JSON.stringify(estado === 200 ? body : { mensaje: 'Error sintético recuperable' }) });
            }
            if (u.pathname === '/pdf') { const body = route.request().postData(); pdfs.push(body); return route.fulfill({ contentType: 'application/pdf', body: fs.readFileSync(path.join(out, body.includes('name="AgrupadoPorCuit"') ? 'sufridos-IB-por-CUIT-sintetico.pdf' : 'sufridos-IB-sintetico.pdf')) }); }
            const target = path.resolve(web, '.' + u.pathname);
            if (!target.startsWith(web + path.sep) || !fs.existsSync(target) || !fs.statSync(target).isFile()) return route.fulfill({ status: 404, body: '' });
            return route.fulfill({ contentType: target.endsWith('.css') ? 'text/css' : target.endsWith('.js') ? 'application/javascript' : 'application/octet-stream', body: fs.readFileSync(target) });
        });
        await page.goto('http://geco.test/');
        check(queries.length === 0 && await page.locator('#btnDetalle').isDisabled() && await page.locator('#suf-pdf').isDisabled(), 'Entrada sin consultas automáticas ni acciones habilitadas');
        check(!await page.locator('#suf-agrupar').isDisabled(), 'Agrupado por CUIT disponible');
        await page.locator('#suf-separar').check(); await page.locator('#suf-agrupar').check();
        check(await page.locator('#suf-separar').isDisabled() && !await page.locator('#suf-separar').isChecked(), 'Modo agrupado limpia corte no disponible');
        await page.locator('#suf-agrupar').uncheck();
        await page.locator('#suf-separar').check();
        let release; gate = new Promise(resolve => release = resolve);
        await page.locator('button[type="submit"]').click(); await page.waitForFunction(() => document.querySelector('#suf-panel').getAttribute('aria-busy') === 'true');
        check(await page.locator('#suf-campos').evaluate(e => e.disabled) && await page.locator('#suf-cargando').isVisible(), 'Spinner y bloqueo durante consulta');
        await page.locator('#suf-filtros').dispatchEvent('submit'); check(queries.length === 1, 'Submit repetido no duplica consulta'); release(); gate = null;
        await page.waitForFunction(() => document.querySelector('#suf-panel').getAttribute('aria-busy') === 'false');
        check(await page.locator('#suf-filas tr:not(.suf-grupo)').count() === 100 && await page.locator('#suf-filas .suf-grupo').count() === 1, 'Primera página limitada a 100 y encabezado del grupo');
        check(await page.locator('#suf-filas').innerText().then(t=>t.includes('<img src=x')) && await page.locator('#suf-filas img').count() === 0, 'Datos no se interpretan como HTML');
        const total = await page.locator('#suf-total').innerText();
        await page.screenshot({ path: path.join(out, 'pantalla-escritorio.png'), fullPage: true });
        await page.locator('#suf-paginador [aria-label="Página 2"]').click();
        check(await page.locator('#suf-filas tr:not(.suf-grupo)').count() === 80 && queries.length === 1 && await page.locator('#suf-total').innerText() === total, 'Página 2 local, 80 filas y totales de consulta intactos');
        check((await page.locator('#suf-filas .suf-grupo').innerText()).includes('80 registros en el grupo completo'), 'Total del grupo completo al continuar entre páginas');
        const downloadPromise = page.waitForEvent('download'); await page.locator('#suf-pdf').click(); const download = await downloadPromise;
        await download.saveAs(path.join(out, 'descarga-navegador.pdf')); await page.waitForFunction(() => !document.querySelector('#suf-pdf').disabled);
        check(pdfs.length === 1 && pdfs[0].includes('SepararCierre') && !pdfs[0].includes('Pagina'), 'PDF desde página intermedia conserva filtros y no envía página');
        check(download.suggestedFilename().endsWith(`${year}-02.pdf`), 'Nombre y MIME de PDF correcto');
        await page.locator('#btnFiltro').click(); await page.locator('#suf-tamanio').selectOption('50');
        check(await page.locator('#suf-filas tr:not(.suf-grupo)').count() === 50 && queries.length === 1, 'Cambio de tamaño no consulta SQL ni cambia filtros');
        await page.locator('#suf-paginador [data-gc-page-input]').fill('99'); await page.locator('#suf-paginador [data-gc-page-go]').click();
        check(await page.locator('#suf-paginador [aria-current="page"]').innerText() === '1', 'Salto inválido no confirma otra página');
        await page.locator('#suf-paginador [data-gc-page-input]').fill('4'); await page.locator('#suf-paginador [data-gc-page-input]').press('Enter');
        check(await page.locator('#suf-filas tr:not(.suf-grupo)').count() === 30, 'Enter llega a última página parcial');
        await page.locator('#suf-por-fecha').check(); await page.locator('#suf-desde').fill(`${year}-02-10`); await page.locator('#suf-hasta').fill(`${year}-02-01`);
        await page.locator('button[type="submit"]').click(); check(queries.length === 1, 'Rango invertido impide consulta');
        await page.locator('#suf-hasta').fill(`${year}-02-20`); await page.locator('button[type="submit"]').click();
        await page.waitForFunction(() => !document.querySelector('#suf-pdf').disabled); check(queries.length === 2, 'Corregir fecha permite consultar sin Inicializar');
        await page.locator('#btnFiltro').click(); await page.locator('#suf-impuesto').selectOption('GA');
        check(await page.locator('#suf-separar').isDisabled() && !await page.locator('#suf-separar').isChecked() && await page.locator('#suf-pdf').isDisabled(), 'Cambiar impuesto limpia corte y consulta aplicada');
        await page.locator('button[type="reset"]').click(); check(await page.locator('#suf-impuesto').inputValue() === 'IB' && !await page.locator('#suf-por-fecha').isChecked(), 'Inicializar repone filtros iniciales');
        for (const status of [400, 401, 403, 422, 503]) {
            estado = status; await page.locator('button[type="submit"]').click();
            await page.waitForFunction(() => document.querySelector('#suf-mensaje').textContent.includes('Error sintético'));
            check(await page.locator('#suf-filas tr').count() === 0 && !await page.locator('#suf-campos').evaluate(e => e.disabled), `Error ${status} limpia datos y recupera controles`);
        }
        estado = 200; await page.setViewportSize({ width: 390, height: 844 });
        check(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), 'Filtros a 390 px sin desborde horizontal del body');
        await page.screenshot({ path: path.join(out, 'pantalla-movil-filtros.png'), fullPage: true });
        await page.locator('button[type="submit"]').click(); await page.waitForFunction(() => !document.querySelector('#suf-pdf').disabled);
        check(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), 'Grilla a 390 px con scroll interno y body sin desborde');
        await page.screenshot({ path: path.join(out, 'pantalla-movil-grilla.png'), fullPage: true });
        check(errors.length === 0, 'Sin errores JavaScript del navegador');
        await page.setViewportSize({ width: 1366, height: 900 }); await page.locator('#btnFiltro').click(); await page.locator('#suf-agrupar').check();
        check(await page.locator('#suf-pdf').isDisabled(), 'Cambiar a agrupado invalida consulta y PDF anteriores');
        await page.locator('button[type="submit"]').click(); await page.waitForFunction(() => !document.querySelector('#suf-pdf').disabled);
        check(await page.locator('#suf-columnas th').count() === 8 && await page.locator('#suf-filas tr:not(.suf-grupo)').count() === 100, 'Agrupado: contrato de ocho columnas y 100 filas iniciales');
        check(await page.locator('#suf-base-bloque').isHidden() && await page.locator('#suf-lh-bloque').isHidden() && await page.locator('#suf-origen-bloque').isVisible(), 'Agrupado: no muestra base o desglose LH inexistentes');
        check(await page.locator('#suf-total').innerText() === payloadPorCuit.totalImporte && await page.locator('#suf-origen').innerText() === '540', 'Agrupado: conserva total con LH y 540 registros de origen');
        check(await page.locator('#suf-filas img').count() === 0 && (await page.locator('#suf-filas').innerText()).includes('<img src=x'), 'Agrupado: texto fiscal no se ejecuta como HTML');
        await page.locator('#suf-paginador [aria-label="Página 2"]').click();
        check(await page.locator('#suf-filas tr:not(.suf-grupo)').count() === 80 && (await page.locator('#suf-filas .suf-grupo').innerText()).includes('Registros de origen: 240'), 'Agrupado: última página y total completo del grupo, no de la página');
        await page.screenshot({ path: path.join(out, 'pantalla-agrupada-escritorio.png'), fullPage: true });
        const groupedDownloadPromise = page.waitForEvent('download'); await page.locator('#suf-pdf').click(); const groupedDownload = await groupedDownloadPromise;
        await groupedDownload.saveAs(path.join(out, 'descarga-agrupada-navegador.pdf')); await page.waitForFunction(() => !document.querySelector('#suf-pdf').disabled);
        check(groupedDownload.suggestedFilename().endsWith('_por_CUIT.pdf') && pdfs.at(-1).includes('name="AgrupadoPorCuit"') && !pdfs.at(-1).includes('name="SepararCierre"'), 'Agrupado: PDF completo con modo aplicado y sin corte');
        await page.setViewportSize({ width: 390, height: 844 });
        check(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), 'Agrupado: sin desborde horizontal a 390 px');
        await page.screenshot({ path: path.join(out, 'pantalla-agrupada-movil.png'), fullPage: true });
        await page.locator('#btnFiltro').click(); estado = 503; await page.locator('button[type="submit"]').click();
        await page.waitForFunction(() => document.querySelector('#suf-mensaje').textContent.includes('Error sintético'));
        check(await page.locator('#suf-filas tr').count() === 0 && await page.locator('#suf-pdf').isDisabled(), 'Agrupado: fallo de servidor no deja datos anteriores');
        estado = 200; empty = true; await page.locator('button[type="submit"]').click();
        await page.waitForFunction(() => !document.querySelector('#suf-vacio').hidden);
        check(await page.locator('#suf-paginador').innerHTML() === '' && await page.locator('#suf-pdf').isDisabled(), 'Agrupado: vacío sin paginación ni PDF');
        empty = false; await page.locator('#btnFiltro').click(); mismatch = true; await page.locator('button[type="submit"]').click();
        await page.waitForFunction(() => document.querySelector('#suf-mensaje').textContent.includes('respuesta no contiene'));
        check(await page.locator('#suf-filas tr').count() === 0, 'Respuesta de modo equivocado rechazada');
        mismatch = false; await page.locator('#suf-agrupar').uncheck(); await page.locator('button[type="submit"]').click(); await page.waitForFunction(() => !document.querySelector('#suf-pdf').disabled);
        check(await page.locator('#suf-columnas th').count() === 15 && await page.locator('#suf-base-bloque').isVisible(), 'Volver al detalle recupera contrato y totales de detalle');
        check(errors.length === 0, 'Sin errores JavaScript en ambos modos');
        available = false; await page.reload(); check(await page.locator('#suf-campos').evaluate(e => e.disabled) && await page.locator('#btnFiltro').isDisabled(), 'Fallo de catálogo mantiene formulario bloqueado');
        console.log(`PASS: ${checks} comprobaciones de navegador. HTTP simulado, sin SQL.`);
    } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
