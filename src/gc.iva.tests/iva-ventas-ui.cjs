// Prueba de interacción aislada: JS/CSS reales con respuestas sintéticas, sin GECO ni SQL.
// NODE_PATH debe apuntar a un runtime que disponga de playwright; usa Edge instalado.
const { chromium } = require('playwright');
const path = require('node:path');
const assert = require('node:assert/strict');
(async () => {
    const browser = await chromium.launch({ channel: 'msedge', headless: true });
    try {
        const page = await browser.newPage();
        const year = new Date().getFullYear();
        let mode = 'ok', calls = [], downloads = 0;
        const html = `<!doctype html><html><body><section id="iva-ventas" class="iva-ventas" data-consulta="/consultar" data-excel="/excel">
        <button id="btnFiltro" type="button">Filtros</button><button id="btnDetalle" type="button" disabled>Cerrar consulta</button><button type="button" id="iva-exportar" disabled>Exportar Excel</button>
        <form id="iva-filtros" novalidate><input name="__RequestVerificationToken" value="synthetic-token" type="hidden"><fieldset><div id="divFiltro">
        <select name="Periodo"><option>${year}</option></select><select name="Mes"><option value="2">Febrero</option><option value="3">Marzo</option></select>
        <select name="Registros"><option>100</option><option>500</option></select>
        <input type="checkbox" name="Fechas" value="true" data-enable="iva-fechas"><fieldset id="iva-fechas" disabled><input name="Desde" type="date" required><input name="Hasta" type="date" required></fieldset>
        <input type="checkbox" name="PorCuit" value="true" data-enable="iva-cuit"><fieldset id="iva-cuit" disabled><input name="Cuit" maxlength="15" required></fieldset>
        <input type="checkbox" name="PorCaja" value="true" data-enable="iva-cajas"><fieldset id="iva-cajas" disabled><select name="Cajas" multiple required><option value="0001">Caja 1</option><option value="0002">Caja 2</option></select></fieldset>
        </div><button type="reset">Inicializar</button><button type="submit">Ver</button>
        </fieldset></form><div id="iva-mensaje" role="status"></div><div id="divDetalle" class="iva-detail" hidden><div id="iva-resultado"></div><div id="iva-cargando" class="iva-loading" role="status" hidden><span class="spinner-border"></span><span id="iva-cargando-texto"></span></div></div></section></body></html>`;
        const grid = `<section class="iva-panel iva-datos" data-total="501"><div class="iva-table-wrap"><table class="iva-table"><tr><th>Total</th></tr><tr><td class="iva-importe">123.456.789,12</td></tr></table></div><nav class="gc-pager" data-gc-pager data-current="1" data-pages="6"><button class="gc-pager-button" data-page="1" aria-current="page">1</button><button class="gc-pager-button" data-page="2">Siguiente</button></nav></section>`;
        await page.route('https://iva.test/**', async route => {
            const req = route.request();
            const pathname = new URL(req.url()).pathname;
            if (pathname === '/') return route.fulfill({ contentType: 'text/html', body: html });
            calls.push({ path: pathname, data: req.postData() || '' });
            await new Promise(r => setTimeout(r, 120));
            if (mode === 'error') return route.fulfill({ status: 503, contentType: 'application/json', body: JSON.stringify({ mensaje: 'Fallo controlado' }) });
            if (pathname === '/excel') return route.fulfill({ contentType: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', body: Buffer.from('PK-synthetic-download') });
            return route.fulfill({ contentType: 'text/html', body: mode === 'empty' ? '<p>No se encontraron comprobantes</p>' : grid });
        });
        await page.goto('https://iva.test/');
        await page.addStyleTag({ path: path.resolve(__dirname, '../gc.sitio/wwwroot/css/modules/iva-ventas-detalle.css') });
        await page.addStyleTag({ path: path.resolve(__dirname, '../gc.sitio/wwwroot/css/components/gc-pager.css') });
        await page.addScriptTag({ path: path.resolve(__dirname, '../gc.sitio/wwwroot/js/components/gc-pager.js') });
        await page.addScriptTag({ path: path.resolve(__dirname, '../gc.sitio/wwwroot/js/app/libros/iva-ventas-detalle.js') });
        assert.equal(await page.locator('[name=Cuit]').isDisabled(), true);
        assert.equal(await page.locator('[name=Cajas]').isDisabled(), true);
        assert.equal(await page.locator('[name=Desde]').inputValue(), `${year}-02-01`);
        console.log('OK UI: filtros opcionales deshabilitados y fechas iniciales');
        await page.locator('[name=PorCaja]').check();
        await page.locator('[name=Cajas]').selectOption(['0001', '0002']);
        await page.getByRole('button', { name: 'Ver', exact: true }).click();
        assert.equal(await page.locator('#iva-exportar').isDisabled(), true);
        await page.locator('[data-total="501"]').waitFor();
        assert(calls[0].data.includes('0001') && calls[0].data.includes('0002') && calls[0].data.includes('synthetic-token'));
        await page.getByRole('button', { name: 'Siguiente' }).click();
        await page.locator('#iva-cargando').waitFor({ state: 'hidden' });
        await page.locator('[data-total="501"]').waitFor();
        assert.match(calls[1].data, /name="Pagina"\r\n\r\n2/);
        console.log('OK UI: selección múltiple, antiforgery, bloqueo durante consulta y segunda página');
        assert.equal(await page.locator('#divFiltro').isVisible(), false);
        assert.equal(await page.locator('#iva-exportar').isVisible(), true);
        await page.locator('#btnFiltro').click();
        await page.locator('[name=PorCuit]').check();
        await page.locator('[name=Cuit]').fill('abc%');
        assert.equal(await page.locator('[data-total="501"]').count(), 0);
        mode = 'empty';
        await page.getByRole('button', { name: 'Ver', exact: true }).click();
        await page.getByText('No se encontraron comprobantes', { exact: true }).waitFor();
        console.log('OK UI: cambio de filtro y conjunto vacío limpian datos anteriores');
        assert.equal(await page.locator('#iva-exportar').isDisabled(), true);
        await page.locator('#btnFiltro').click();
        mode = 'error';
        await page.getByRole('button', { name: 'Ver', exact: true }).click();
        await page.getByText('Fallo controlado', { exact: true }).waitFor();
        assert.equal(await page.locator('#iva-exportar').isDisabled(), true);
        console.log('OK UI: fallo visible, controles reactivados');
        await page.getByRole('button', { name: 'Inicializar' }).click();
        await page.waitForFunction(() => document.getElementById('iva-cajas').disabled);
        assert.equal(await page.locator('[name=PorCaja]').isChecked(), false);
        assert.equal(await page.locator('[name=Cajas]').isDisabled(), true);
        assert.match(await page.locator('#iva-mensaje').textContent(), /Seleccione el período/);
        await page.locator('[name=Fechas]').check();
        await page.locator('[name=Desde]').fill(`${year}-02-20`);
        await page.locator('[name=Hasta]').fill(`${year}-02-10`);
        const before = calls.length;
        await page.getByRole('button', { name: 'Ver', exact: true }).click();
        assert.equal(calls.length, before);
        assert.equal(await page.locator('[name=Hasta]').evaluate(el => el.validity.customError), true);
        await page.locator('[name=Hasta]').fill(`${year}-02-25`);
        assert.equal(await page.locator('[name=Hasta]').evaluate(el => el.validity.customError), false);
        await page.getByRole('button', { name: 'Inicializar' }).click();
        await page.waitForFunction(() => document.getElementById('iva-fechas').disabled);
        console.log('OK UI: inicialización y validación de rango invertido');
        mode = 'ok';
        await page.getByRole('button', { name: 'Ver', exact: true }).click();
        await page.locator('#iva-cargando').waitFor({ state: 'hidden' });
        assert.equal(await page.locator('#iva-exportar').isDisabled(), false);
        const downloadPromise = page.waitForEvent('download');
        await page.locator('#iva-exportar').click();
        const download = await downloadPromise;
        assert.equal(download.suggestedFilename(), `Libro_IVA_Ventas_${year}-02.xlsx`);
        await page.getByText(/Excel completo generado/).waitFor();
        assert.match(calls.at(-1).data, /name="Pagina"\r\n\r\n1/);
        console.log('OK UI: descarga directa y página 1 para exportar');
        await page.setViewportSize({ width: 390, height: 844 });
        const overflow = await page.evaluate(() => document.documentElement.scrollWidth > innerWidth);
        assert.equal(overflow, false);
        console.log('PASS UI: flujo sintético sin conexión a servicios reales. Sin overflow en fixture a 390 px.');
    } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
