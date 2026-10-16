(() => {
    'use strict';
    const root = document.getElementById('iva-cierre');
    if (!root) return;
    const byId = id => document.getElementById(id);
    const result = byId('ivc-resultado'), content = byId('ivc-contenido'), message = byId('ivc-mensaje');
    const refresh = byId('ivc-actualizar'), close = byId('ivc-cerrar'), download = byId('ivc-rg');
    const modalEl = byId('ivc-confirmacion'), confirm = byId('ivc-confirmar');
    const available = root.dataset.disponible === 'true';
    let busy = false, confirming = false, next = '', canClose = false, selected = null, expected = '', framePending = false;
    const periodText = p => /^\d{6}$/.test(p) ? p.slice(0, 4) + '-' + p.slice(4) : '';
    function notify(text, tone = 'info') { message.textContent = text; message.className = text ? `alert alert-${tone}` : ''; }
    function rows() { return Array.from(result.querySelectorAll('tr[data-clave]')); }
    function sync() {
        refresh.disabled = !available || busy || confirming;
        close.disabled = !available || busy || confirming || !canClose || !next;
        download.disabled = !available || busy || confirming || !selected;
        close.innerHTML = '<i class="bx bx-lock-alt" aria-hidden="true"></i> ' + (next ? 'Cerrar IVA ' + periodText(next) : 'Cerrar IVA');
        confirm.disabled = busy;
        byId('ivc-cancelar').disabled = busy;
        byId('ivc-esperando').hidden = !busy;
        result.inert = busy || confirming;
        result.setAttribute('aria-busy', String(busy));
        layout();
    }
    function layout() {
        if (framePending) return;
        framePending = true;
        requestAnimationFrame(() => {
            framePending = false;
            const footer = document.querySelector('body > footer');
            content.style.height = `${Math.max(290, Math.floor(window.innerHeight - content.getBoundingClientRect().top - (footer?.getBoundingClientRect().height || 0) - 65))}px`;
        });
    }
    function select(row) {
        if (busy || confirming || !row) return;
        selected = { key: row.dataset.clave, period: row.dataset.periodo };
        rows().forEach(r => { const active = r === row; r.classList.toggle('is-selected', active); r.querySelector('button').setAttribute('aria-pressed', String(active)); });
        byId('ivc-seleccion').textContent = 'Período seleccionado para RG 3685: ' + periodText(selected.period);
        sync();
    }
    function data(period) {
        const body = new FormData(byId('ivc-token'));
        if (period) body.append('Periodo', period);
        return body;
    }
    async function request(url, body) {
        const response = await fetch(url, { method: 'POST', credentials: 'same-origin', cache: 'no-store', body });
        if (response.redirected) throw new Error('La sesión venció. Ingrese nuevamente.');
        if (!response.ok) {
            let text = [401, 403].includes(response.status) ? 'La sesión venció o el perfil no tiene permiso para esta operación.' : 'No se pudo completar la operación.';
            try { const json = await response.json(); if (typeof json.mensaje === 'string') text = json.mensaje; } catch { }
            throw new Error(text);
        }
        return response;
    }
    function setBusy(text) { busy = true; byId('ivc-espera-texto').textContent = text; sync(); }
    async function load(preferredKey) {
        const previous = preferredKey || selected?.key;
        selected = null; next = ''; canClose = false; result.replaceChildren(); sync();
        const response = await request(root.dataset.estado, data());
        if (!(response.headers.get('Content-Type') || '').includes('text/html')) throw new Error('No se recibió un historial válido.');
        result.innerHTML = await response.text();
        const panel = result.querySelector('.ivc-panel');
        if (!panel || !/^\d{6}$/.test(panel.dataset.proximo)) { result.replaceChildren(); throw new Error('La respuesta no contiene el próximo período válido.'); }
        next = panel.dataset.proximo; canClose = panel.dataset.puedeCerrar === 'true';
        const row = rows().find(r => r.dataset.clave === previous);
        // La carga se ejecuta ocupada: restaurar únicamente la selección previa del mismo registro.
        if (row) {
            selected = { key: row.dataset.clave, period: row.dataset.periodo };
            row.classList.add('is-selected'); row.querySelector('button').setAttribute('aria-pressed', 'true');
            byId('ivc-seleccion').textContent = 'Período seleccionado para RG 3685: ' + periodText(selected.period);
        }
    }
    async function reload() {
        if (busy || confirming || !available) return;
        setBusy('Consultando el historial de cierres. Esta operación puede demorar; por favor, espere.'); notify('');
        try { await load(); }
        catch (e) { notify(e instanceof TypeError ? 'No se pudo conectar. Presioná Actualizar para intentar nuevamente.' : e.message, 'warning'); }
        finally { busy = false; sync(); }
    }
    async function performClose() {
        if (busy || !confirming || !expected) return;
        const period = expected; // Nunca cambiar al nuevo período después de una confirmación vieja.
        setBusy('Registrando el cierre del período ' + periodText(period) + '. Esperá la respuesta antes de volver a operar.');
        bootstrap.Modal.getInstance(modalEl).hide(); notify('');
        let info, tone;
        try {
            const response = await request(root.dataset.cerrar, data(period));
            const json = await response.json();
            if (typeof json.resultado !== 'number' || typeof json.resultado_msj !== 'string') throw new Error('No se recibió una confirmación válida del cierre.');
            tone = json.resultado === 0 ? 'success' : 'warning';
            info = json.resultado === 0 ? 'Cierre del período ' + periodText(period) + ' registrado correctamente.' : json.resultado_msj;
            try { await load(json.resultado === 0 ? '2:' + period : selected?.key); }
            catch { info += ' No se pudo actualizar el historial. Presioná Actualizar antes de continuar.'; }
        } catch (e) {
            // No reintentar escrituras. Invalidar acciones hasta obtener otra consulta explícita.
            next = ''; canClose = false; selected = null; result.replaceChildren();
            info = (e instanceof TypeError ? 'Se interrumpió la conexión y no se pudo confirmar el resultado del cierre.' : e.message) + ' Presioná Actualizar para comprobar el estado antes de volver a cerrar.';
            tone = 'warning';
        } finally { busy = false; confirming = false; expected = ''; sync(); notify(info, tone); }
    }
    async function exportRg() {
        if (busy || confirming || !available || !selected) return;
        const period = selected.period;
        setBusy('Generando cabecera y detalle RG 3685 del período ' + periodText(period) + '. La descarga comenzará cuando ambos archivos estén listos.'); notify('');
        try {
            const response = await request(root.dataset.descarga, data(period));
            if (!(response.headers.get('Content-Type') || '').includes('application/zip')) throw new Error('No se recibió un archivo ZIP válido.');
            const blob = await response.blob(), url = URL.createObjectURL(blob), a = document.createElement('a');
            a.href = url; a.download = 'IVA_Ventas_3685_' + periodText(period) + '.zip'; document.body.appendChild(a); a.click(); a.remove();
            setTimeout(() => URL.revokeObjectURL(url), 60000);
            notify('Descarga preparada: cabecera y detalle del período ' + periodText(period) + '.', 'success');
        } catch (e) { notify(e instanceof TypeError ? 'No se pudo completar la descarga. Verificá la conexión e intentá nuevamente.' : e.message, 'warning'); }
        finally { busy = false; sync(); }
    }
    result.addEventListener('click', e => { const row = e.target.closest('tr[data-clave]'); if (row && result.contains(row)) select(row); });
    result.addEventListener('keydown', e => {
        if (busy || confirming || !['ArrowUp', 'ArrowDown'].includes(e.key) || !e.target.closest('.ivc-select')) return;
        const list = rows(), current = list.indexOf(e.target.closest('tr'));
        const row = list[Math.max(0, Math.min(list.length - 1, current + (e.key === 'ArrowDown' ? 1 : -1)))];
        e.preventDefault(); select(row); row?.querySelector('button').focus();
    });
    refresh.addEventListener('click', reload); download.addEventListener('click', exportRg);
    close.addEventListener('click', () => {
        if (busy || confirming || !canClose || !next) return;
        expected = next; confirming = true;
        byId('ivc-periodo-confirmar').textContent = periodText(expected); sync();
        bootstrap.Modal.getOrCreateInstance(modalEl).show();
    });
    modalEl.addEventListener('hidden.bs.modal', () => { if (!busy) { confirming = false; expected = ''; sync(); close.focus(); } });
    modalEl.addEventListener('shown.bs.modal', () => byId('ivc-cancelar').focus());
    confirm.addEventListener('click', performClose);
    window.addEventListener('resize', layout); sync();
    if (available) reload();
})();
