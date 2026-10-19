(() => {
    'use strict';
    const root = document.getElementById('iva-compras');
    if (!root) return;
    const form = document.getElementById('iva-filtros');
    const result = document.getElementById('iva-resultado');
    const message = document.getElementById('iva-mensaje');
    const outer = form.querySelector('fieldset');
    const filterPanel = document.getElementById('divFiltro');
    const detailPanel = document.getElementById('divDetalle');
    const filterButton = document.getElementById('btnFiltro');
    const detailButton = document.getElementById('btnDetalle');
    const exportButton = document.getElementById('iva-exportar');
    const loading = document.getElementById('iva-cargando');
    const loadingText = document.getElementById('iva-cargando-texto');
    const unavailable = outer.disabled;
    let busy = false;
    let applied = null;
    let filterOpen = true;
    let hasRecords = false;
    let layoutPending = false;
    const initialMessage = 'Seleccione el período y presione Ver para consultar.';

    // Control local: sin data-bs-toggle ni eventos globales de los ABM.
    // Ocultar el filtro no descarta la consulta; cerrar Detalle sí la descarta.
    function syncPanels() {
        form.hidden = !filterOpen;
        filterPanel.hidden = !filterOpen;
        detailPanel.hidden = filterOpen || (!applied && !busy);
        filterButton.disabled = unavailable || busy;
        detailButton.disabled = unavailable || busy || !applied;
        exportButton.disabled = unavailable || busy || !applied || !hasRecords;
        loading.hidden = !busy;
        filterButton.setAttribute('aria-expanded', String(filterOpen));
        detailButton.setAttribute('aria-expanded', String(!detailPanel.hidden));
        scheduleLayout();
    }
    // Medir solamente al cambiar paneles/tamaño: sin polling ni altura fija de 65vh.
    // Se reserva el pie del sitio y el paginador queda fuera del scroll de filas.
    function scheduleLayout() {
        if (layoutPending) return;
        layoutPending = true;
        window.requestAnimationFrame(() => {
            layoutPending = false;
            if (detailPanel.hidden) return;
            const top = detailPanel.getBoundingClientRect().top + window.scrollY;
            const footer = document.querySelector('body > footer');
            const footerHeight = footer ? footer.getBoundingClientRect().height : 0;
            const height = Math.max(260, Math.floor(window.innerHeight - top - footerHeight - 16));
            const value = `${height}px`;
            if (detailPanel.style.height !== value) detailPanel.style.height = value;
        });
    }
    function clearQuery(text) {
        applied = null;
        hasRecords = false;
        result.replaceChildren();
        filterOpen = true;
        syncPanels();
        notify(text);
    }

    function notify(text, error = false) {
        message.textContent = text;
        message.className = 'iva-message' + (text ? (error ? ' alert alert-warning' : ' alert alert-info') : '');
    }
    function dates() {
        const year = Number(form.elements.Periodo.value);
        const month = Number(form.elements.Mes.value);
        const last = new Date(year, month, 0).getDate();
        const prefix = `${year}-${String(month).padStart(2, '0')}-`;
        [form.elements.Desde, form.elements.Hasta].forEach((el, i) => {
            el.min = prefix + '01';
            el.max = prefix + String(last);
            if (!el.value || el.value < el.min || el.value > el.max) el.value = i === 0 ? el.min : el.max;
        });
    }
    function syncToggles() {
        root.querySelectorAll('[data-enable]').forEach(el => {
            document.getElementById(el.dataset.enable).disabled = !el.checked;
        });
    }
    function valid() {
        form.elements.Hasta.setCustomValidity('');
        if (form.elements.Fechas.checked && form.elements.Desde.value > form.elements.Hasta.value)
            form.elements.Hasta.setCustomValidity('Hasta debe ser igual o posterior a Desde.');
        // Los controles inválidos deben estar visibles antes de enfocarlos.
        if (!form.checkValidity()) {
            filterOpen = true;
            syncPanels();
            return form.reportValidity();
        }
        return true;
    }
    async function failure(response) {
        if (response.status === 401 || response.status === 403) return 'La sesión venció o el perfil no tiene permiso. Ingrese nuevamente.';
        try {
            const data = await response.json();
            if (typeof data.mensaje === 'string') return data.mensaje;
        } catch { /* Un proxy o IIS puede devolver HTML: no mostrarlo al usuario. */ }
        return 'No se pudo completar la operación. Verifique la conexión e intente nuevamente.';
    }
    async function run(exporting, page = 1, paging = false) {
        if (busy || outer.disabled) return;
        if (exporting && (!applied || !hasRecords)) return;
        if (!valid()) return;
        // Capturar antes de deshabilitar; FormData excluye controles deshabilitados.
        const useApplied = applied && (exporting || paging);
        const data = useApplied ? new FormData() : new FormData(form);
        if (useApplied) applied.forEach((v, k) => data.append(k, v));
        data.set('Pagina', String(exporting ? 1 : page));
        const year = data.get('Periodo');
        const month = String(data.get('Mes')).padStart(2, '0');
        busy = true;
        outer.disabled = true;
        filterOpen = false;
        loadingText.textContent = exporting
            ? 'Generando Excel con todos los registros. Puede demorar según el volumen; no cierre esta pantalla.'
            : 'Consultando comprobantes… Esta operación puede demorar. Por favor, espere.';
        notify('');
        syncPanels();
        result.setAttribute('aria-busy', 'true');
        const pager = [...result.querySelectorAll('button,input')].map(b => [b, b.disabled]);
        pager.forEach(([b]) => { b.disabled = true; });
        try {
            const response = await fetch(exporting ? root.dataset.excel : root.dataset.consulta, {
                method: 'POST', body: data, credentials: 'same-origin', cache: 'no-store'
            });
            if (response.redirected) throw new Error('La sesión venció. Ingrese nuevamente.');
            if (!response.ok) throw new Error(await failure(response));
            const type = response.headers.get('Content-Type') || '';
            if (exporting) {
                if (!type.includes('spreadsheetml.sheet')) throw new Error('No se recibió un archivo Excel válido.');
                const blob = await response.blob();
                const url = URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = `Libro_IVA_Compras_${year}-${month}.xlsx`;
                document.body.appendChild(link);
                link.click();
                link.remove();
                setTimeout(() => URL.revokeObjectURL(url), 60000);
                notify('Excel completo generado. La descarga contiene todos los registros de los filtros seleccionados.');
            } else {
                if (!type.includes('text/html')) throw new Error('La respuesta de consulta no es válida.');
                result.innerHTML = await response.text();
                applied = data;
                hasRecords = Number(result.querySelector('.iva-datos')?.dataset.total) > 0;
                filterOpen = false;
                notify('');
            }
        } catch (e) {
            if (!exporting) clearQuery('');
            notify(e instanceof TypeError ? 'No se pudo conectar con el servidor. Intente nuevamente.' : e.message, true);
        } finally {
            busy = false;
            outer.disabled = unavailable;
            result.removeAttribute('aria-busy');
            pager.forEach(([b, disabled]) => { b.disabled = disabled; });
            syncPanels();
            // El HTML anterior se reemplazó: devolver el foco a la página confirmada.
            if (paging && applied) result.querySelector('[aria-current="page"]')?.focus({ preventScroll: true });
        }
    }
    filterButton.addEventListener('click', () => {
        if (busy || unavailable) return;
        filterOpen = !filterOpen;
        syncPanels();
    });
    detailButton.addEventListener('click', () => {
        if (busy || unavailable || !applied) return;
        clearQuery('Consulta cerrada. Se conservaron los filtros; presione Ver para consultar nuevamente.');
        filterButton.focus();
    });
    form.addEventListener('submit', e => { e.preventDefault(); run(false); });
    exportButton.addEventListener('click', () => run(true));
    result.addEventListener('gc:page', e => {
        const page = e.detail?.page;
        const pager = result.querySelector('[data-gc-pager]');
        if (!busy && applied && pager && Number.isSafeInteger(page) && page >= 1 &&
            page <= Number(pager.dataset.pages) && page !== Number(pager.dataset.current)) run(false, page, true);
    });
    form.addEventListener('input', () => {
        if (busy) return;
        form.elements.Hasta.setCustomValidity('');
        clearQuery('Los filtros cambiaron. Presione Ver para actualizar la consulta.');
    });
    form.addEventListener('change', e => {
        if (busy) return;
        syncToggles();
        if (e.target.name === 'Periodo' || e.target.name === 'Mes') dates();
        clearQuery('Los filtros cambiaron. Presione Ver para actualizar la consulta.');
    });
    form.addEventListener('reset', e => {
        if (busy) { e.preventDefault(); return; }
        // El evento reset se dispara antes de que el navegador restaure los valores.
        setTimeout(() => {
            syncToggles(); dates();
            form.elements.Hasta.setCustomValidity('');
            clearQuery(initialMessage);
        }, 0);
    });
    window.addEventListener('resize', scheduleLayout);
    if (typeof ResizeObserver !== 'undefined') {
        const observer = new ResizeObserver(scheduleLayout);
        observer.observe(root);
        const footer = document.querySelector('body > footer');
        if (footer) observer.observe(footer);
    }
    syncToggles(); dates(); syncPanels();
    notify(initialMessage);
})();
