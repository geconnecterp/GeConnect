(() => {
    'use strict';
    const root = document.getElementById('certificados-sufridos');
    if (!root) return;
    const id = n => document.getElementById(n), form = id('suf-filtros'), filtro = id('divFiltro'), detalle = id('divDetalle');
    const panel = id('suf-panel'), campos = id('suf-campos'), btnFiltro = id('btnFiltro'), btnDetalle = id('btnDetalle'), pdf = id('suf-pdf');
    const available = root.dataset.disponible === 'true';
    let busy = false, aplicada = null, consulta = null, pagina = 1, size = 100, frame = false;
    function mensaje(texto = '', tono = 'warning') { id('suf-mensaje').textContent = texto; id('suf-mensaje').className = texto ? 'alert alert-' + tono : ''; }
    function element(tag, texto, clase) { const e = document.createElement(tag); if (texto != null) e.textContent = texto; if (clase) e.className = clase; return e; }
    function layout() {
        if (frame) return; frame = true;
        requestAnimationFrame(() => {
            frame = false;
            const footer = document.querySelector('body > footer');
            detalle.style.height = Math.max(300, Math.floor(window.innerHeight - detalle.getBoundingClientRect().top - (footer?.getBoundingClientRect().height || 0) - 22)) + 'px';
        });
    }
    function sync() {
        campos.disabled = busy || !available;
        id('suf-fechas').disabled = !id('suf-por-fecha').checked;
        id('suf-cuit-campos').disabled = !id('suf-por-cuit').checked;
        const puedeSeparar = id('suf-impuesto').value === 'IB' && !id('suf-agrupar').checked;
        id('suf-separar').disabled = !puedeSeparar; if (!puedeSeparar) id('suf-separar').checked = false;
        btnFiltro.disabled = busy || !available; btnDetalle.disabled = busy || !consulta;
        pdf.disabled = busy || !available || !aplicada || !consulta?.total;
        btnFiltro.setAttribute('aria-expanded', String(!filtro.hidden)); btnDetalle.setAttribute('aria-expanded', String(!detalle.hidden));
        id('suf-cargando').hidden = !busy; panel.setAttribute('aria-busy', String(busy)); panel.inert = busy; layout();
    }
    function clear() {
        consulta = null; aplicada = null; pagina = 1;
        id('suf-filas').replaceChildren(); id('suf-columnas').replaceChildren(); id('suf-paginador').replaceChildren(); detalle.hidden = true;
    }
    function bounds(reset) {
        const year = Number(id('suf-periodo').value), month = Number(id('suf-mes').value);
        const start = `${year}-${String(month).padStart(2, '0')}-01`, end = `${year}-${String(month).padStart(2, '0')}-${new Date(year, month, 0).getDate()}`;
        for (const [name, value] of [['suf-desde', start], ['suf-hasta', end]]) {
            const input = id(name); input.min = start; input.max = end;
            if (reset || !input.value || input.value < start || input.value > end) input.value = value;
            input.setCustomValidity('');
        }
    }
    function validarFechas() {
        const desde = id('suf-desde').value, hasta = id('suf-hasta').value;
        id('suf-hasta').setCustomValidity(id('suf-por-fecha').checked && desde && hasta && desde > hasta ? 'Hasta no puede ser anterior a Desde.' : '');
        id('suf-cuit').setCustomValidity(id('suf-por-cuit').checked && /[%_\[\]]/.test(id('suf-cuit').value) ? 'Ingrese el CUIT exacto, sin patrones.' : '');
    }
    // Adaptador del contrato gc-pager existente; navegación sobre la respuesta completa.
    function pager(pages) {
        const nav = element('nav', null, 'gc-pager'); nav.setAttribute('data-gc-pager', ''); nav.dataset.current = String(pagina); nav.dataset.pages = String(pages); nav.setAttribute('aria-label', 'Páginas de certificados sufridos');
        const status = element('span', `Página ${pagina} de ${pages}`, 'gc-pager-status'); status.setAttribute('role', 'status'); nav.appendChild(status);
        const controles = element('div', null, 'gc-pager-controls'); nav.appendChild(controles);
        function button(text, page, label, edge = false) {
            const b = element('button', text, 'gc-pager-button' + (edge ? ' gc-pager-edge' : ''));
            b.type = 'button'; b.dataset.page = String(page); b.setAttribute('aria-label', label); controles.appendChild(b); return b;
        }
        button('«', 1, 'Primera página', true).disabled = pagina === 1; button('‹', Math.max(1, pagina - 1), 'Página anterior').disabled = pagina === 1;
        const start = Math.max(1, Math.min(pagina - 2, pages - 4)), numeros = new Set([1, pages]);
        for (let n = start; n <= Math.min(pages, start + 4); n++) numeros.add(n);
        let anterior = 0;
        for (const n of [...numeros].sort((a, b) => a - b)) {
            if (anterior && n - anterior > 1) controles.appendChild(element('span', '…', 'gc-pager-gap'));
            const b = button(String(n), n, `Página ${n}`); if (n === pagina) b.setAttribute('aria-current', 'page'); anterior = n;
        }
        button('›', Math.min(pages, pagina + 1), 'Página siguiente').disabled = pagina === pages; button('»', pages, 'Última página', true).disabled = pagina === pages;
        if (pages > 1) {
            const salto = element('div', null, 'gc-pager-jump'), label = element('label', 'Ir a página '), input = element('input', null, 'gc-pager-input');
            input.type = 'number'; input.min = '1'; input.max = String(pages); input.step = '1'; input.value = String(pagina); input.required = true; input.setAttribute('data-gc-page-input', ''); label.appendChild(input); salto.appendChild(label);
            const go = element('button', 'Ir', 'gc-pager-button'); go.type = 'button'; go.setAttribute('data-gc-page-go', ''); salto.appendChild(go); nav.appendChild(salto);
        }
        return nav;
    }
    function clase(c) { return c.numero ? 'suf-number' : c.textoLargo ? 'suf-long' : ''; }
    function render(page, focus = false) {
        if (!consulta || busy) return;
        const pages = Math.max(1, Math.ceil(consulta.total / size)); if (!Number.isInteger(page) || page < 1 || page > pages) return;
        const desde = (page - 1) * size, filas = consulta.filas.slice(desde, desde + size), fragment = document.createDocumentFragment();
        let grupoActual = null;
        for (const f of filas) {
            if (f.grupo !== grupoActual) {
                grupoActual = f.grupo; const grupo = consulta.grupos[f.grupo], tr = element('tr', null, 'suf-grupo'), cell = element('td'); cell.colSpan = consulta.columnas.length;
                cell.appendChild(element('strong', grupo.titulo + ` · ${grupo.cantidad} ${consulta.agrupadoPorCuit ? 'filas agrupadas' : 'registros'} en el grupo completo`));
                if (grupo.totalBase !== null) cell.appendChild(element('span', 'Base: ' + grupo.totalBase));
                cell.appendChild(element('span', (consulta.agrupadoPorCuit ? 'Total sufrido (incluye LH): ' : 'Retención / Percepción: ') + grupo.totalImporte));
                if (grupo.registrosOrigen !== null) cell.appendChild(element('span', 'Registros de origen: ' + grupo.registrosOrigen));
                if (grupo.totalLH !== null) { cell.appendChild(element('span', 'LH: ' + grupo.totalLH)); cell.appendChild(element('span', 'Total con LH: ' + grupo.total)); }
                tr.appendChild(cell); fragment.appendChild(tr);
            }
            const row = element('tr'); f.celdas.forEach((valor, n) => row.appendChild(element('td', valor, clase(consulta.columnas[n])))); fragment.appendChild(row);
        }
        id('suf-filas').replaceChildren(fragment); pagina = page;
        id('suf-cantidad').textContent = consulta.total ? `${desde + 1}–${desde + filas.length} de ${consulta.total} filas · ${consulta.grupos.length} grupos` : '0 filas';
        id('suf-vacio').hidden = consulta.total !== 0; id('suf-paginador').replaceChildren(...(consulta.total ? [pager(pages)] : []));
        id('suf-filas').closest('.suf-table-wrap').scrollTop = 0;
        if (focus) id('suf-paginador').querySelector('[aria-current="page"]')?.focus({ preventScroll: true }); layout();
    }
    function validarRespuesta(json, columnas, agrupado) {
        if (!Array.isArray(json.filas) || !Array.isArray(json.columnas) || !Array.isArray(json.grupos) || json.columnas.length !== columnas ||
            json.agrupadoPorCuit !== agrupado || (agrupado ? json.totalBase !== null || typeof json.registrosOrigen !== 'string' || json.totalLH !== null : typeof json.totalBase !== 'string' || json.registrosOrigen !== null) ||
            !Number.isSafeInteger(json.total) || json.total !== json.filas.length || typeof json.contexto !== 'string' ||
            ['totalImporte', 'totalFinal'].some(k => typeof json[k] !== 'string') || (json.totalLH !== null && typeof json.totalLH !== 'string') || typeof json.incompletos !== 'boolean' ||
            json.columnas.some(c => !c || typeof c.titulo !== 'string' || typeof c.numero !== 'boolean' || typeof c.textoLargo !== 'boolean') ||
            json.grupos.some((g, n) => !g || g.id !== n || typeof g.titulo !== 'string' || !Number.isSafeInteger(g.cantidad) || g.cantidad < 1 || ['totalImporte', 'total'].some(k => typeof g[k] !== 'string') ||
                (agrupado ? g.totalBase !== null || typeof g.registrosOrigen !== 'string' || g.totalLH !== null : typeof g.totalBase !== 'string' || g.registrosOrigen !== null) || (g.totalLH !== null && typeof g.totalLH !== 'string')) ||
            json.filas.some(f => !f || !Number.isSafeInteger(f.grupo) || !json.grupos[f.grupo] || !Array.isArray(f.celdas) || f.celdas.length !== columnas || f.celdas.some(v => typeof v !== 'string')))
            throw new Error('La respuesta no contiene registros válidos. No se muestran datos parciales.');
        const cantidades = new Array(json.grupos.length).fill(0); for (const fila of json.filas) cantidades[fila.grupo]++;
        if (json.grupos.some((g, n) => g.cantidad !== cantidades[n])) throw new Error('La respuesta contiene grupos incompletos.');
    }
    form.addEventListener('submit', async event => {
        event.preventDefault(); if (busy || !available) return; validarFechas(); if (!form.reportValidity()) return;
        const data = new FormData(form), requestedSize = Number(id('suf-tamanio').value), agrupado = data.get('AgrupadoPorCuit') === 'true', columnas = agrupado ? 8 : data.get('Impuesto') === 'IB' ? 15 : 12;
        clear(); mensaje(); busy = true; detalle.hidden = false; id('suf-espera-texto').textContent = 'Consultando certificados. Esta operación puede demorar; por favor, espere.'; sync();
        try {
            const response = await fetch(root.dataset.consulta, { method: 'POST', body: data, credentials: 'same-origin', cache: 'no-store' });
            if (response.redirected) throw new Error('La sesión venció. Ingrese nuevamente.');
            if (!(response.headers.get('Content-Type') || '').includes('application/json')) throw new Error('No se recibió una consulta válida. Revise su sesión.');
            const json = await response.json(); if (!response.ok) throw new Error(typeof json.mensaje === 'string' ? json.mensaje : 'No se pudo completar la consulta.'); validarRespuesta(json, columnas, agrupado);
            consulta = json; aplicada = data; size = [50, 100, 250, 500].includes(requestedSize) ? requestedSize : 100;
            const tr = element('tr'); json.columnas.forEach(c => { const th = element('th', c.titulo, clase(c)); th.scope = 'col'; tr.appendChild(th); }); id('suf-columnas').replaceChildren(tr);
            id('suf-contexto').textContent = json.contexto; id('suf-base-bloque').hidden = agrupado; id('suf-base').textContent = json.totalBase ?? ''; id('suf-total').textContent = json.totalImporte;
            id('suf-importe-etiqueta').textContent = agrupado ? 'Total sufrido (incluye LH)' : 'Retención / Percepción';
            id('suf-alcance-agrupado').hidden = !agrupado; id('suf-origen-bloque').hidden = !agrupado; id('suf-origen').textContent = json.registrosOrigen ?? '';
            id('suf-lh-bloque').hidden = id('suf-final-bloque').hidden = json.totalLH === null; id('suf-lh').textContent = json.totalLH ?? ''; id('suf-final').textContent = json.totalFinal;
            filtro.hidden = true; if (json.incompletos) mensaje('Hay valores nulos de origen: los totales afectados no están disponibles.');
        } catch (error) { clear(); filtro.hidden = false; mensaje(error instanceof TypeError ? 'No se pudo conectar. Revise la conexión y vuelva a consultar.' : error.message); }
        finally { busy = false; if (consulta) render(1); sync(); }
    });
    form.addEventListener('input', event => {
        if (busy) return; validarFechas();
        if (['suf-desde', 'suf-hasta', 'suf-cuit'].includes(event.target.id)) { clear(); mensaje(); sync(); }
    });
    form.addEventListener('change', event => {
        if (busy) return;
        if (event.target.id === 'suf-tamanio') { const value = Number(event.target.value); if ([50, 100, 250, 500].includes(value)) { size = value; if (consulta) render(1); } return; }
        if (['suf-periodo', 'suf-mes'].includes(event.target.id)) bounds(true); validarFechas(); clear(); mensaje(); sync();
    });
    form.addEventListener('reset', event => {
        event.preventDefault(); if (busy || !available) return;
        for (const input of form.querySelectorAll('input,select')) {
            if (input.tagName === 'SELECT') input.value = [...input.options].find(o => o.defaultSelected)?.value ?? input.options[0]?.value ?? '';
            else if (input.type === 'checkbox') input.checked = input.defaultChecked;
            else if (input.type !== 'hidden') input.value = input.defaultValue;
            input.setCustomValidity('');
        }
        size = 100; clear(); mensaje(); filtro.hidden = false; bounds(true); sync();
    });
    btnFiltro.addEventListener('click', () => { if (!busy && available) { filtro.hidden = consulta ? !filtro.hidden : false; sync(); } });
    btnDetalle.addEventListener('click', () => { if (!busy && consulta) { clear(); mensaje(); filtro.hidden = false; sync(); } });
    root.addEventListener('gc:page', event => { if (!busy && consulta && event.detail?.page !== pagina) render(event.detail?.page, true); });
    pdf.addEventListener('click', async () => {
        if (busy || !aplicada || !consulta?.total || !available) return;
        const pedido = aplicada; busy = true; mensaje(); id('suf-espera-texto').textContent = 'Preparando el PDF completo con los filtros aplicados. Esta operación puede demorar.'; sync();
        try {
            const response = await fetch(root.dataset.pdf, { method: 'POST', body: pedido, credentials: 'same-origin', cache: 'no-store' });
            if (response.redirected) throw new Error('La sesión venció. Ingrese nuevamente.');
            const mime = (response.headers.get('Content-Type') || '').split(';')[0].trim().toLowerCase();
            if (!response.ok) { let texto = 'No se pudo generar el reporte completo.'; if (mime === 'application/json') { const json = await response.json(); if (typeof json.mensaje === 'string') texto = json.mensaje; } throw new Error(texto); }
            if (mime !== 'application/pdf') throw new Error('No se recibió un reporte PDF válido.');
            const blob = await response.blob(); if (!blob.size) throw new Error('El reporte está vacío.');
            const url = URL.createObjectURL(blob), a = element('a'); a.href = url;
            a.download = `Certificados_Sufridos_${pedido.get('Impuesto')}_${pedido.get('Periodo')}-${String(pedido.get('Mes')).padStart(2, '0')}${pedido.get('AgrupadoPorCuit') === 'true' ? '_por_CUIT' : ''}.pdf`;
            document.body.appendChild(a); a.click(); a.remove(); setTimeout(() => URL.revokeObjectURL(url), 60000); mensaje('PDF completo preparado con los filtros aplicados.', 'success');
        } catch (error) { mensaje(error instanceof TypeError ? 'No se pudo completar el reporte. Revise la conexión.' : error.message); }
        finally { busy = false; sync(); pdf.focus(); }
    });
    window.addEventListener('resize', layout); bounds(true); sync();
})();
