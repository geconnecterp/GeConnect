(() => {
    'use strict';
    const root = document.getElementById('percepciones-emitidas');
    if (!root) return;
    const id = n => document.getElementById(n), form = id('per-filtros'), filtro = id('divFiltro'), detalle = id('divDetalle');
    const panel = id('per-panel'), campos = id('per-campos'), btnFiltro = id('btnFiltro'), btnDetalle = id('btnDetalle');
    const available = root.dataset.disponible === 'true';
    const ddjj = id('per-ddjj'), dialogo = id('per-ddjj-dialogo');
    let busy = false, confirming = false, aplicada = null, consulta = null, pagina = 1, size = 100, frame = false;
    function mensaje(texto = '', tono = 'warning') { id('per-mensaje').textContent = texto; id('per-mensaje').className = texto ? 'alert alert-' + tono : ''; }
    function layout() {
        if (frame) return;
        frame = true;
        requestAnimationFrame(() => {
            frame = false;
            const footer = document.querySelector('body > footer');
            detalle.style.height = Math.max(300, Math.floor(window.innerHeight - detalle.getBoundingClientRect().top - (footer?.getBoundingClientRect().height || 0) - 22)) + 'px';
        });
    }
    function sync() {
        campos.disabled = busy || confirming || !available;
        id('per-fechas').disabled = !id('per-por-fecha').checked;
        id('per-cuit-campos').disabled = !id('per-por-cuit').checked;
        btnFiltro.disabled = busy || confirming || !available; btnDetalle.disabled = busy || confirming || !consulta;
        // Una consulta filtrada vacía no implica que la DDJJ del mes esté vacía.
        ddjj.disabled = busy || confirming || !available || !aplicada;
        btnFiltro.setAttribute('aria-expanded', String(!filtro.hidden)); btnDetalle.setAttribute('aria-expanded', String(!detalle.hidden));
        id('per-cargando').hidden = !busy; panel.setAttribute('aria-busy', String(busy)); panel.inert = busy || confirming;
        layout();
    }
    function clear() {
        consulta = null; aplicada = null; pagina = 1;
        id('per-filas').replaceChildren(); id('per-columnas').replaceChildren(); id('per-paginador').replaceChildren(); detalle.hidden = true;
    }
    function bounds(reset) {
        const year = Number(id('per-periodo').value), month = Number(id('per-mes').value);
        const start = `${year}-${String(month).padStart(2, '0')}-01`;
        const end = `${year}-${String(month).padStart(2, '0')}-${new Date(year, month, 0).getDate()}`;
        for (const [name, value] of [['per-desde', start], ['per-hasta', end]]) {
            const input = id(name); input.min = start; input.max = end;
            if (reset || !input.value || input.value < start || input.value > end) input.value = value;
            input.setCustomValidity('');
        }
    }
    function validarFechas() {
        const desde = id('per-desde').value, hasta = id('per-hasta').value;
        id('per-hasta').setCustomValidity(id('per-por-fecha').checked && desde && hasta && desde > hasta ? 'Hasta no puede ser anterior a Desde.' : '');
    }
    function element(tag, texto, clase) {
        const e = document.createElement(tag); if (texto != null) e.textContent = texto; if (clase) e.className = clase; return e;
    }
    // Mismo contrato gc-pager de Libro IVA/Retenciones. Paginación de la respuesta completa, sin SQL adicional.
    function pager(pages) {
        const nav = element('nav', null, 'gc-pager'); nav.setAttribute('data-gc-pager', '');
        nav.dataset.current = String(pagina); nav.dataset.pages = String(pages); nav.setAttribute('aria-label', 'Páginas de percepciones');
        const status = element('span', `Página ${pagina} de ${pages}`, 'gc-pager-status'); status.setAttribute('role', 'status'); nav.appendChild(status);
        const controles = element('div', null, 'gc-pager-controls'); nav.appendChild(controles);
        function button(text, page, label, edge = false) {
            const b = element('button', text, 'gc-pager-button' + (edge ? ' gc-pager-edge' : ''));
            b.type = 'button'; b.dataset.page = String(page); b.setAttribute('aria-label', label); controles.appendChild(b); return b;
        }
        button('«', 1, 'Primera página', true).disabled = pagina === 1;
        button('‹', Math.max(1, pagina - 1), 'Página anterior').disabled = pagina === 1;
        const start = Math.max(1, Math.min(pagina - 2, pages - 4)), numeros = new Set([1, pages]);
        for (let n = start; n <= Math.min(pages, start + 4); n++) numeros.add(n);
        let anterior = 0;
        for (const n of [...numeros].sort((a, b) => a - b)) {
            if (anterior && n - anterior > 1) controles.appendChild(element('span', '…', 'gc-pager-gap'));
            const b = button(String(n), n, `Página ${n}`); if (n === pagina) b.setAttribute('aria-current', 'page'); anterior = n;
        }
        button('›', Math.min(pages, pagina + 1), 'Página siguiente').disabled = pagina === pages;
        button('»', pages, 'Última página', true).disabled = pagina === pages;
        if (pages > 1) {
            const salto = element('div', null, 'gc-pager-jump'), label = element('label', 'Ir a página '), input = element('input', null, 'gc-pager-input');
            input.type = 'number'; input.min = '1'; input.max = String(pages); input.step = '1'; input.value = String(pagina); input.required = true; input.setAttribute('data-gc-page-input', '');
            label.appendChild(input); salto.appendChild(label);
            const go = element('button', 'Ir', 'gc-pager-button'); go.type = 'button'; go.setAttribute('data-gc-page-go', ''); salto.appendChild(go); nav.appendChild(salto);
        }
        return nav;
    }
    function clase(c) { return c.numero ? 'per-number' : c.textoLargo ? 'per-long' : ''; }
    function render(page, focus = false) {
        if (!consulta || busy || confirming) return;
        const pages = Math.max(1, Math.ceil(consulta.total / size));
        if (!Number.isInteger(page) || page < 1 || page > pages) return;
        const desde = (page - 1) * size, filas = consulta.filas.slice(desde, desde + size), fragment = document.createDocumentFragment();
        for (const f of filas) {
            const row = element('tr'); f.forEach((valor, n) => row.appendChild(element('td', valor, clase(consulta.columnas[n])))); fragment.appendChild(row);
        }
        id('per-filas').replaceChildren(fragment); pagina = page;
        id('per-cantidad').textContent = consulta.total ? `${desde + 1}–${desde + filas.length} de ${consulta.total} filas` : '0 filas';
        id('per-vacio').hidden = consulta.total !== 0;
        id('per-paginador').replaceChildren(...(consulta.total ? [pager(pages)] : []));
        id('per-filas').closest('.per-table-wrap').scrollTop = 0;
        if (focus) id('per-paginador').querySelector('[aria-current="page"]')?.focus({ preventScroll: true });
        layout();
    }
    form.addEventListener('submit', async event => {
        event.preventDefault(); if (busy || confirming || !available) return;
        validarFechas();
        if (!form.reportValidity()) return;
        const data = new FormData(form), requestedSize = Number(id('per-tamanio').value);
        const columnasEsperadas = data.get('Impuesto') === 'IB' ? 16 : 14;
        clear(); mensaje(); busy = true; detalle.hidden = false;
        id('per-espera-texto').textContent = 'Consultando percepciones. Esta operación puede demorar; por favor, espere.'; sync();
        try {
            const response = await fetch(root.dataset.consulta, { method: 'POST', body: data, credentials: 'same-origin', cache: 'no-store' });
            if (response.redirected) throw new Error('La sesión venció. Ingrese nuevamente.');
            if (!(response.headers.get('Content-Type') || '').includes('application/json')) throw new Error('No se recibió una consulta válida. Revise su sesión.');
            const json = await response.json();
            if (!response.ok) throw new Error(typeof json.mensaje === 'string' ? json.mensaje : 'No se pudo completar la consulta.');
            if (!Array.isArray(json.filas) || !Array.isArray(json.columnas) || json.columnas.length !== columnasEsperadas ||
                json.columnas.some(c => !c || typeof c.titulo !== 'string' || typeof c.numero !== 'boolean' || typeof c.textoLargo !== 'boolean') ||
                !Number.isSafeInteger(json.total) || json.total !== json.filas.length || typeof json.contexto !== 'string' ||
                typeof json.totalBase !== 'string' || typeof json.totalPercepcion !== 'string' || (json.totalLH !== null && typeof json.totalLH !== 'string') || typeof json.incompletos !== 'boolean' ||
                json.filas.some(f => !Array.isArray(f) || f.length !== json.columnas.length || f.some(v => typeof v !== 'string')))
                throw new Error('La respuesta no contiene registros válidos. No se muestran datos parciales.');
            // Instantánea de filas y contexto devueltos: el paginador no vuelve a leer los controles ni deduplica.
            consulta = json; size = [50, 100, 250, 500].includes(requestedSize) ? requestedSize : 100;
            aplicada = Object.freeze({ impuesto: data.get('Impuesto'), periodo: data.get('Periodo'), mes: data.get('Mes') });
            const tr = element('tr'); json.columnas.forEach(c => { const th = element('th', c.titulo, clase(c)); th.scope = 'col'; tr.appendChild(th); }); id('per-columnas').replaceChildren(tr);
            id('per-contexto').textContent = json.contexto;
            const importe = n => n === 'No disponible' ? n : '$ ' + n;
            id('per-base').textContent = importe(json.totalBase); id('per-total').textContent = importe(json.totalPercepcion);
            id('per-lh-bloque').hidden = json.totalLH == null; id('per-lh').textContent = json.totalLH == null ? '' : importe(json.totalLH);
            filtro.hidden = true;
            if (json.incompletos) mensaje('Hay importes nulos en el origen: los totales afectados no están disponibles. No se reemplazaron por cero.');
        } catch (error) { clear(); filtro.hidden = false; mensaje(error instanceof TypeError ? 'No se pudo conectar. Revise la conexión y vuelva a consultar.' : error.message); }
        finally { busy = false; if (consulta) render(1); sync(); }
    });
    // La validación nativa ocurre antes de submit: actualizar el error al editar,
    // no esperar a un submit que el propio error anterior puede impedir.
    form.addEventListener('input', event => {
        if (!busy && !confirming && ['per-desde', 'per-hasta'].includes(event.target.id)) validarFechas();
    });
    form.addEventListener('change', event => {
        if (busy || confirming) return;
        if (event.target.id === 'per-tamanio') {
            const value = Number(event.target.value); if ([50, 100, 250, 500].includes(value)) { size = value; if (consulta) render(1); } return;
        }
        if (['per-periodo', 'per-mes'].includes(event.target.id)) bounds(true);
        if (['per-desde', 'per-hasta', 'per-por-fecha'].includes(event.target.id)) validarFechas();
        clear(); mensaje(); sync();
    });
    form.addEventListener('reset', event => {
        event.preventDefault(); if (busy || confirming || !available) return;
        for (const input of form.querySelectorAll('input,select')) {
            if (input.tagName === 'SELECT') input.value = [...input.options].find(o => o.defaultSelected)?.value ?? input.options[0]?.value ?? '';
            else if (input.type === 'checkbox') input.checked = input.defaultChecked;
            else if (input.type !== 'hidden') input.value = input.defaultValue;
            input.setCustomValidity('');
        }
        size = 100; clear(); mensaje(); filtro.hidden = false; bounds(true); sync();
    });
    btnFiltro.addEventListener('click', () => { if (!busy && !confirming && available) { filtro.hidden = consulta ? !filtro.hidden : false; sync(); } });
    btnDetalle.addEventListener('click', () => { if (!busy && !confirming && consulta) { clear(); mensaje(); filtro.hidden = false; sync(); } });
    root.addEventListener('gc:page', event => { if (!busy && !confirming && consulta && event.detail?.page !== pagina) render(event.detail?.page, true); });
    ddjj.addEventListener('click', () => {
        if (busy || confirming || !aplicada || !available) return;
        id('per-ddjj-periodo').textContent = `Impuesto ${aplicada.impuesto} · Libro IVA ${aplicada.periodo}-${String(aplicada.mes).padStart(2, '0')} · Excel completo`;
        confirming = true; dialogo.showModal(); sync();
    });
    dialogo.addEventListener('close', () => { confirming = false; sync(); if (!busy) ddjj.focus(); });
    id('per-ddjj-cancelar').addEventListener('click', () => dialogo.close());
    id('per-ddjj-confirmar').addEventListener('click', async () => {
        if (busy || !confirming || !aplicada) return;
        const pedido = aplicada, data = new FormData();
        data.append('__RequestVerificationToken', form.querySelector('input[name="__RequestVerificationToken"]').value);
        data.append('Impuesto', pedido.impuesto); data.append('Periodo', pedido.periodo); data.append('Mes', pedido.mes);
        busy = true; confirming = false; dialogo.close(); mensaje();
        id('per-espera-texto').textContent = 'Preparando la DDJJ mensual completa. Esta operación puede demorar; la descarga comenzará cuando el archivo esté listo.'; sync();
        try {
            const response = await fetch(root.dataset.ddjj, { method: 'POST', body: data, credentials: 'same-origin', cache: 'no-store' });
            if (response.redirected) throw new Error('La sesión venció. Ingrese nuevamente.');
            const mime = (response.headers.get('Content-Type') || '').split(';')[0].trim().toLowerCase();
            if (!response.ok) {
                let text = [401, 403].includes(response.status) ? 'La sesión venció o el perfil no tiene permiso para descargar DDJJ.' : 'No se pudo preparar la DDJJ mensual. No se entregó un archivo parcial.';
                if (mime === 'application/json') { const json = await response.json(); if (typeof json.mensaje === 'string') text = json.mensaje; }
                throw new Error(text);
            }
            if (mime !== 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet') throw new Error('No se recibió el formato Excel de DDJJ esperado.');
            const blob = await response.blob();
            if (!blob.size) throw new Error('La descarga está vacía.');
            const url = URL.createObjectURL(blob), a = element('a');
            a.href = url; a.download = `DDJJ_Percepciones_${pedido.impuesto}_${pedido.periodo}-${String(pedido.mes).padStart(2, '0')}.xlsx`;
            document.body.appendChild(a); a.click(); a.remove(); setTimeout(() => URL.revokeObjectURL(url), 60000);
            mensaje('Descarga preparada: DDJJ de ' + pedido.impuesto + ', mes completo ' + pedido.periodo + '-' + String(pedido.mes).padStart(2, '0') + '.', 'success');
        } catch (error) { mensaje(error instanceof TypeError ? 'No se pudo completar la descarga. Revise la conexión e intente nuevamente.' : error.message); }
        finally { busy = false; sync(); ddjj.focus(); }
    });
    window.addEventListener('resize', layout); bounds(true); sync();
})();
