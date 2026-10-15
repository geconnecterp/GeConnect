(() => {
    'use strict';
    const root = document.getElementById('iva-resumen');
    if (!root) return;
    const form = document.getElementById('ivr-filtros');
    const outer = form.querySelector('fieldset');
    const result = document.getElementById('ivr-resultado');
    const detail = document.getElementById('divDetalle');
    const filterButton = document.getElementById('btnFiltro');
    const detailButton = document.getElementById('btnDetalle');
    const printButton = document.getElementById('ivr-imprimir');
    const excelButton = document.getElementById('ivr-excel');
    const message = document.getElementById('ivr-mensaje');
    const loading = document.getElementById('ivr-cargando');
    const unavailable = outer.disabled;
    let busy = false, filterOpen = true, applied = null, hasRows = false, layoutPending = false;
    const initial = 'Seleccione año y mes y presione Ver.';
    function notify(text, warning = false) {
        message.textContent = text;
        message.className = text ? (warning ? 'alert alert-warning' : 'alert alert-info') : '';
    }
    function layout() {
        if (layoutPending) return;
        layoutPending = true;
        requestAnimationFrame(() => {
            layoutPending = false;
            if (detail.hidden) return;
            const footer = document.querySelector('body > footer');
            const h = Math.max(280, Math.floor(window.innerHeight - detail.getBoundingClientRect().top - (footer?.getBoundingClientRect().height || 0) - 20));
            detail.style.height = `${h}px`;
        });
    }
    function sync() {
        form.hidden = !filterOpen;
        detail.hidden = filterOpen || (!applied && !busy);
        filterButton.disabled = unavailable || busy;
        detailButton.disabled = unavailable || busy || !applied;
        printButton.disabled = excelButton.disabled = unavailable || busy || !applied || !hasRows;
        outer.disabled = unavailable || busy;
        filterButton.setAttribute('aria-expanded', String(filterOpen));
        detailButton.setAttribute('aria-expanded', String(!detail.hidden));
        loading.hidden = !busy;
        result.setAttribute('aria-busy', String(busy));
        layout();
    }
    function clear(text) {
        applied = null; hasRows = false; filterOpen = true;
        result.replaceChildren();
        ReporteResetCeldaEnArre(103);
        sync(); notify(text);
    }
    async function failure(response) {
        if ([401, 403].includes(response.status)) return 'La sesión venció o el perfil no tiene permiso para Consulta Resumen.';
        try { const data = await response.json(); if (typeof data.mensaje === 'string') return data.mensaje; } catch { }
        return 'No se pudo completar la operación. Verifique la conexión e intente nuevamente.';
    }
    async function run(excel) {
        if (busy || unavailable || (excel && (!applied || !hasRows))) return;
        if (!excel && !form.reportValidity()) return;
        const data = new FormData();
        const source = excel ? applied : new FormData(form);
        source.forEach((v, k) => data.append(k, v));
        const year = data.get('Periodo'), month = String(data.get('Mes')).padStart(2, '0');
        if (!excel) { result.replaceChildren(); applied = null; hasRows = false; ReporteResetCeldaEnArre(103); }
        busy = true; filterOpen = false;
        document.getElementById('ivr-espera').textContent = excel
            ? 'Generando Excel completo. Esta operación puede demorar; por favor, espere.'
            : 'Consultando el resumen de IVA Ventas. Esta operación puede demorar; por favor, espere.';
        notify(''); sync();
        try {
            const response = await fetch(excel ? root.dataset.excel : root.dataset.consulta,
                { method: 'POST', body: data, credentials: 'same-origin', cache: 'no-store' });
            if (response.redirected) throw new Error('La sesión venció. Ingrese nuevamente.');
            if (!response.ok) throw new Error(await failure(response));
            const type = response.headers.get('Content-Type') || '';
            if (excel) {
                if (!type.includes('spreadsheetml.sheet')) throw new Error('No se recibió un archivo Excel válido.');
                const blob = await response.blob();
                const url = URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url; link.download = `Libro_IVA_Ventas_Resumen_${year}-${month}.xlsx`;
                document.body.appendChild(link); link.click(); link.remove();
                setTimeout(() => URL.revokeObjectURL(url), 60000);
                notify('Excel completo generado, con todas las sucursales y Total Libro.');
            } else {
                if (!type.includes('text/html')) throw new Error('La respuesta de consulta no es válida.');
                result.innerHTML = await response.text();
                const panel = result.querySelector('.ivr-panel');
                if (!panel) throw new Error('No se recibió la grilla del resumen.');
                applied = data; hasRows = Number(panel.dataset.count) > 0;
                if (hasRows) cargarReporteEnArre(103, { Periodo: String(year), Mes: month, Perfil: root.dataset.perfil, Menu: root.dataset.menu },
                    'Libro IVA Ventas - Resumen', '', '');
            }
        } catch (e) {
            if (!excel) clear('');
            notify(e instanceof TypeError ? 'No se pudo conectar con el servidor. Intente nuevamente.' : e.message, true);
        } finally { busy = false; sync(); }
    }
    form.addEventListener('submit', e => { e.preventDefault(); run(false); });
    form.addEventListener('reset', e => {
        if (busy) { e.preventDefault(); return; }
        setTimeout(() => clear(initial), 0);
    });
    form.addEventListener('change', () => { if (!busy) clear('Los filtros cambiaron. Presione Ver para consultar.'); });
    filterButton.addEventListener('click', () => { if (!busy && !unavailable) { filterOpen = !filterOpen; sync(); } });
    detailButton.addEventListener('click', () => { if (!busy && applied) { clear('Consulta cerrada. Se conservaron los filtros.'); filterButton.focus(); } });
    excelButton.addEventListener('click', () => run(true));
    printButton.addEventListener('click', () => {
        if (!busy && applied && hasRows) invocacionGestorDoc({ ...arrRepoParams[102], moduloGestor: 'IVA_VENTAS_RESUMEN', reportesPreseleccionados: [103] });
    });
    window.addEventListener('resize', layout);
    sync(); notify(initial);
})();
