(function (global) {
    'use strict';
    const dinero = n => '$ ' + Number(n || 0).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    function transporteHttp(root) {
        async function pedir(url, opciones, tiempo) {
            const abort = new AbortController(), timer = setTimeout(() => abort.abort(), tiempo);
            try {
                const response = await fetch(url, { credentials: 'same-origin', ...opciones, signal: abort.signal });
                const r = await response.json();
                if (!response.ok && r.ok === true) throw new Error('Respuesta de pago no válida. Verifique el resultado.');
                return r;
            } finally { clearTimeout(timer); }
        }
        return {
            opciones: solicitud => {
                const url = new URL(root.dataset.opcionesUrl, global.location.href);
                url.searchParams.set('insId', solicitud.ins_id); url.searchParams.set('coTipo', solicitud.co_tipo);
                return pedir(url, { headers: { Accept: 'application/json' } }, 15000);
            },
            procesar: solicitud => pedir(root.dataset.procesarUrl, {
                method: 'POST', headers: { Accept: 'application/json', 'Content-Type': 'application/json', RequestVerificationToken: root.querySelector('[name="__RequestVerificationToken"]')?.value || '' }, body: JSON.stringify(solicitud)
            }, 690000)
        };
    }
    function crearVista(root, transporte, alAprobar) {
        const q = id => root.querySelector('#' + id);
        const valor = (id, v) => { if (q(id).value !== String(v)) q(id).value = v; };
        let claves = '';
        const sesion = new global.CloverModelo.Sesion(transporte, s => {
            const bloqueado = !s.puedeEditar();
            ['txtMontoClover','cmbCuotaClover','chkQrClover','txtPaymentIdClover'].forEach(id => q(id).disabled = bloqueado);
            ['btnVolverClover','btnCerrarClover'].forEach(id => q(id).disabled = !s.puedeCerrar());
            q('formDetalleClover').setAttribute('aria-busy', String(s.estado === 'ESPERANDO' || s.estado === 'CARGANDO'));
            q('lblInstrumentoClover').textContent = s.contexto?.instrumento?.ins_desc || 'Clover';
            q('lblSaldoClover').textContent = dinero(s.contexto?.saldo);
            valor('txtMontoClover', s.base || ''); valor('txtPaymentIdClover', s.paymentid || '');
            q('chkQrClover').checked = !!s.qr;
            const firma = JSON.stringify([s.estado === 'CARGANDO', s.opciones]);
            if (firma !== claves) {
                claves = firma; q('cmbCuotaClover').replaceChildren();
                if (!s.opciones.length) {
                    const o = root.ownerDocument.createElement('option'); o.value = ''; o.textContent = s.estado === 'CARGANDO' ? 'Cargando opciones…' : 'Sin cuotas disponibles'; q('cmbCuotaClover').appendChild(o);
                }
                s.opciones.forEach(o => { const elemento = root.ownerDocument.createElement('option'); elemento.value = o.clave; elemento.textContent = `${o.pos_desc} (${o.recargo_porcentaje} %)`; q('cmbCuotaClover').appendChild(elemento); });
            }
            valor('cmbCuotaClover', s.plan || '');
            let valida = false;
            try { const c = s.cotizacion(); valor('txtRecargoClover', dinero(c.recargo)); valor('txtTotalClover', dinero(c.total)); valida = true; }
            catch (_) { valor('txtRecargoClover', '$ 0.00'); valor('txtTotalClover', '$ 0.00'); }
            q('btnPagarClover').disabled = bloqueado || !valida;
            if (s.estado === 'INCIERTO') q('txtPaymentIdClover').disabled = false;
            let idValido = false; try { global.CloverModelo.normalizarId(s.paymentid); idValido = true; } catch (_) { }
            // Tres caracteres nunca pueden alcanzar la llamada al SP entregado.
            q('btnManualClover').disabled = (bloqueado && s.estado !== 'INCIERTO') || !valida || !idValido;
            q('estadoClover').textContent = s.mensaje || 'Seleccione monto y cuotas.';
            q('estadoClover').className = 'alert mt-3 mb-0 ' + (s.estado === 'INCIERTO' ? 'alert-warning' : s.estado === 'APROBADO' ? 'alert-success' : ['RECHAZADO','BLOQUEADO'].includes(s.estado) ? 'alert-danger' : 'alert-light border');
            if (s.paymentid?.length === 3 && s.puedeEditar()) q('estadoClover').textContent = 'Por ahora, la recuperación requiere un ID de al menos 4 caracteres. Verifique el cupón.';
        }, async resultado => {
            const aplicado = await alAprobar(resultado);
            if (aplicado === true) { setTimeout(() => global.jQuery(root).modal('hide'), 0); }
            return aplicado;
        });
        q('formDetalleClover').addEventListener('submit', e => e.preventDefault());
        q('txtMontoClover').addEventListener('input', e => sesion.actualizar({ base: e.target.value }));
        q('cmbCuotaClover').addEventListener('change', e => sesion.actualizar({ plan: e.target.value }));
        q('chkQrClover').addEventListener('change', e => sesion.actualizar({ qr: e.target.checked }));
        q('txtPaymentIdClover').addEventListener('input', e => sesion.actualizar({ paymentid: e.target.value }));
        q('btnPagarClover').addEventListener('click', () => sesion.enviar('POS'));
        q('btnManualClover').addEventListener('click', () => sesion.enviar('MANUAL'));
        const cerrar = () => { if (sesion.cerrar()) global.jQuery(root).modal('hide'); };
        q('btnCerrarClover').addEventListener('click', cerrar); q('btnVolverClover').addEventListener('click', cerrar);
        global.jQuery(root).on('hide.bs.modal', e => { if (!sesion.puedeCerrar()) e.preventDefault(); });
        const orden = ['txtMontoClover','cmbCuotaClover','chkQrClover','txtPaymentIdClover','btnManualClover'];
        orden.slice(0,-1).forEach((id, i) => q(id).addEventListener('keydown', e => {
            if (e.key === 'Enter') { e.preventDefault(); const siguiente = q(orden[i+1]); if (!siguiente.disabled) { siguiente.focus(); siguiente.select?.(); } }
        }));
        return {
            sesion,
            abrir: contexto => { claves = ''; global.jQuery(root).modal('show'); const tarea = sesion.abrir(contexto); return tarea.then(r => { if (sesion.puedeEditar()) { q('txtMontoClover').focus(); q('txtMontoClover').select(); } return r; }); }
        };
    }
    let vista;
    function abrir(contexto) {
        const root = global.document.getElementById('modalDetalleClover');
        if (!root) throw new Error('La ventana Clover no está disponible. Recargue la página.');
        if (!vista) vista = crearVista(root, transporteHttp(root), resultado => {
            if (typeof vista.sesion.contexto?.alAprobar !== 'function') throw new Error('No está disponible el registro del pago aprobado. Conserve el ID.');
            return vista.sesion.contexto.alAprobar(resultado);
        });
        return vista.abrir(contexto);
    }
    function bloqueado() { return !!vista && ['ESPERANDO','APROBADO','INCIERTO'].includes(vista.sesion.estado); }
    global.CloverPago = Object.freeze({ abrir, bloqueado, esInstrumento: global.CloverModelo.esInstrumento, crearVista });
})(window);
